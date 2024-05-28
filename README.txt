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