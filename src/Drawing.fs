namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

module Drawing =

    let drawState assets (window: PollableWindow) state =
        window.Clear()

        for enemy in state.Enemies do
            use e =
                new CircleShape(enemy.Radius, FillColor = enemy.AliveColor, Position = enemy.Position)

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
            hudText.Font <- assets.Fonts.DejaVuSansMono
            hudText.DisplayedString <- sprintf $"Wall Crossings: %u{state.WallCrossings}"
            hudText.CharacterSize <- 30u
            hudText.Position <- hudPos
            hudText.FillColor <- Color.Black

        window.Draw(hudText)
        window.Display()
