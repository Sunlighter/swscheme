The core of this Scheme implementation is an Expression Object Model -- an instance of the Interpreter pattern from the book _Design Patterns_ by Gamma, Helm, Johnson, and Vlissides. [http://en.wikipedia.org/wiki/Design_Patterns](http://en.wikipedia.org/wiki/Design_Patterns).

Scheme has a number of expression types. For each expression type, there is a class that implements the {{IExpression}} interface. The most basic expression types are:

* Literal (or _constant_)
* Variable reference
* Variable set!
* Begin
* If-Then-Else (or just _If_)
* Lambda
* Invocation (or _function call_)

The structure of the implementation follows from the implementation of {{IExpression}} and other related interfaces.

For example, you might start with something like this:

{{
public interface IExpression
{
    object Eval();
}
}}

However, that does not provide an environment for variable references, variable sets, or variables created by lambda. So you need an environment.

{{
public interface IExpression
{
    object Eval(Env env);
}
}}

An environment could map symbols directly to values, but that would mean that lambda would capture variables by value. In Scheme, lambda captures variables by reference. So, it makes more sense for the environment to map symbols to boxes. A box has a mutable value. Multiple environments can refer to the same box.

A lambda needs to create a procedure, and it is also useful to create procedures for primitive functions.

{{
public interface IProcedure
{
    object Call(FList<object> args);
}
}}

With the above {{IExpression}} interface, a Lambda cannot look into its body and see which variables it uses, so a Lambda expression has to capture the entire environment when creating the lambda procedure. It is more optimal if a Lambda expression captures only the variables it actually needs. So we need to modify the {{IExpression}} interface to support that:

{{
public interface IExpression
{
    EnvSpec GetEnvSpec();
    object Eval(Env env);
}
}}

{{EnvSpec}} is a set of symbols, and every expression must now return the set of symbols which it uses. Now a Lambda expression can inspect its body.

It is somewhat slow to use mappings from symbols to boxes. It is faster to map integers to boxes; then we can represent environments as arrays of boxes. This necessitates a "pre-treatment" step, which means splitting {{IExpression}} into two interfaces.

{{
public interface IExpressionSource
{
    EnvSpec GetEnvSpec();
    IExpression Compile(EnvDesc envDesc);
}

public interface IExpression
{
    object Eval(Env env);
}
}}

An {{IExpressionSource}} uses symbols to refer to variables. An {{EnvDesc}}, or environment description, maps those symbols to their integer locations in the environment. The {{Compile}} function can recursively compile any subexpressions.

These interfaces don't support continuations yet.

Scheme uses recursion to express iteration. This means that a simple loop will pile up a lot of activation records on the stack. To prevent stack overflows from limiting the number of iterations in a loop, it is necessary to perform tail calls.

C# does not support tail calls, so to compensate for this, two things must be done.

First, "continuation passing style" has to be used. Continuations have to be broken out and treated explicitly. So an {{IContinuation}} interface is defined. The main thing you can do with a continuation is return a value to it.

Every expression evaluation, and every function call, is passed a continuation. An expression or procedure can perform a tail call (or tail eval) by passing along the same continuation it received. Or it can use the continuation to return a value. Or it can construct a new continuation.

However, we cannot implement continuations like this:

{{
public interface IContinuation
{
    object Return(object val);
}
}}

It is necessary to create one final interface:

{{
public interface IRunnableStep
{
    IRunnableStep Run();
}
}}

This is the idea of "run and return successor."

We modify the interfaces like this (the {{IExpressionSource}} interface does not change):

{{
public interface IExpression
{
    IRunnableStep Eval(Env env, IContinuation k);
}

public interface IProcedure
{
    IRunnableStep Call(FList<object> args, IContinuation k);
}

public interface IContinuation
{
    IRunnableStep Return(object val);
}
}}

Then we create:

* A runnable step which will call {{Eval}} on an {{IExpression}} given an {{Env}} and an {{IContinuation}}.
* A runnable step which will call {{Call}} on an {{IProcedure}} given an {{FList<object>}} and an {{IContinuation}}.
* A runnable step which will call {{Return}} on an {{IContinuation}} given an {{object}} being returned.

Instances of these three runnable step types can be constructed and returned by the expression and procedure classes.

* A "final runnable step." We can use a while loop to keep replacing any runnable step with its successor, until we get the final runnable step.
* A "final continuation." When a value is returned to this continuation, it returns the final runnable step.

At that point, it is possible to create function that can evaluate an expression in an environment and return the result, simply by creating a final continuation, passing it to Eval to get an initial runnable step, then replacing the runnable step with its successor until it gets to the final runnable step.

The {{IRunnableStep}} implementation represents the entire state of the computation.