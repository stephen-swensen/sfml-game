namespace Swensen.SFML.Game

open System
open SFML.System
open SFML.Graphics

module private StateHelpers =
    let boxPosToCenterPos (v: Vector2f) r =
        let x = v.X + r
        let y = v.Y + r
        Vector2f(x, y)

open StateHelpers

type Player =
    { Speed: float32
      ///The position of the top left corner of the bounding box
      Position: Vector2f
      Radius: float32
      Color: Color
      Poisoned: bool }
    ///The position of the center of the circle
    member this.CenterPosition =
        boxPosToCenterPos this.Position this.Radius

type Enemy =
    { ///The position of the top left corner of the bounding box
      Position: Vector2f
      Speed: float32
      Radius: float32
      AliveColor: Color
      EatenColor: Color
      Poison: bool
      Eaten: bool
      Direction: Direction option }
    ///The position of the center of the circle
    member this.CenterPosition =
        boxPosToCenterPos this.Position this.Radius

type LevelState =
    { Player: Player
      Enemies: Enemy list
      WallCrossings: uint
      EnemyCount: int
      ElapsedMs: int64 }

module LevelState =

    ///Calc new position from old position and directional movement
    ///(new pos, true|false wrapped around window)
    let calcNewPosition moveUnit (pos: Vector2f) direction (x, y) =
        let pos' =
            match direction with
            | Up -> Vector2f(pos.X, pos.Y - moveUnit)
            | Left -> Vector2f(pos.X - moveUnit, pos.Y)
            | Down -> Vector2f(pos.X, pos.Y + moveUnit)
            | Right -> Vector2f(pos.X + moveUnit, pos.Y)

        let pos'' =
            Vector2f(pos'.X %% (float32 x), pos'.Y %% (float32 y))

        pos'', pos' <> pos''

    //check if the two (circles) intersect https://stackoverflow.com/a/8367547/236255
    let checkCollision (player: Player) (enemy: Enemy) =
        if enemy.Radius <= 0f then
            false
        else
            let r' = pown (player.Radius - enemy.Radius) 2
            let r'' = pown (player.Radius + enemy.Radius) 2

            let d' =
                (pown (player.CenterPosition.X - enemy.CenterPosition.X) 2)
                + (pown (player.CenterPosition.Y - enemy.CenterPosition.Y) 2)

            r' <= d' && d' <= r''

    let update (boardDimensions: uint * uint) commands levelState =
        let pos = levelState.Player.Position

        let state =
            match commands.ChangeDirection with
            | Some direction ->
                let pos, wrapped =
                    calcNewPosition levelState.Player.Speed pos direction boardDimensions

                { levelState with
                      Player = { levelState.Player with Position = pos }
                      WallCrossings =
                          if wrapped then
                              levelState.WallCrossings + 1u else levelState.WallCrossings }
            | None -> levelState

        let enemies =
            state.Enemies
            //move enemies and reduce radius if needed
            |> Seq.map
                (fun e ->
                    match e.Eaten, e.Direction, e.Speed with
                    | true, _, _
                    | _, None, _ -> e
                    | _, Some (direction), _ ->
                        let pos, _ =
                            calcNewPosition e.Speed e.Position direction boardDimensions

                        let radius =
                            if e.Speed > 2f then
                                e.Radius
                                - ((float32 state.ElapsedMs) / 1_000_000f)
                            else
                                e.Radius

                        { e with
                              Position = pos
                              Radius = radius })
            //check collisions with player
            |> Seq.map
                (fun e ->
                    if e.Eaten || e.Poison then
                        e
                    else
                        let collision = checkCollision state.Player e
                        { e with Eaten = collision })
            |> Seq.filter (fun e -> e.Radius > 0f)
            |> Seq.toList

        let playerPoisoned =
            enemies
            |> List.exists (fun e -> e.Poison && checkCollision state.Player e)

        { state with
            Enemies = enemies
            Player = { state.Player with Poisoned = playerPoisoned} }

    let genEnemies (rnd: unit -> int) level boardDimensions =
        let radius = 20f
        let x, y = boardDimensions
        let x = x - ((uint radius) * 2u)
        let y = y - ((uint radius) * 2u)

        let genDirection () =
            let i = rnd () %% level.EnemyDirections.Length
            level.EnemyDirections.[i]

        let genRandomCoord c = ((rnd () |> uint) %% c) |> float32

        [ for n in 1 .. level.EnemyCount do
              //n.b. circles are drawn from top left corner of bounding box
              let pos =
                  //generate random coords but don't let start in
                  //collision course with player
                  Vector2f(max (genRandomCoord x) (2f*radius), max (genRandomCoord y) (2f*radius))

              let poison = n <= level.PoisonCount
              { Position = pos
                Speed = level.EnemySpeed
                AliveColor = if poison then Color.Magenta else Color.Red
                EatenColor = Color.Blue
                Eaten = false
                Radius = radius
                Poison = poison
                Direction = genDirection () } ]

    let init rnd boardDimensions (level: Level) =

        let levelState =
            { Player =
                  { Speed = 4f
                    Position = Vector2f(0f, 0f)
                    Color = Color.Green
                    Radius = 10f
                    Poisoned = false }
              WallCrossings = 0u
              Enemies = []
              EnemyCount = level.EnemyCount
              ElapsedMs = 0L }

        let state =
            { levelState with
                  Enemies = genEnemies rnd level boardDimensions }

        state
