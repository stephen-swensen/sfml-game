namespace Swensen.SFML.Game

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Window
open SFML.Graphics
open System

type World = PollableWindow * GameState * InputCommands

module Game =
    let rnd =
        fun () ->
            let r = new Random()
            r.Next()

    ///Create the World with a BIG BANG
    let bang () =
        let state = GameState.init ()

        let window =
            let window =
                new PollableWindow(
                    VideoMode(fst state.WindowDimensions, snd state.WindowDimensions),
                    state.Title
                )
            //https://www.sfml-dev.org/tutorials/2.5/window-window.php#controlling-the-framerate
            //per docs "Never use both setVerticalSyncEnabled and setFramerateLimit at the same time! They would badly mix and make things worse."
            //window.SetVerticalSyncEnabled(true)
            window.SetFramerateLimit(60u)
            window

        let commands =
            { ChangeDirection = None
              CloseWindow = false
              Continue = false }

        window, state, commands

    [<EntryPoint>]
    let main args =
        let assets = Assets.load ()
        let lsw = Diagnostics.Stopwatch()

        let rec loop ((window, state, commands): World) =
            if not window.IsOpen then
                ()
            else
                let sw = Diagnostics.Stopwatch()
                sw.Start()
                let commands = Input.pollEvents window commands
                let gameState = GameState.update rnd assets.Levels commands state
                //clear out previous commands that shouldn't persist
                let commands = { commands with Continue = false }

                match gameState.PlayState with
                | ActiveLevel _ when lsw.IsRunning |> not -> lsw.Start()
                | PausedLevel _ when lsw.IsRunning -> lsw.Stop()
                | EndLevel _ -> lsw.Stop(); lsw.Reset()
                | _ -> ()

                let playState =
                    match gameState.PlayState with
                    | ActiveLevel levelState ->
                        ActiveLevel { levelState with ElapsedMs = lsw.ElapsedMilliseconds }
                    | ps -> ps

                let gameState = { gameState with PlayState=playState }

                if commands.CloseWindow then
                    window.Dispose()
                else
                    window.Clear()
                    Drawing.drawState assets window gameState
                    sw.Stop()
                    Console.Write($"World Refresh: %i{sw.ElapsedMilliseconds}ms     \r")
                    window.Display()
                    loop (window, gameState, commands)

        loop (bang ())
        0
