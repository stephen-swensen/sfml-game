namespace Swensen.SFML.Game

open SFML.Graphics

type Level = {
    EnemyCount: int
    EnemyDirections: (Direction option) list
    StartText: string
}

type Fonts = { DejaVuSansMono: Font }

//todo: make this IDisposable?
type Assets = {
    Fonts: Fonts; Levels: Level list
}

module Assets =
    let loadLevels () = [
        { EnemyCount = 4
          EnemyDirections = [None]
          StartText = "Unidentified Flying Circles abound...\nYou.\nMust.\nEat them all!!" }
        { EnemyCount = 8
          EnemyDirections = [Some Up; Some Down; Some Left; Some Right]
          StartText = "They've learned and they've learned fast...\nTry to keep up!!" }
        { EnemyCount = 14
          EnemyDirections = [Some Up; Some Down; Some Left; Some Right]
          StartText = "Things are getting a little hectic\naround here!" }
    ]

    let loadFonts () =
        let sansMono =
            new Font("assets/fonts/truetype/dejavu/DejaVuSansMono.ttf")
        { DejaVuSansMono = sansMono }

    let load () =
        let fonts = loadFonts ()
        let levels = loadLevels ()
        { Fonts = fonts; Levels = levels }
