namespace Swensen.SFML.Game

type Outcome =
    | Win
    | Lose

type PlayState =
    | StartGame of string
    | StartLevel of string
    | ActiveLevel of LevelState
    | PausedLevel of string * LevelState
    | EndLevel of string * LevelState
    | EndGame of string * LevelState * Outcome

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
            levels |> List.item gameState.CurrentLevelIndex

        match gameState.PlayState with
        | StartGame _ when commands.Continue ->
            { gameState with
                  PlayState = StartLevel(currentLevel.StartText) }
        | StartLevel _ when commands.Continue ->
            let level = currentLevel

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
            //all visible enemies are eaten or poison but
            //the number of enemies on the board does not equal
            //the enemy level count
            levelState.Enemies
            |> List.forall (fun e -> e.Eaten)
            && (currentLevel.EnemyCount - currentLevel.PoisonCount) <> levelState.Enemies.Length ->
            { gameState with
                  PlayState = EndGame("Game over (you lose): some got away!", levelState, Lose) }
        | ActiveLevel levelState when levelState.Player.Poisoned ->
            { gameState with
                  PlayState = EndGame("Game over (you lose): you were poisoned!", levelState, Lose) }
        | ActiveLevel levelState when
            levelState.Enemies
            |> List.forall (fun e -> e.Poison || e.Eaten) ->
            if gameState.CurrentLevelIndex = (levels.Length - 1) then
                { gameState with
                      PlayState = EndGame("Game over, you win!!", levelState, Win) }
            else
                { gameState with
                      PlayState = EndLevel("You beat the level!", levelState) }
        | ActiveLevel levelState ->
            let levelState' =
                LevelState.update gameState.BoardDimensions commands levelState

            { gameState with
                  PlayState = ActiveLevel levelState' }
        | EndLevel _ when commands.Continue ->
            let nextLevelIndex = gameState.CurrentLevelIndex + 1
            let nextLevel = levels |> List.item nextLevelIndex

            { gameState with
                  CurrentLevelIndex = nextLevelIndex
                  PlayState = StartLevel nextLevel.StartText }
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
