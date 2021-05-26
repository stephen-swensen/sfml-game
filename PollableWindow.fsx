[<AutoOpen>]
module PollableWindow_fsx

#load "refs.fsx"

open SFML.Graphics
open SFML.Window

///A RenderWindow which exposes functional methods for polling events directly
///which can be used instead of using event handlers and DispatchEvents.
type PollableWindow(videoMode: VideoMode, title: string) =
    inherit RenderWindow(videoMode, title)

    ///Poll for the next event, if any
    member _.PollEvent() =
        let mutable event = SFML.Window.Event()

        if base.PollEvent(&event) then
            let event' = event
            Some(event')
        else
            None

    ///Poll for all events, applying the input state to f and returning the final state
    member this.PollEvents(state, f) =
        let rec loop state (event: SFML.Window.Event option) =
            match event with
            | None -> state
            | Some (event) ->
                let state = f state event
                loop state (this.PollEvent())

        loop state (this.PollEvent())
