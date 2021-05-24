#load "refs.fsx"

open SFML.System
open SFML.Graphics
open SFML.Window

let window = new RenderWindow(new VideoMode(400u, 400u), "SFML works!")
let shape = new CircleShape(10.0f, FillColor=Color.Green)
let mutable pressedKey = Keyboard.Key.Unknown

let moveKeys = [ Keyboard.Key.Up; Keyboard.Key.Left;
                 Keyboard.Key.Down; Keyboard.Key.Right ]

let keyPress (e : KeyEventArgs) =
    pressedKey <-
        if moveKeys |> List.contains e.Code then
            e.Code
        else
            Keyboard.Key.Unknown

let keyRelease (e : KeyEventArgs) =
    let pressedKeys = List.filter (fun key -> Keyboard.IsKeyPressed(key)) moveKeys
    if pressedKeys.IsEmpty then pressedKey <- Keyboard.Key.Unknown
    else pressedKey <- pressedKeys.Head

window.Closed.Add(fun evArgs -> window.Close())
window.KeyPressed.Add(keyPress)
window.KeyReleased.Add(keyRelease)

while window.IsOpen do
    window.DispatchEvents()

    match pressedKey with
    | Keyboard.Key.Up    -> shape.Position <- new Vector2f(shape.Position.X, shape.Position.Y - 0.1f)
    | Keyboard.Key.Left  -> shape.Position <- new Vector2f(shape.Position.X - 0.1f, shape.Position.Y)
    | Keyboard.Key.Down  -> shape.Position <- new Vector2f(shape.Position.X, shape.Position.Y + 0.1f)
    | Keyboard.Key.Right -> shape.Position <- new Vector2f(shape.Position.X + 0.1f, shape.Position.Y)
    | _ -> ()

    window.Clear()
    window.Draw(shape)
    window.Display()
