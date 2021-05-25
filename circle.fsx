#load "refs.fsx"

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Graphics
open SFML.Window

type FWindow(videoMode: VideoMode, title: string) =
    inherit RenderWindow(videoMode, title)

    member _.PollEvent() =
        let mutable event = SFML.Window.Event()

        if base.PollEvent(&event) then
            let event' = event
            Some(event')
        else
            None

    member this.PollEvents(state, f) =
        let rec loop (event: SFML.Window.Event option) state =
            match event with
            | None -> state
            | Some (event) ->
                let state = f event state
                loop (this.PollEvent()) state

        loop (this.PollEvent()) state

let window =
    new FWindow(new VideoMode(400u, 400u), "SFML works!")

window.SetVerticalSyncEnabled(true)
window.SetFramerateLimit(60u)

let shape =
    new CircleShape(10.0f, FillColor = Color.Green)

type EventState =
    { PressedKey: Keyboard.Key
      CloseRequested: bool }

type ConfigState = { MoveUnit: float32 }

type State =
    { EventState: EventState
      ConfigState: ConfigState }

let initState () =
    { EventState =
          { PressedKey = Keyboard.Key.Unknown
            CloseRequested = false }
      ConfigState = { MoveUnit = 4.f } }

let rec loop state =
    let { EventState = eventState ; ConfigState = configState } = state

    if not window.IsOpen then
        ()
    else
        let eventState =
            let moveKeys =
                [ Keyboard.Key.Up
                  Keyboard.Key.Left
                  Keyboard.Key.Down
                  Keyboard.Key.Right ]

            window.PollEvents(
                eventState,
                fun event state ->
                    match event.Type with
                    | EventType.Closed -> { state with CloseRequested = true }
                    | EventType.KeyPressed ->
                        let pressedKey =
                            if moveKeys |> List.contains event.Key.Code then
                                event.Key.Code
                            else
                                Keyboard.Key.Unknown

                        { state with PressedKey = pressedKey }
                    | EventType.KeyReleased ->
                        let pressedKeys =
                            List.filter (fun key -> Keyboard.IsKeyPressed(key)) moveKeys

                        let pressedKey =
                            if pressedKeys.IsEmpty then
                                Keyboard.Key.Unknown
                            else
                                pressedKeys.Head

                        { state with PressedKey = pressedKey }
                    | _ -> state
            )

        match eventState.PressedKey with
        | Keyboard.Key.Up -> shape.Position <- new Vector2f(shape.Position.X, shape.Position.Y - configState.MoveUnit)
        | Keyboard.Key.Left -> shape.Position <- new Vector2f(shape.Position.X - configState.MoveUnit, shape.Position.Y)
        | Keyboard.Key.Down -> shape.Position <- new Vector2f(shape.Position.X, shape.Position.Y + configState.MoveUnit)
        | Keyboard.Key.Right -> shape.Position <- new Vector2f(shape.Position.X + configState.MoveUnit, shape.Position.Y)
        | _ -> ()

        window.Clear()
        window.Draw(shape)
        window.Display()
        loop { state with EventState = eventState }

loop (initState())
