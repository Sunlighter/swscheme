Sunlit World Scheme can be easily extended. There are a few different ways to do this.

The easiest way to extend Sunlit World Scheme is to add a function.

# SchemeFunctionAttribute

The easiest way to add a function is to write the function in C#, place it in the **ExprObjModel.dll** source code, and decorate it with the {{SchemeFunctionAttribute}}. Most of the implementation's functions already use this attribute.

The {{SchemeFunctionAttribute}} takes one parameter, the name you want the function to have in the Scheme interpreter. The name must be unique. The interpreter automatically reflects over itself at startup, and makes all the attributed functions available.

* If you decorate a class's constructor, you get a Scheme function that takes the same parameters as the constructor and returns the new object.
* If you decorate a static method or an operator, you get a Scheme function that takes the same parameters as the method or operator and returns the same return value.
* If you decorate an instance method, you get a Scheme function that takes the instance as the first parameter and then takes the same parameters as the method. It returns the same value.
* You cannot decorate a property, but you can decorate the "get" and "set" methods of the property.
	* The get and set methods of a static property won't take any parameters.
	* The get and set methods of an instance property will take one parameter, the object instance.
	* A static indexer will take the same parameters.
	* An instance indexer will take the instance as the first parameter and then take the same parameters.

Parameters and return values are marshalled between Scheme and C#.

* "ref" and "out" parameters are not supported.
* Parameters with the "params" attribute are not supported.
* Integral types are converted back and forth to BigInteger. Internally, Scheme does not use any integral type except BigInteger.
* System.Float, when returned, is converted to System.Double.
* Parameters of type System.Float or System.Double can receive any Scheme numeric type.
* Parameters and return values of type Object are ignored by the marshalling mechanism. This allows you to receive or return any Scheme type, or bypass the marshalling mechanism if you need to.
* Disposable types are converted back and forth to DisposableIDs.
* Functions that return void will map to Scheme functions that return the unspecified value.

A parameter of type {{IGlobalState}} is treated specially. Scheme will provide the real global state as the argument for this parameter. The parameter will not appear in the signature of the Scheme function. You can have only one parameter of type {{IGlobalState}}.

The main usefulness of {{IGlobalState}} is to convert an {{IAsyncResult}} into an {{AsyncID}}.

Function overloading -- the creation of a Scheme function that maps to two or more implementations depending on the types of the arguments -- is not supported. However, you can apply the {{SchemeFunctionAttribute}} to two C# functions that have the same names but different signatures. You have to give them different Scheme names.

# SchemeIsAFunctionAttribute

This attribute can be applied to a class or interface. It takes one argument, the name you want the Scheme function to have.

The generated Scheme function always takes one argument of any type. It returns {{#t}} if the argument is an instance of the class or implements the interface to which the {{SchemeIsAFunctionAttribute}} was applied. Otherwise it returns {{#f}}.

# IProcedure

A more complex but more powerful way to add a function to Scheme is to implement the {{IProcedure}} interface. The easiest way to do this is to create a class which implements the interface and has a default constructor. Decorate it with the {{SchemeSingletonAttribute}}. This attribute takes a name parameter, which must be unique.

The {{IProcedure}} interface is fairly simple. It has two properties and a method.

* The {{Arity}} property indicates the minimum number of arguments the procedure takes.
* The {{More}} property is true if the procedure can take more than the minimum number of arguments, and false if the minimum is also the maximum.
* The {{Call}} method takes an {{IGlobalState}}, an {{FList}} of arguments, and a continuation. The {{Call}} function must return an {{IRunnableStep}}.
	* {{FList}} is just a singly-linked list with immutable nodes.

Parameters and return types are somewhat restricted in Sunlit World Scheme. The best way to see what types are preferred is to go to {{SchemeWrite.cs}} and look at the {{WriteItem}} function. You generally don't want to see {{#< NULL >}}, and {{#<object of type ...>}} may not be acceptable in some cases (such as math).

The most common reason to use {{IProcedure}} is that you want to write a function that accepts different numbers or types of parameters. For example, you might want to implement a function that takes an optional argument, or a function that can take a byte array or a string.

In this case, the best thing to do is:

* Extract and check your arguments, and throw a {{SchemeRuntimeException}} or other exception if the arguments are incorrect.
* Perform your operation.
* Construct a {{RunnableReturn}} taking the continuation you received, and the value you would like to return.

If you want to throw an exception, consider returning a {{RunnableThrow}} instead. (Unfortunately my own code is inconsistent about this.)

## Tail-Calling

Sunlit World Scheme does not support multiple return values. However, it is possible for your procedure to accept another procedure, and tail-call it with arguments.

In this case, instead of a {{RunnableReturn}}, you create a {{RunnableCall}}, passing it the procedure, the parameters, and the same continuation that your function received.

## Non-Tail-Calling

In some cases you may want to call another Scheme procedure and receive a result. This is complicated because you have to create your own implementation of {{IContinuation}}. You create a new instance of your new continuation type, then call the procedure, passing it the new continuation rather than the one you received. The procedure may return to your new continuation, may throw an exception into it, or may skip it altogether using call/cc.

It is also possible that your continuation may be used more than once. For this reason, it is best if your continuation is immutable.

{{IContinuation}} is fairly complicated to implement.

Your continuation will almost always have a "parent" continuation, which is the continuation that your function received. By convention, this is stored in a field named {{k}}.

* The {{Return}} function receives a value from a computation. You can take that value and use it to return a value to your own continuation. You can also call or tail-call another procedure.
* The {{Throw}} function receives an exception from a computation. The usual way to deal with it is to call {{Throw}} on the parent continuation, with the same exception. However, you may want to deal with the exception in some other way.
* There are some functions to support {{dynamic-wind}} functionality.
	* The {{Parent}} attribute returns the parent continuation.
	* The {{EntryProc}} attribute returns a procedure of zero arguments which must be executed before entering this continuation. Most continuation types return {{null}}.
	* The {{ExitProc}} attribute returns a procedure of zero arguments which must be executed after exiting this continuation. Most continuation types return {{null}}.
* There is a function to support partial continuations.
	* The {{PartialCapture}} function is described below.
* There are functions to support dynamic variables.
	* The {{DynamicLookup}} function gets the location of a dynamic variable. Most continuations simply call {{DynamicLookup}} on their parent continuations.
	* The {{DynamicEnv}} function gets the set of all the currently defined dynamic variables. Most continuations simply call {{DynamicEnv}} on their parent continuations.

A partial continuation implements the {{IPartialContinuation}} interface. When you create a continuation that works with partial continuations, you have to create a partial continuation class also. This partial continuation class has the same fields as the regular continuation, except that it has partial continuation fields instead of continuation fields.

{{PartialCapture}} converts a continuation to a partial continuation. The partial continuation has only one function, {{Attach}}, which converts the partial continuation back to a regular continuation again.

The implementation of {{PartialCapture}} is usually one line of code. It uses the {{ItemAssociation}} to return the partial continuation that corresponds to {{this}}. The {{Assoc}} function takes {{this}} and a delegate which will construct and return the partial continuation. The delegate is called only if the partial continuation doesn't already exist. The delegate can call {{PartialCapture}} to convert the parent continuation, and any others, into partial continuations.

The implementation of {{Attach}} is usually the same line of code, but working in the opposite direction.

