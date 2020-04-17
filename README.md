GiraffeNotes
============

An experiment in creating a web API with [Giraffe](https://github.com/giraffe-fsharp/Giraffe) and [F#](https://fsharp.org). 

Quick Start
-----------

Requirements:

- .NET Core 3.1

To run the app from the root project directory:

```
dotnet run -p GiraffeNotes
```

You should see some logging indicating the app has started. Verify that things are working with:

```
curl -v http://localhost:5000/