namespace Swensen.SFML.Game

open SFML.Graphics

type Fonts = { DejaVuSansMono: Font }

//todo: make this IDisposable?
type Assets = { Fonts: Fonts; Levels: Level list }

module Assets =
    let levels = Levels.loadAll ()

    let loadFonts () =
        let sansMono =
            new Font("assets/fonts/truetype/dejavu/DejaVuSansMono.ttf")

        { DejaVuSansMono = sansMono }

    let load () =
        let fonts = loadFonts ()
        { Fonts = fonts; Levels = levels }
