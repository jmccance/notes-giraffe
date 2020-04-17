module GiraffeNotes.Notes

open Giraffe
open System

type Note =
    { Id: string
      OwnerId: string
      Text: string
      CreatedAt: DateTime }

module Database =
    open Dapper

    

module Web = 
    type private CreateNoteReq =
        { Text: string }

    let getNote (id: string): HttpHandler =
        json
            { Id = id
              OwnerId = "nobody"
              Text = "The note is... F#"
              CreatedAt = DateTime.UtcNow }

    let newNote: HttpHandler =
        bindJson<CreateNoteReq> 
        |> fun req -> ServerErrors.notImplemented <| text "TODO"

    let handlers: HttpHandler =
        choose
            [ GET >=> routef "/notes/%s" getNote
              POST >=> route "/notes" >=> newNote ]
