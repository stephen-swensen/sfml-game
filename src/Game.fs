namespace Swensen.SFML.Game

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open SFML.System
open SFML.Window
open SFML.Graphics
open System

type World = PollableWindow * GameState * InputCommands

module Game =
    ///Create the World with a BIG BANG
    let bang () =

        let windowWidth, windowHeight = state.WindowDimensions

        let window =
            let window =
                new PollableWindow(
                    new VideoMode(windowWidth, windowHeight),
                    "Unidentified Flying Circles (UFCs)!"
                )
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
        let levelSw = Diagnostics.Stopwatch()
        levelSw.Start()

        let rec loop ((window, state, commands): World) =
            if not window.IsOpen then
                ()
            else
                let sw = Diagnostics.Stopwatch()
                sw.Start()
                let commands = Input.pollEvents window commands
                let state = State.update commands state

                let state =
                    { state with
                          ElapsedMs = levelSw.ElapsedMilliseconds }

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
