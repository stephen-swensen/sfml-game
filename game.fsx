#load "refs.fsx"
#load "PollableWindow.fsx"
#load "assets.fsx"
#load "input.fsx"

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Graphics
open SFML.Window

let inline (%%) n m = ((n % m) + m) % m

type Actor = { Position: Vector2f }

type GameState =
    { Actor: Actor
      WindowDimensions: uint * uint
      HudHeight: uint
      WallCrossings: uint }

type World = PollableWindow * GameState * InputCommands

///Create the World with a BIG BANG
let bang () =
    let state =
        { Actor = { Position = Vector2f(0f, 0f) }
          WindowDimensions = (800u, 600u)
          HudHeight = 60u
          WallCrossings = 0u }

    let windowWidth, windowHeight = state.WindowDimensions

    let window =
        let window =
            new PollableWindow(new VideoMode(windowWidth, windowHeight), "Stephen's first game!")
        //https://www.sfml-dev.org/tutorials/2.5/window-window.php#controlling-the-framerate
        //per docs "Never use both setVerticalSyncEnabled and setFramerateLimit at the same time! They would badly mix and make things worse."
        //window.SetVerticalSyncEnabled(true)
        window.SetFramerateLimit(60u)
        window

    let commands =
        { ChangeDirection = None
          CloseWindow = false }

    window, state, commands

///Calc new position from old position and directional movement
///(new pos, true|false wrapped around window)
let calcNewPosition (pos: Vector2f) direction (window_w, window_h) hudHeight =
    let moveUnit = 4f

    let pos' =
        match direction with
        | Up -> Vector2f(pos.X, pos.Y - moveUnit)
        | Left -> Vector2f(pos.X - moveUnit, pos.Y)
        | Down -> Vector2f(pos.X, pos.Y + 4f)
        | Right -> Vector2f(pos.X + moveUnit, pos.Y)

    let pos'' =
        Vector2f(pos'.X %% (float32 window_w), pos'.Y %% (float32 (window_h - hudHeight)))

    pos'', pos' <> pos''

let updateState commands state =
    let pos = state.Actor.Position

    match commands.ChangeDirection with
    | Some direction ->
        let pos, wrapped =
            calcNewPosition pos direction state.WindowDimensions state.HudHeight

        { state with
              Actor = { state.Actor with Position = pos }
              WallCrossings =
                  if wrapped then
                      state.WallCrossings + 1u
                  else
                      state.WallCrossings }
    | None -> state

let drawState assets (window: PollableWindow) state =
    window.Clear()

    use circle =
        new CircleShape(10.0f, FillColor = Color.Green, Position = state.Actor.Position)

    window.Draw(circle)

    let hudPos =
        Vector2f(
            0f,
            ((snd >> float32) state.WindowDimensions)
            - (float32 state.HudHeight)
        )

    use hud =
        new RectangleShape(
            Vector2f((fst >> float32) state.WindowDimensions, float32 state.HudHeight),
            FillColor = Color.Cyan,
            Position = hudPos
        )

    window.Draw(hud)

    use hudText = new SFML.Graphics.Text()

    do
        hudText.Font <- assets.Fonts.DejaVuSansMono
        hudText.DisplayedString <- sprintf $"Wall Crossings: %u{state.WallCrossings}"
        hudText.CharacterSize <- 30u
        hudText.Position <- hudPos
        hudText.FillColor <- Color.Black

    window.Draw(hudText)
    window.Display()

let run () =
    let assets = Assets.load ()

    let rec loop ((window, state, commands): World) =
        if not window.IsOpen then
            ()
        else
            let commands = Input.pollEvents window commands
            let state = updateState commands state

            if commands.CloseWindow then
                window.Dispose()
            else
                drawState assets window state
                loop (window, state, commands)

    loop (bang ())

run ()
