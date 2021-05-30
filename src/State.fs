namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

type Player =
    { Position: Vector2f
      Radius: float32
      Color: Color }

type Enemy =
    { Position: Vector2f
      Radius: float32
      AliveColor: Color
      EatenColor: Color
      Eaten: bool }

type GameState =
    { Player: Player
      Enemies: Enemy list
      WindowDimensions: uint * uint
      HudHeight: uint
      WallCrossings: uint
      EnemyCount: int }
    member this.BoardDimensions =
        let wx, wy = this.WindowDimensions
        wx, wy - this.HudHeight

module State =

    ///Calc new position from old position and directional movement
    ///(new pos, true|false wrapped around window)
    let calcNewPosition (pos: Vector2f) direction (x, y) =
        let moveUnit = 4f

        let pos' =
            match direction with
            | Up -> Vector2f(pos.X, pos.Y - moveUnit)
            | Left -> Vector2f(pos.X - moveUnit, pos.Y)
            | Down -> Vector2f(pos.X, pos.Y + 4f)
            | Right -> Vector2f(pos.X + moveUnit, pos.Y)

        let pos'' =
            Vector2f(pos'.X %% (float32 x), pos'.Y %% (float32 y))

        pos'', pos' <> pos''

    let update commands state =
        let pos = state.Player.Position

        match commands.ChangeDirection with
        | Some direction ->
            let pos, wrapped =
                calcNewPosition pos direction state.BoardDimensions

            { state with
                  Player = { state.Player with Position = pos }
                  WallCrossings =
                      if wrapped then
                          state.WallCrossings + 1u
                      else
                          state.WallCrossings }
        | None -> state
