Here are some things I am considering adding to this Scheme. I cannot guarantee that I will add them.

## Later

* Ability to get geometry for convex hulls so they can be drawn
* Support for floating-point vectors and floating-point quaternions
* Support for a non-parallel {{For}} in Pascalesque (although while can be used for this)
* Support for char and string and StringBuilder in Pascalesque
* Support for try-finally and using in Pascalesque
* DateTime and TimeSpan support
* HttpListener and HttpClient
* SqlConnection, SqlCommand, SqlParameter, SqlDataReader (possibly OleDb and Odbc as well)
* VirtualAlloc from Pascalesque
* Native code function generation (giving access to SSE, SSE2, AVX, maybe even the legacy FPU)
	* I will prefer 64-bit over 32-bit
	* This will probably involve yet another language
* Better support for enumerations. Using reflection for them is a real pain.

## Blue Sky

* A full-screen OpenGL mode.
* Amazon S3 / SQS / SNS support
	* I want to avoid awssdk.dll, and code straight to the REST API
	* I also want credentials in the registry so I don't accidentally include my S3 password in the distribution :P
	* Or the credentials could be encrypted, so that if I did distribute them, no one could get in.
* Ability to use Pascalesque to generate entire stand-alone DLLs and EXEs (with no dependency on ExprObjModel.dll)
	* Use this version of Sunlit World Scheme to generate the next one

I also want to write more documentation.

# How You Can Help

I am currently in need of:

* Feature Requests
* Documentation Requests
* Bug Reports
* Success or Failure Stories