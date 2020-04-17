module GiraffeNotes.Web

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

// ---------------------------------
// Models
// ---------------------------------

type Message =
    { Text: string }

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose
        [ route "/" >=> text "Hello, world!"
          Notes.Web.handlers

          setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler).UseStaticFiles().UseResponseCaching().UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore
