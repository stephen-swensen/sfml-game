#load "refs.fsx"
#load "PollableWindow.fsx"

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Graphics
open SFML.Window

type MoveAction =
    | Up
    | Down
    | Left
    | Right

type InputCommands =
    { MovePosition: MoveAction option
      CloseWindow: bool }

type VirtualState = { Position: Vector2f }

type PhysicalState =
    { Window: PollableWindow
      Shape: CircleShape }

type World = InputCommands * VirtualState * PhysicalState

let initWorld () =
    let inputCommands =
        { MovePosition = None
          CloseWindow = false }

    let virtualState = { Position = Vector2f(0f, 0f) }

    let physicalState =
        let window =
            new PollableWindow(new VideoMode(800u, 600u), "Circle Me Timbers!")

        window.SetVerticalSyncEnabled(true)
        window.SetFramerateLimit(60u)

        let shape =
            new CircleShape(10.0f, FillColor = Color.Green)

        { Window = window; Shape = shape }

    inputCommands, virtualState, physicalState

let pollEvents (window: PollableWindow) ic =
    let keyMapping =
        [ Keyboard.Key.Up, Up
          Keyboard.Key.Left, Left
          Keyboard.Key.Right, Right
          Keyboard.Key.Down, Down ]

    window.PollEvents(
        ic,
        fun event state ->
            match event.Type with
            | EventType.Closed -> { state with CloseWindow = true }
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
                | Some _ as mp -> { ic with MovePosition = mp }
                | None -> ic
            | EventType.KeyReleased ->
                let action =
                    keyMapping
                    |> Seq.tryPick
                        (fun (code, action) ->
                            if Keyboard.IsKeyPressed(code) then
                                Some action
                            else
                                None)

                { ic with MovePosition = action }
            | _ -> state
    )

let updateVirtualState ic vs =
    let moveUnit = 4f
    let pos = vs.Position

    match ic.MovePosition with
    | Some action ->
        let pos =
            match action with
            | Up -> new Vector2f(pos.X, pos.Y - moveUnit)
            | Left -> new Vector2f(pos.X - moveUnit, pos.Y)
            | Down -> new Vector2f(pos.X, pos.Y + 4f)
            | Right -> new Vector2f(pos.X + moveUnit, pos.Y)

        { vs with Position = pos }
    | None -> vs

let updatePhysicalState vs ps =
    ps.Shape.Position <- vs.Position
    ps

let rec loop (ic, vs, ps) =
    if not ps.Window.IsOpen then
        ()
    else
        let ic = pollEvents ps.Window ic
        let vs = updateVirtualState ic vs
        let ps = updatePhysicalState vs ps

        ps.Window.Clear()
        ps.Window.Draw(ps.Shape)
        ps.Window.Display()
        loop (ic, vs, ps)

loop (initWorld ())
