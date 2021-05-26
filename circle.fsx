#load "refs.fsx"
#load "PollableWindow.fsx"

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Graphics
open SFML.Window

type Direction =
    | Up
    | Down
    | Left
    | Right

type InputCommands =
    { ChangeDirection: Direction option
      CloseWindow: bool }

type Actor = { Position: Vector2f }

type GameState = { Actor: Actor }

type World = PollableWindow * GameState * InputCommands

///Create the World with a BIG BANG
let bang () =
    let window =
        let window = new PollableWindow(new VideoMode(800u, 600u), "Circle Me Timbers!")
        window.SetVerticalSyncEnabled(true)
        window.SetFramerateLimit(120u)
        window

    let state = { Actor = { Position = Vector2f(0f,0f) } }

    let commands =
        { ChangeDirection = None
          CloseWindow = false }

    window, state, commands

///Apply a single event to some existing command state, producing a new command state
let applyEvent commands (event:Event) =
    let keyMapping =
        [ Keyboard.Key.Up, Up
          Keyboard.Key.Left, Left
          Keyboard.Key.Right, Right
          Keyboard.Key.Down, Down ]

    match event.Type with
    | EventType.Closed -> { commands with CloseWindow = true }
    | EventType.KeyPressed ->
        let action =
            keyMapping
            |> Seq.tryPick
                (fun (code, action) ->
                    if code = event.Key.Code then
                        Some action
                    else
                        None)

        match action with
        | Some _ as mp ->
            { commands with ChangeDirection = mp }
        | None when event.Key.Code = Keyboard.Key.Escape ->
            { commands with CloseWindow = true }
        | None -> commands

    | EventType.KeyReleased ->
        let action =
            keyMapping
            |> Seq.tryPick
                (fun (code, action) ->
                    if Keyboard.IsKeyPressed(code) then
                        Some action
                    else
                        None)

        { commands with ChangeDirection = action }
        | _ -> commands

let pollEvents (window: PollableWindow) commands =
    window.PollEvents(commands, applyEvent)

let calcNewPosition (pos: Vector2f) action =
    let moveUnit = 4f
    match action with
    | Up -> new Vector2f(pos.X, pos.Y - moveUnit)
    | Left -> new Vector2f(pos.X - moveUnit, pos.Y)
    | Down -> new Vector2f(pos.X, pos.Y + 4f)
    | Right -> new Vector2f(pos.X + moveUnit, pos.Y)

let updateState commands state =
    let pos = state.Actor.Position

    match commands.ChangeDirection with
    | Some action ->
        let pos = calcNewPosition pos action
        { state with Actor = { state.Actor with Position = pos } }
    | None ->
        state

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
            window.Close()
        else
            drawState window state
            loop (window, state, commands)
loop (bang ())
