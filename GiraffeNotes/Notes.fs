module GiraffeNotes.Notes

open System

type Note =
    { Id: string
      OwnerId: string
      Text: string
      CreatedAt: DateTime }

module Persistence =
    open Database
    open FSharp.Data.Sql

    let notes = ctx.Public.Note

    let private toNote (note: NoteEntity) =
        { Id = note.Id.ToString()
          OwnerId = note.OwnerId
          Text = note.Text
          CreatedAt = note.CreatedAt }

    let saveNew ownerId text =
        let row = notes.Create()
        row.Id <- Guid.NewGuid()
        row.OwnerId <- ownerId
        row.Text <- text
        ctx.SubmitUpdates()
        toNote row

    let getById id =
        query {
            for note in ctx.Public.Note do
                where (note.Id = id)
                select note
        }
        |> Seq.map toNote
        |> Seq.tryHead

let saveNew = Persistence.saveNew

let getById (id: string) =
    match Guid.TryParse id with
        | (true, parsedId) -> Persistence.getById parsedId
        | (false, _) -> None
