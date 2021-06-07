namespace Swensen.SFML.Game

type PlayState =
    | StartGame of string
    | StartLevel of string
    | ActiveLevel of LevelState
    | PausedLevel of string * LevelState
    | EndLevel of string
    | EndGame of string

type GameState =
    { WindowDimensions: uint * uint
      HudHeight: uint
      PlayState: PlayState
      CurrentLevelIndex: int
      Title: string }
    member this.BoardDimensions =
        let wx, wy = this.WindowDimensions
        wx, wy - this.HudHeight

module GameState =

    let update rnd levels commands gameState =
        let currentLevel =
            lazy (levels |> List.item gameState.CurrentLevelIndex)

        match gameState.PlayState with
        | StartGame _ when commands.Continue ->
            { gameState with
                  PlayState = StartLevel(currentLevel.Value.StartText) }
        | StartLevel _ when commands.Continue ->
            let level = currentLevel.Value

            let levelState =
                LevelState.init rnd gameState.BoardDimensions level

            { gameState with
                  PlayState = ActiveLevel levelState }
        | PausedLevel (_, levelState) when commands.Continue ->
            { gameState with
                  PlayState = ActiveLevel levelState }
        | ActiveLevel levelState when commands.Continue ->
            { gameState with
                  PlayState = PausedLevel("Paused", levelState) }
        | ActiveLevel levelState when
            levelState.Enemies
            |> List.forall (fun e -> e.Eaten)
            && levelState.EnemyCount <> levelState.Enemies.Length ->
            { gameState with
                  PlayState = EndGame("Game over (you lose): some got away!") }
        | ActiveLevel levelState when
            levelState.Enemies
            |> List.forall (fun e -> e.Eaten) ->
            { gameState with
                  PlayState = EndLevel("You beat the level!") }
        | ActiveLevel levelState ->
            let levelState' =
                LevelState.update gameState.BoardDimensions commands levelState

            { gameState with
                  PlayState = ActiveLevel levelState' }
        | EndLevel _ when
            commands.Continue
            && gameState.CurrentLevelIndex = (levels.Length - 1) ->
            { gameState with
                  PlayState = EndGame("Game over, you win!!") }
        | EndLevel _ when commands.Continue ->
            let nextLevel = gameState.CurrentLevelIndex + 1
            let level = levels |> List.item nextLevel

            { gameState with
                  CurrentLevelIndex = nextLevel
                  PlayState = StartLevel level.StartText }
        | _ -> gameState

    let init levelIndex =
        let state =
            { WindowDimensions = 800u, 600u
              HudHeight = 60u
              PlayState =
                  StartGame(
                      "Welcome to UFC\n\n\
                     Something strange is going on...\n\
                     Unidentified Flying Circles have...\n\
                     been identified!\n\n\
                     -----------------------------------------\n\n\
                     Controls:\n\
                     - Press Enter to Continue between screens\n\
                     - Press ESC to exit at any time\n\
                     - During game play,\n\
                     -- Press Up / Down / Left / Right arrows\n   \
                     to move around\n\
                     -- Press Enter to Pause or Resume\n\
                     "
                  )
              CurrentLevelIndex = levelIndex
              Title = "Unidentified Flying Circles (UFCs)!" }

        state
