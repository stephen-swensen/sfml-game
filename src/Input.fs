namespace Swensen.SFML.Game

open SFML.Window

type Direction =
    | Up
    | Down
    | Left
    | Right

type InputCommands =
    { ChangeDirection: Direction option
      CloseWindow: bool
      Continue: bool }

module Input =

    ///Apply a single event to some existing command state, producing a new command state
    let private applyEvent commands (event: Event) =

        let keyMapping =
            [ Keyboard.Key.Up, Up
              Keyboard.Key.Left, Left
              Keyboard.Key.Right, Right
              Keyboard.Key.Down, Down ]

        match event.Type with
        | EventType.Closed -> { commands with CloseWindow = true }
        | EventType.KeyPressed ->
            let direction =
                keyMapping
                |> Seq.tryPick
                    (fun (code, direction) ->
                        if code = event.Key.Code then
                            Some direction
                        else
                            None)

            match direction with
            | Some _ as mp -> { commands with ChangeDirection = mp }
            | None when event.Key.Code = Keyboard.Key.Escape -> { commands with CloseWindow = true }
            | None when event.Key.Code = Keyboard.Key.Enter -> { commands with Continue = true }
            | None -> commands

        | EventType.KeyReleased ->
            let direction =
                keyMapping
                |> Seq.tryPick
                    (fun (code, direction) ->
                        if Keyboard.IsKeyPressed(code) then
                            Some direction
                        else
                            None)

            { commands with
                  ChangeDirection = direction }
        | _ -> commands

    let pollEvents (window: PollableWindow) commands = window.PollEvents(commands, applyEvent)
