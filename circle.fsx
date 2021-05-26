#load "refs.fsx"
#load "PollableWindow.fsx"

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Graphics
open SFML.Window

let inline (%%) n m = ((n % m) + m) % m

type Direction =
    | Up
    | Down
    | Left
    | Right

type InputCommands =
    { ChangeDirection: Direction option
      CloseWindow: bool }

type Actor = { Position: Vector2f }

type GameState =
    { Actor: Actor
      WindowDimensions: uint * uint }

type World = PollableWindow * GameState * InputCommands

///Create the World with a BIG BANG
let bang () =
    let state =
        { Actor = { Position = Vector2f(0f, 0f) }
          WindowDimensions = (800u, 600u) }

    let windowWidth, windowHeight = state.WindowDimensions

    let window =
        let window =
            new PollableWindow(new VideoMode(windowWidth, windowHeight), "Circle Me Timbers!")
        //https://www.sfml-dev.org/tutorials/2.5/window-window.php#controlling-the-framerate
        //per docs "Never use both setVerticalSyncEnabled and setFramerateLimit at the same time! They would badly mix and make things worse."
        //window.SetVerticalSyncEnabled(true)
        window.SetFramerateLimit(60u)
        window

    let commands =
        { ChangeDirection = None
          CloseWindow = false }

    window, state, commands

///Apply a single event to some existing command state, producing a new command state
let applyEvent commands (event: Event) =
    let keyMapping =
        [ Keyboard.Key.Up, Up
          Keyboard.Key.Left, Left
          Keyboard.Key.Right, Right
          Keyboard.Key.Down, Down ]

    match event.Type with
    | EventType.Closed -> { commands with CloseWindow = true }
    | EventType.KeyPressed ->
        let direction =
            keyMapping
            |> Seq.tryPick
                (fun (code, direction) ->
                    if code = event.Key.Code then
                        Some direction
                    else
                        None)

        match direction with
        | Some _ as mp -> { commands with ChangeDirection = mp }
        | None when event.Key.Code = Keyboard.Key.Escape -> { commands with CloseWindow = true }
        | None -> commands

    | EventType.KeyReleased ->
        let direction =
            keyMapping
            |> Seq.tryPick
                (fun (code, direction) ->
                    if Keyboard.IsKeyPressed(code) then
                        Some direction
                    else
                        None)

        { commands with
              ChangeDirection = direction }
    | _ -> commands

let pollEvents (window: PollableWindow) commands = window.PollEvents(commands, applyEvent)

let calcNewPosition (pos: Vector2f) direction (window_w, window_h) =
    let moveUnit = 4f

    let pos =
        match direction with
        | Up -> Vector2f(pos.X, pos.Y - moveUnit)
        | Left -> Vector2f(pos.X - moveUnit, pos.Y)
        | Down -> Vector2f(pos.X, pos.Y + 4f)
        | Right -> Vector2f(pos.X + moveUnit, pos.Y)

    Vector2f(pos.X %% (float32 window_w), pos.Y %% (float32 window_h))


let updateState commands state =
    let pos = state.Actor.Position

    match commands.ChangeDirection with
    | Some direction ->
        let pos =
            calcNewPosition pos direction state.WindowDimensions

        { state with
              Actor = { state.Actor with Position = pos } }
    | None -> state

let drawState (window: PollableWindow) state =
    window.Clear()

    use circle =
        new CircleShape(10.0f, FillColor = Color.Green, Position = state.Actor.Position)

    window.Draw(circle)
    window.Display()

let rec loop ((window, state, commands): World) =
    if not window.IsOpen then
        ()
    else
        let commands = window.PollEvents(commands, applyEvent)
        let state = updateState commands state

        if commands.CloseWindow then
            window.Dispose()
        else
            drawState window state
            loop (window, state, commands)

loop (bang ())
