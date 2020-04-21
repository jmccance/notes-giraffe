module GiraffeNotes.Notes

open Giraffe
open System

type Note =
    { Id: string
      OwnerId: string
      Text: string
      CreatedAt: DateTime }

module Database =
    open FSharp.Data.Sql

    type private Sql =
        SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL,
                        "Host=localhost;Database=example;Username=user;Password=password",
                        CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL>

    type private NoteEntity = Sql.dataContext.``public.noteEntity``

    let private ctx = Sql.GetDataContext()

    let private toModel (note: NoteEntity) =
        { Id = note.Id.ToString()
          OwnerId = note.OwnerId
          Text = note.Text
          CreatedAt = note.CreatedAt }

    module Notes =
        let getById id =
            query {
                for note in ctx.Public.Note do
                    where (note.Id = id)
                    select note
            }
            |> Seq.map toModel
            |> Seq.tryHead

module Web =
    type private CreateNoteReq =
        { Text: string }

    let newNote: HttpHandler =
        bindJson<CreateNoteReq> |> fun req -> ServerErrors.notImplemented <| text "TODO"

    let getNote (id: string): HttpHandler =
        id
        |> Guid.Parse
        |> Database.Notes.getById
        |> Option.map json
        |> Option.defaultValue (json "Not found")

    let handlers: HttpHandler =
        choose
            [ GET >=> routef "/notes/%s" getNote
              POST >=> route "/notes" >=> newNote ]
