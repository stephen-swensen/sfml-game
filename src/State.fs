namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

//todo: need states for losing
type PlayState =
    | StartGame of string
    | StartLevel of string
    | ActiveLevel of ActiveLevelState
    | PausedLevel of string * ActiveLevelState
    | EndLevel of string
    | EndGame of string

type GameState =
    { WindowDimensions: uint * uint
      PlayState: PlayState
      CurrentLevel: int }

module State =

    let update rnd levels elapsedMs commands gameState =
        let currentLevel = lazy(levels |> List.item gameState.CurrentLevel)
        match gameState.PlayState with
        | StartGame _ when commands.Continue ->
            { gameState with PlayState = StartLevel(currentLevel.Value.StartText) }
        | StartLevel _ when commands.Continue ->
            let level = currentLevel.Value
            let activeLevelState = ActiveLevelState.init rnd gameState.WindowDimensions level
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
            let activeLevelState' = ActiveLevelState.update gameState.WindowDimensions commands activeLevelState
            { gameState with PlayState = ActiveLevel activeLevelState' }
        | EndLevel _ when commands.Continue && gameState.CurrentLevel = (levels.Length - 1) ->
            { gameState with PlayState = EndGame("Game over, you win!!")}
        | EndLevel _ when commands.Continue ->
            let nextLevel = gameState.CurrentLevel + 1
            let level = levels |> List.item nextLevel
            { gameState with CurrentLevel = nextLevel; PlayState = StartLevel level.StartText }
        | _ ->
            gameState