namespace Swensen.SFML.Game

type Level =
    { EnemyCount: int
      EnemyDirections: (Direction option) list
      StartText: string }

module Levels =
    let loadAll () =
        [ { EnemyCount = 4
            EnemyDirections = [ None ]
            StartText = "Circles abound...\nYou.\nMust.\nEat them all!!" }
          { EnemyCount = 8
            EnemyDirections =
                [ Some Up
                  Some Down
                  Some Left
                  Some Right ]
            StartText = "They've learned and they've learned fast...\nTry to keep up!!" }
          { EnemyCount = 14
            EnemyDirections =
                [ Some Up
                  Some Down
                  Some Left
                  Some Right ]
            StartText = "Things are getting a little hectic\naround here!" } ]
