The object system is planned and will work as follows:

* Based on message passing
	* Asynchronous (the function to post a message causes the message to be queued and returns immediately)
	* Unidirectional
		* This allows for transparent forwarding, routing, and delegation
		* Analogous to tail-calls or continuation passing style
	* Reliable (once a message is queued, delivery is guaranteed, unless the target object enters an infinite loop, or all the worker threads are trapped in infinite loops)
	* Ordered (messages are delivered in the order posted)
* Not everything is an object; the overhead would be too high
* There are custom types for messages and signatures
* Each user-defined object will have Scheme functions to handle messages
* Each object can process only one message at a time, but different objects can process messages on different threads at the same time
* Scheme functions can add, change, and remove the handlers attached to a user-defined object; data members can be created and destroyed dynamically as well
* There are no classes (but you can write a Scheme function to automate the process of creating a blank object and then filling it with data members and message handlers)
* It will also be possible to create system-defined objects such as graphics windows
	* System-defined objects will only respond to specific messages
	* System-defined objects will not have accessible data members

It is planned that most of the disposable objects with asynchronous functions, such as sockets, will be replaced with system-defined objects in this object system.

The object system will run in parallel with the interpreter, and it will be possible to use the interpreter to modify objects _while they are running._ For example, you'll be able to create a UDP socket, tell it to post a message to an object of yours when it receives a packet, and then alter your object on the fly to have it post messages to itself or other objects based on the contents of the received UDP packets. You will be able to send a UDP packet by posting a special message to the UDP socket.