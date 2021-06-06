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
      CurrentLevel: int
      Title: string }
with
    member this.BoardDimensions =
        let wx, wy = this.WindowDimensions
        wx, wy - this.HudHeight

module GameState =

    let update rnd levels commands gameState =
        let currentLevel = lazy(levels |> List.item gameState.CurrentLevel)
        match gameState.PlayState with
        | StartGame _ when commands.Continue ->
            { gameState with PlayState = StartLevel(currentLevel.Value.StartText) }
        | StartLevel _ when commands.Continue ->
            let level = currentLevel.Value
            let levelState = LevelState.init rnd gameState.BoardDimensions level
            { gameState with PlayState = ActiveLevel levelState }
        | PausedLevel(_, levelState) when commands.Continue ->
            { gameState with PlayState = ActiveLevel levelState }
        | ActiveLevel levelState when commands.Continue ->
            { gameState with PlayState = PausedLevel("Paused", levelState) }
        | ActiveLevel levelState when levelState.Enemies |> List.forall (fun e -> e.Eaten) && levelState.EnemyCount <> levelState.Enemies.Length ->
            { gameState with PlayState = EndGame("Game over (you lose): some got away!")}
        | ActiveLevel levelState when levelState.Enemies |> List.forall (fun e -> e.Eaten) ->
            { gameState with PlayState = EndLevel("You beat the level!")}
        | ActiveLevel levelState ->
            let levelState' = LevelState.update gameState.BoardDimensions commands levelState
            { gameState with PlayState = ActiveLevel levelState' }
        | EndLevel _ when commands.Continue && gameState.CurrentLevel = (levels.Length - 1) ->
            { gameState with PlayState = EndGame("Game over, you win!!")}
        | EndLevel _ when commands.Continue ->
            let nextLevel = gameState.CurrentLevel + 1
            let level = levels |> List.item nextLevel
            { gameState with CurrentLevel = nextLevel; PlayState = StartLevel level.StartText }
        | _ ->
            gameState

    let init () =
        let state = {
            WindowDimensions = 800u, 600u
            HudHeight = 60u
            PlayState = StartGame ("Welcome to UFC\n\nSomething strange is going on...\nUnidentified Fly Circles have...\nbeen identified!")
            CurrentLevel = 0
            Title = "Unidentified Flying Circles (UFCs)!"
        }
        state
