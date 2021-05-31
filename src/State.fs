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

    //check if the two (circles) intersect https://stackoverflow.com/a/8367547/236255
    let checkCollision (player: Player) (enemy: Enemy) =
        let r' = pown (player.Radius - enemy.Radius) 2
        let r'' = pown (player.Radius + enemy.Radius) 2

        let d' =
            (pown (player.Position.X - enemy.Position.X) 2)
            + (pown (player.Position.Y - enemy.Position.Y) 2)

        r' <= d' && d' <= r''

    let update commands state =
        let pos = state.Player.Position

        let state =
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

        let enemies =
            state.Enemies
            |> List.map
                (fun e ->
                    if e.Eaten then
                        e
                    else
                        let collision = checkCollision state.Player e
                        { e with Eaten = collision })

        { state with Enemies = enemies }
