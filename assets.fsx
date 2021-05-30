[<AutoOpen>]
module assets_fsx

#load "refs.fsx"

open SFML.Graphics

type Fonts = { DejaVuSansMono: Font }

//todo: make this IDisposable?
type Assets = { Fonts: Fonts }

module Assets =

    let load () =
        let sansMono =
            new Font("assets/fonts/truetype/dejavu/DejaVuSansMono.ttf")

        { Fonts = { DejaVuSansMono = sansMono } }
