**Codeplex Shutdown** see [NextSteps](NextSteps)

**News** see [LatestNews](LatestNews)

**Latest Release** is [release:50975](release_50975).

**Source Code** is still being updated as of Feb 2013.

**Project Description**
Sunlit World Scheme is a nearly R4RS-compliant Scheme implementation that supports threading, TCP, UDP, cryptography, and simple graphics and windowing. It's designed to be easy to extend. It also has an embedded compiler (for a different language) that produces MSIL.

Features:

* Scheme in .NET, with .NET features
* XCOPY deployment
* Partial standard compliance (aiming for more)
	* Proper tail calls
	* Call/cc with unlimited extent
	* Arbitrary precision integers, arbitrary-precision rationals, double-precision floats
	* Mutable strings
	* No complex numbers
	* No "do" syntax
	* No hygienic macros
	* eval works in a non-standard way
* Pre-treatment for faster execution
* Dynamic variables
* Dynamic wind
* Partial continuations
* Procedures are serializable (except when created from delegates or Pascalesque)
* generate-exe (via serialized procedures)
* throw and catch
* Disposable object tracking (but no gc for disposable objects)
* Asynchronous I/O
* Threading (a few functions may not be thread-safe)
* A new object system based on the actor model
* Low-level windowing
* Bitmaps and graphics
* Binary files
* **New in Mercurial repository!** Codec generator (i.e., serializer generator, pickler combinators)
* Byte arrays
* Sets and maps
* 3-D vectors, vertices, lines, planes, and convex hulls
* TCP and UDP support, client and server modes
* "Pascalesque" compiler to MSIL byte code (works, but still in development)
	* Mis-named; actually more like Scheme than Pascal
	* Strongly typed
	* Proper tail calls
	* No call/cc
	* Lambda, let, let*, letrec, and let loop
	* "While" syntax allows iteration without the overhead of lambda
	* Unsafe code and pinning of arrays
	* Parallel For

Some [Documentation](Documentation) is on this Wiki. There is also an XML documentation file in the source code and the download; you can browse it at [http://swscheme.s3.amazonaws.com/swscheme2.xml](http://swscheme.s3.amazonaws.com/swscheme2.xml). The two sources of documentation complement each other and do not overlap.

Latest binary release: [release:50975](release_50975). The binaries in the release download are in the SchemeCode subdirectory.

It's probably better to get the source code and build it; the features are better! (I don't yet have an automated process to create a binary release, so I don't do it very often...)

[FutureDirections](FutureDirections) are being considered. Currently, development is concentrating on Pascalesque, 3-D math, and the object system.