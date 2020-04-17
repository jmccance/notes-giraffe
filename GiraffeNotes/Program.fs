module GiraffeNotes.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Serilog
open Serilog.Events
open Giraffe

[<EntryPoint>]
let main _ =
    Log.Logger <-
        (LoggerConfiguration())
            .MinimumLevel.Information()
            .Enrich.WithProperty("Environment", Config.environmentName)
            .Enrich.WithProperty("Application", "DustedCodes")
            .WriteTo.Console()
            .CreateLogger()

    Log.Information "Starting GiraffeNotes..."
    
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseSerilog()
                    .Configure(Web.configureApp)
                    .ConfigureServices(Web.configureServices)
                    |> ignore)
        .Build()
        .Run()
    0