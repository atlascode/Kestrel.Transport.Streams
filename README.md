# Kestrel.Transport.Streams

This project contains the [Atlas.AspNetCore.Server.Kestrel.Transport.Streams](https://www.nuget.org/packages/Atlas.AspNetCore.Server.Kestrel.Transport.Streams) NuGet package.

This transport works alongside the existing LibUV or Socket transports. It allows you to make your own internal web requests and pass them through the Kestrel pipeline without the need to open any sockets.

There is currently a single helper which wraps the streams and returns a string for convenience.

``` csharp
var connection = StreamTransport.CreateConnection();
string html = await connection.Get("/contact");
```

Here you can see how to get the html returned from a get request to /contact. This can be called from services like email generators that run on background threads.