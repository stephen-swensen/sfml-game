namespace Swensen.SFML.Game

type Level =
    { Name: string
      EnemyCount: int
      EnemySpeed: int
      Directions: Direction list }

module Levels =

    let genLevels () = []
