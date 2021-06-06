namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

module Drawing =

    /// Draw state to the Window (but don't clear or draw the window itself)
    let drawState assets (window: PollableWindow) state =
        for enemy in state.Enemies do
            use e =
                new CircleShape(
                    enemy.Radius,
                    FillColor =
                        (if enemy.Eaten then
                             enemy.EatenColor
                         else
                             enemy.AliveColor),
                    Position = enemy.Position
                )

            window.Draw(e)

        use player =
            new CircleShape(
                state.Player.Radius,
                FillColor = state.Player.Color,
                Position = state.Player.Position
            )

        window.Draw(player)

        let hudPos =
            Vector2f(
                0f,
                ((snd >> float32) state.WindowDimensions)
                - (float32 state.HudHeight)
            )

        use hud =
            new RectangleShape(
                Vector2f((fst >> float32) state.WindowDimensions, float32 state.HudHeight),
                FillColor = Color.Cyan,
                Position = hudPos
            )

        window.Draw(hud)

        use hudText = new SFML.Graphics.Text()

        do
            let eatenEnemies =
                state.Enemies
                |> Seq.filter (fun e -> e.Eaten)
                |> Seq.length

            hudText.Font <- assets.Fonts.DejaVuSansMono

            hudText.DisplayedString <-
                sprintf
                    $"Wall Crossings: %u{state.WallCrossings}, Eaten: %i{eatenEnemies}, Time: %i{
                                                                                                     state.ElapsedMs
                                                                                                     / 1000L
                    }s"

            hudText.CharacterSize <- 30u
            hudText.Position <- hudPos
            hudText.FillColor <- Color.Black

        window.Draw(hudText)
