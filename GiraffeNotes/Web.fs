module GiraffeNotes.Web

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Security.Claims

// ---------------------------------
// Response Helpers
// ---------------------------------

[<CLIMutable>]
type GenericResponse =
    { Status: int
      Message: string }

let notFound message =
    RequestErrors.NOT_FOUND { Status = 404; Message = message }

let unauthorized: HttpHandler =
    setStatusCode 401
    >=> setHttpHeader "WWW-Authenticate" "Bearer"

let internalError message: HttpHandler =
    ServerErrors.INTERNAL_ERROR { Status = 500; Message = message }

// ---------------------------------
// Authentication
// ---------------------------------

let authenticated: HttpHandler = requiresAuthentication unauthorized

// ---------------------------------
// Handlers
// ---------------------------------

module Notes =
    [<CLIMutable>]
    type CreateNoteReq =
        { Text: string }

    let newNote (req: CreateNoteReq): HttpHandler =
        let newNote = Notes.saveNew "google-oauth2|101971299993373356905" req.Text
        json newNote

    let getNote (id: string): HttpHandler =
        id
        |> Notes.getById
        |> Option.map json
        |> Option.defaultValue (
            notFound (sprintf "Could not find note with id %s" id)
        )

    let handlers: HttpHandler =
        choose
            [ POST
                >=> route "/notes"
                >=> authenticated
                >=> bindModel<CreateNoteReq> None newNote
                
              GET >=> routef "/notes/%s" getNote ]

// ---------------------------------
// Config
// ---------------------------------

let webApp =
    choose
        [ route "/" >=> text "Hello, world!"
          Notes.handlers

          setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> internalError ex.Message

let configureApp (app: IApplicationBuilder) =
    app
        .UseAuthentication()
        .UseGiraffeErrorHandler(errorHandler)
        .UseStaticFiles()
        .UseResponseCaching()
        .UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun options ->
            options.Authority <- "https://notes-api-demo.auth0.com/"
            options.Audience <- "https://notes-api/"

            options.TokenValidationParameters <- TokenValidationParameters(
                NameClaimType = ClaimTypes.NameIdentifier
            )
        ) |> ignore
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore
