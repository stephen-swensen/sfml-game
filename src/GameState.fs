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
      PlayState: PlayState
      CurrentLevel: int
      Title: string }

module GameState =

    let update rnd levels commands gameState =
        let currentLevel = lazy(levels |> List.item gameState.CurrentLevel)
        match gameState.PlayState with
        | StartGame _ when commands.Continue ->
            { gameState with PlayState = StartLevel(currentLevel.Value.StartText) }
        | StartLevel _ when commands.Continue ->
            let level = currentLevel.Value
            let activeLevelState = LevelState.init rnd gameState.WindowDimensions level
            { gameState with PlayState = ActiveLevel activeLevelState }
        | PausedLevel(_, activeLevelState) when commands.Continue ->
            { gameState with PlayState = ActiveLevel activeLevelState }
        | ActiveLevel activeLevelState when commands.Continue ->
            { gameState with PlayState = PausedLevel("Paused", activeLevelState) }
        | ActiveLevel activeLevelState when activeLevelState.Enemies = [] ->
            { gameState with PlayState = EndGame("Game over (you lose): they all got away!")}
        | ActiveLevel activeLevelState when activeLevelState.Enemies |> List.forall (fun e -> e.Eaten) ->
            { gameState with PlayState = EndLevel("You beat the level!")}
        | ActiveLevel activeLevelState ->
            let activeLevelState' = LevelState.update gameState.WindowDimensions commands activeLevelState
            { gameState with PlayState = ActiveLevel activeLevelState' }
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
            PlayState = StartGame ("Welcome to UFC's - something strange")
            CurrentLevel = 0
            Title = "Unidentified Flying Circles (UFCs)!"
        }
        state
