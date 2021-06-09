namespace Swensen.SFML.Game

// originally adapted from xcvd's question at https://stackoverflow.com/questions/22072603/f-game-development-modifying-state-variables/22076855#22076855

open System
open SFML.Window
open FsToolkit

type World = PollableWindow * GameState * InputCommands

module Game =
    let rnd =
        fun () ->
            let r = new Random()
            r.Next()

    ///Create the World with a BIG BANG
    let bang levelIndex =
        let state = GameState.init levelIndex

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

    ///Start, Stop, or Reset level stop watch according to game state
    let controlLevelStopWatch (lsw: Diagnostics.Stopwatch) gameState =
        match gameState.PlayState with
        | ActiveLevel _ when lsw.IsRunning |> not -> lsw.Start()
        | PausedLevel _ when lsw.IsRunning -> lsw.Stop()
        | EndLevel _ ->
            lsw.Stop()
            lsw.Reset()
        | _ -> ()

    let updateActiveLevelElapsedMs ms gameState =
        let playState =
            match gameState.PlayState with
            | ActiveLevel levelState -> ActiveLevel { levelState with ElapsedMs = ms }
            | ps -> ps

        { gameState with PlayState = playState }

    [<EntryPoint>]
    let main args =
        let levelIndex =
            Config.tryGetSetting ("level") |? "1"
            |> int32
            |> ((+) (-1))

        let assets = Assets.load ()
        let world = bang levelIndex

        let lsw = Diagnostics.Stopwatch()

        let rec loop ((window, state, commands): World) =
            if not window.IsOpen then
                ()
            else
                let sw = Diagnostics.Stopwatch()
                sw.Start()
                let commands = Input.pollEvents window commands

                let gameState =
                    GameState.update rnd assets.Levels commands state

                controlLevelStopWatch lsw gameState

                let gameState =
                    updateActiveLevelElapsedMs lsw.ElapsedMilliseconds gameState

                if gameState.Exiting then
                    window.Dispose()
                else
                    window.Clear()
                    Drawing.drawState assets window gameState
                    sw.Stop()
                    Console.Write($"World Refresh: %i{sw.ElapsedMilliseconds}ms     \r")
                    window.Display()

                    //clear out previous commands that shouldn't persist
                    let commands = { commands with Continue = false }
                    loop (window, gameState, commands)

        loop world
        0
