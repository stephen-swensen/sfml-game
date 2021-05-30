namespace Swensen.SFML.Game

[<AutoOpen>]
module Prelude =
    let inline (%%) n m = ((n % m) + m) % m
