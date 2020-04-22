namespace GiraffeNotes

[<RequireQualifiedAccess>]
module Config =
    let environmentName = "Development"

    [<CLIMutable>]
    type JwtBearer =
        { Authority: string
          Audience: string }

    [<CLIMutable>]
    type Authentication = { JwtBearer: JwtBearer }

    [<CLIMutable>]
    type Database = { ConnectionString: string }

    [<CLIMutable>]
    type Configuration =
        { Authentication: Authentication
          Database: Database }