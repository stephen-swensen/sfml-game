#load "refs.fsx"

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
        let rec loop (event:SFML.Window.Event option) state =
            match event with
            | None -> state
            | Some(event) ->
                let state = f event state
                loop (this.PollEvent()) state
        loop (this.PollEvent()) state

let window = new FWindow(new VideoMode(400u, 400u), "SFML works!")
window.SetVerticalSyncEnabled(true)
window.SetFramerateLimit(60u)
let shape = new CircleShape(10.0f, FillColor=Color.Green)

let moveKeys = [ Keyboard.Key.Up; Keyboard.Key.Left;
                 Keyboard.Key.Down; Keyboard.Key.Right ]

type EventState = { PressedKey: Keyboard.Key }
with
    static member Initial = { PressedKey = Keyboard.Key.Unknown }

while window.IsOpen do
    let eventState =
        window.PollEvents(EventState.Initial, fun event state ->
            match event.Type with
            | EventType.Closed ->
                window.Close()
                state
            | EventType.KeyPressed ->
                let pressedKey =
                    if moveKeys |> List.contains event.Key.Code then
                        event.Key.Code
                    else
                        Keyboard.Key.Unknown
                { state with PressedKey = pressedKey }
            | EventType.KeyReleased ->
                let pressedKeys = List.filter (fun key -> Keyboard.IsKeyPressed(key)) moveKeys
                let pressedKey =
                    if pressedKeys.IsEmpty then
                        Keyboard.Key.Unknown
                    else
                        pressedKeys.Head
                { state with PressedKey = pressedKey }
            | _ ->
                state)

    let moveUnit = 2.f
    match eventState.PressedKey with
    | Keyboard.Key.Up    -> shape.Position <- new Vector2f(shape.Position.X, shape.Position.Y - moveUnit)
    | Keyboard.Key.Left  -> shape.Position <- new Vector2f(shape.Position.X - moveUnit, shape.Position.Y)
    | Keyboard.Key.Down  -> shape.Position <- new Vector2f(shape.Position.X, shape.Position.Y + moveUnit)
    | Keyboard.Key.Right -> shape.Position <- new Vector2f(shape.Position.X + moveUnit, shape.Position.Y)
    | _ -> ()

    window.Clear()
    window.Draw(shape)
    window.Display()
