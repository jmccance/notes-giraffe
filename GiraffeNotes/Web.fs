module GiraffeNotes.Web

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Security.Claims
open Microsoft.Extensions.Configuration

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

let bindUser (f: ClaimsPrincipal -> HttpHandler): HttpHandler =
    warbler (fun _ -> fun next ctx -> f ctx.User next ctx)

// ---------------------------------
// Handlers
// ---------------------------------

module Notes =
    [<CLIMutable>]
    type CreateNoteReq =
        { Text: string }

    let newNote (user: ClaimsPrincipal) (req: CreateNoteReq): HttpHandler =
        let newNote = Notes.saveNew user.Identity.Name req.Text
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
                >=> bindUser (fun u ->
                    bindModel<CreateNoteReq> None (fun req ->
                    newNote u req ))
                
                
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
    let config = 
        services
            .BuildServiceProvider()
            .GetService<IConfiguration>()
            .Get<Config.Configuration>()

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun options ->
            let bearerConfig = config.Authentication.JwtBearer

            options.Authority <- bearerConfig.Authority
            options.Audience <- bearerConfig.Audience

            options.TokenValidationParameters <- TokenValidationParameters(
                NameClaimType = ClaimTypes.NameIdentifier
            )
        ) |> ignore
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore
