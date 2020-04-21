module GiraffeNotes.Notes

open Giraffe
open System

type Note =
    { Id: string
      OwnerId: string
      Text: string
      CreatedAt: DateTime }

module Persistence =
    open Database
    open FSharp.Data.Sql

    let private toModel (note: NoteEntity) =
        { Id = note.Id.ToString()
          OwnerId = note.OwnerId
          Text = note.Text
          CreatedAt = note.CreatedAt }

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
        |> Persistence.getById
        |> Option.map json
        |> Option.defaultValue (json "Not found")

    let handlers: HttpHandler =
        choose
            [ GET >=> routef "/notes/%s" getNote
              POST >=> route "/notes" >=> newNote ]
