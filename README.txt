JakeSays:
   By reworking the way the client is making requests I was able to get it to work w/o the OE exception occurring.
     Look at SimpleClient.GetContentStream2().
   The issue has to do with the HttpClient and HttpRequest instances being disposed before the response was read.
   I was able to run to ~16300 requests before it failed, but the failure is due to running out of port instances
     because the client isn't closing the socket. Not sure how that is supposed to happen given the insane complexity
     of the whole HttpClient nonsense.
   By adding the ConnectionClose: true header to the request the server closes the socket after sending, and 
     I was able to achieve well over 100k requests, however the app is consuming memory like crazy. I suspect
     it is because something isn't being cleaned up correctly. Apparently this is a known issue in 4.8.1 that
     MS isn't going to fix.     

REQUIREMENTS
   * Either start Visual Studio as admin or make sure an UrlAcl exists for Program.UrlPrefix
   * Either change Program.UrlPrefix to "http" or make sure a valid(!) SSL certificate is registered for the UrlPrefix's port

STRUCTURE
   * Program.cs coordinates client and server and provides a way to cancel all operations
   * SimpleClient is a minimal client that will return the server's response content (the .docx) as a MemoryStream
   * SimpleServer fires up a simple HttpListener-based server that will deliver a hardcoded .docx file on any request
   * There are two projects:
      * ObjectDisposedReproDotNet48    (.NET Framework 4.8 console application)
      * ObjectDisposedReproNet80       (.NET 8.0 console application)
     Both share the exact same code, ObjectDisposedReproNet80 has all source files added as-link from ObjectDisposedReproDotNet48!

PROBLEM:
   * ObjectDisposedException during Stream.CopyTo() when executing ObjectDisposedReproDotNet48
   * No such problem encountered when executing ObjectDisposedReproNet80