namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

module private StateHelpers =
    let boxPosToCenterPos (v:Vector2f) r =
        let x = v.X + r
        let y = v.Y + r
        Vector2f(x,y)

open StateHelpers

type Player =
    { ///The position of the top left corner of the bounding box
      Position: Vector2f
      Radius: float32
      Color: Color }
with
    ///The position of the center of the circle
    member this.CenterPosition =
        boxPosToCenterPos this.Position this.Radius

type Enemy =
    { ///The position of the top left corner of the bounding box
      Position: Vector2f
      Radius: float32
      AliveColor: Color
      EatenColor: Color
      Eaten: bool
      Direction: Direction option
      ///Whether or not should bounce off of walls
      Bouncy: bool }
with
    ///The position of the center of the circle
    member this.CenterPosition =
        boxPosToCenterPos this.Position this.Radius

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
            (pown (player.CenterPosition.X - enemy.CenterPosition.X) 2)
            + (pown (player.CenterPosition.Y - enemy.CenterPosition.Y) 2)

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
            //move enemies
            |> Seq.map (fun e ->
                match e.Eaten, e.Direction with
                | true, _ | _, None ->
                    e
                | _, Some(direction) ->
                    let pos, _ =
                        calcNewPosition e.Position direction state.BoardDimensions
                    { e with Position = pos }

            )
            //check collisions with player
            |> Seq.map
                (fun e ->
                    if e.Eaten then
                        e
                    else
                        let collision = checkCollision state.Player e
                        { e with Eaten = collision })
            |> Seq.toList

        { state with Enemies = enemies }
