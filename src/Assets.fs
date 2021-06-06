namespace Swensen.SFML.Game

open SFML.Graphics

type Level = {
    EnemyCount: int
    EnemyDirections: (Direction option) list
    StartText: string
}

type Fonts = { DejaVuSansMono: Font }

//todo: make this IDisposable?
type Assets = { Fonts: Fonts; Levels: Level list }

module Assets =

    let load () =
        let fonts =
            let sansMono =
                new Font("assets/fonts/truetype/dejavu/DejaVuSansMono.ttf")
            { DejaVuSansMono = sansMono }

        let levels = [
            { EnemyCount = 8
              EnemyDirections = [None]
              StartText = "Unidentified Flying Circles abound - you must eat them all!!" }
            { EnemyCount = 8
              EnemyDirections = [Some Up; Some Down; Some Left; Some Right]
              StartText = "They've learned and they've learned fast - try to keep up!!" }
        ]

        { Fonts = fonts; Levels = levels }
