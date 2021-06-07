namespace Swensen.SFML.Game

type Level =
    { EnemySpeed: float32
      EnemyCount: int
      EnemyDirections: (Direction option) list
      StartText: string }

module Levels =
    let loadAll () =
        [ { EnemySpeed = 1.5f
            EnemyCount = 4
            EnemyDirections =
                [ Some Up
                  Some Down
                  Some Left
                  Some Right ]
            StartText = "Circles abound...\nYou.\nMust.\nEat them all!!" }
          { EnemySpeed = 4f
            EnemyCount = 6
            EnemyDirections =
                [ Some Up
                  Some Down
                  Some Left
                  Some Right ]
            StartText = "They've learned and they've learned fast...\nTry to keep up!!" }
          { EnemySpeed = 6f
            EnemyCount = 8
            EnemyDirections =
                [ Some Up
                  Some Down
                  Some Left
                  Some Right ]
            StartText = "Things are getting a little hectic\naround here!" } ]
