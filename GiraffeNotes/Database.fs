module GiraffeNotes.Database

open FSharp.Data.Sql

type Sql =
    SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL,
                    "Host=localhost;Database=example;Username=user;Password=password",
                    CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL>

type NoteEntity = Sql.dataContext.``public.noteEntity``

let ctx = Sql.GetDataContext()
