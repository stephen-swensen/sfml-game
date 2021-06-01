namespace Swensen.SFML.Game

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Window
open SFML.Graphics
open System

type World = PollableWindow * GameState * InputCommands

module Game =
    let genEnemies count (x, y) =
        printfn "(x,y)=%A" (x, y)
        let radius = 20f
        let x = x - ((uint radius) * 2u)
        let y = y - ((uint radius) * 2u)
        printfn "(x,y)'=%A" (x, y)
        let rnd = new Random()

        [ for i in 1 .. count do
              //n.b. circles are drawn from top left corner of bounding box
              let pos =
                  Vector2f((((rnd.Next() |> uint) %% x) |> float32), (((rnd.Next() |> uint) %% y) |> float32))

              printfn "pos=%A" pos

              { Position = pos
                AliveColor = Color.Red
                EatenColor = Color.Blue
                Eaten = false
                Radius = radius } ]


    ///Create the World with a BIG BANG
    let bang () =

        let state =
            { Player =
                  { Position = Vector2f(0f, 0f)
                    Color = Color.Green
                    Radius = 10f }
              WindowDimensions = (800u, 600u)
              HudHeight = 60u
              WallCrossings = 0u
              Enemies = []
              EnemyCount = 8 }

        let state =
            { state with
                  Enemies = genEnemies state.EnemyCount state.BoardDimensions }

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

    [<EntryPoint>]
    let main args =
        let assets = Assets.load ()

        let rec loop ((window, state, commands): World) =
            if not window.IsOpen then
                ()
            else
                let sw = Diagnostics.Stopwatch()
                sw.Start()
                let commands = Input.pollEvents window commands
                let state = State.update commands state

                if commands.CloseWindow then
                    window.Dispose()
                else
                    window.Clear()
                    Drawing.drawState assets window state
                    sw.Stop()
                    Console.Write($"World Refresh: %i{sw.ElapsedMilliseconds}ms     \r")
                    window.Display()
                    loop (window, state, commands)

        loop (bang ())
        0
