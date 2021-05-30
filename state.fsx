[<AutoOpen>]
module state_fsx

#load "refs.fsx"
#load "input.fsx"

open SFML.System

type Player = { Position: Vector2f }

type GameState =
    { Player: Player
      WindowDimensions: uint * uint
      HudHeight: uint
      WallCrossings: uint }

module State =

    let inline (%%) n m = ((n % m) + m) % m

    ///Calc new position from old position and directional movement
    ///(new pos, true|false wrapped around window)
    let calcNewPosition (pos: Vector2f) direction (window_w, window_h) hudHeight =
        let moveUnit = 4f

        let pos' =
            match direction with
            | Up -> Vector2f(pos.X, pos.Y - moveUnit)
            | Left -> Vector2f(pos.X - moveUnit, pos.Y)
            | Down -> Vector2f(pos.X, pos.Y + 4f)
            | Right -> Vector2f(pos.X + moveUnit, pos.Y)

        let pos'' =
            Vector2f(pos'.X %% (float32 window_w), pos'.Y %% (float32 (window_h - hudHeight)))

        pos'', pos' <> pos''

    let update commands state =
        let pos = state.Player.Position

        match commands.ChangeDirection with
        | Some direction ->
            let pos, wrapped =
                calcNewPosition pos direction state.WindowDimensions state.HudHeight

            { state with
                  Player = { state.Player with Position = pos }
                  WallCrossings =
                      if wrapped then
                          state.WallCrossings + 1u
                      else
                          state.WallCrossings }
        | None -> state
