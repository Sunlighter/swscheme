**invert-server.scm** demonstrates a TCP client / server application. In this example, the client can send a byte array to the server. The server inverts all the bits in the byte array and sends it back. You have to run two instances of Scheme, one to act as the client, and one to act as the server.

On the interpreter that you want to act as server, load the invert-server source code, then evaluate something like
{{
  (define server (begin-server (ipaddr 'any) 9999))
}}
Then you can query the status with:
{{
  (get-status server)
}}
And you can shut down the server with:
{{
  (stop server)
}}
On the interpreter that you want to act as a client, load the source code, then evaluate something like
{{
  (define socket (connect (ipaddr 'loopback) 9999))
}}
(Warning: I have encountered Windows machines that don't have a loopback address, on such a machine the address returned by (ipaddr 'loopback) won't work.)

Once you have the socket, you can create byte arrays with the random-bytes function, and invert them:
{{
  (define b1 (random-bytes 100))
  (dump (byterange b1))
  (define b2 (invert-via socket b1))
  (dump (byterange b2))
}}
To close the client, just use
{{
  (dispose! socket)
}}
Piece of cake.