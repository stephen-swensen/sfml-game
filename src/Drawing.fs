namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

module Drawing =

    /// Draw active level state to the Window (but don't clear or draw the window itself)
    let drawLevelState assets (window: PollableWindow) winDimensions hudHeight boardDimensions levelState =
        for enemy in levelState.Enemies do
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
                levelState.Player.Radius,
                FillColor = levelState.Player.Color,
                Position = levelState.Player.Position
            )

        window.Draw(player)

        let hudPos =
            Vector2f(
                0f,
                ((snd >> float32) winDimensions)
                - (float32 hudHeight)
            )

        use hud =
            new RectangleShape(
                Vector2f((fst >> float32) boardDimensions, float32 hudHeight),
                FillColor = Color.Cyan,
                Position = hudPos
            )

        window.Draw(hud)

        use hudText = new SFML.Graphics.Text()

        do
            let eatenEnemies =
                levelState.Enemies
                |> Seq.filter (fun e -> e.Eaten)
                |> Seq.length

            hudText.Font <- assets.Fonts.DejaVuSansMono

            hudText.DisplayedString <-
                sprintf
                    $"Wall Crossings: %u{levelState.WallCrossings}, Eaten: %i{eatenEnemies}, Time: %i{levelState.ElapsedMs / 1000L }s"

            hudText.CharacterSize <- 30u
            hudText.Position <- hudPos
            hudText.FillColor <- Color.Black

        window.Draw(hudText)

    let drawState assets (window: PollableWindow) gameState =
        match gameState.PlayState with
        | StartGame text
        | StartLevel text
        | PausedLevel(text, _)
        | EndLevel text
        | EndGame text ->
            use gtext = new SFML.Graphics.Text()
            gtext.Font <- assets.Fonts.DejaVuSansMono
            gtext.DisplayedString <- text
            gtext.CharacterSize <- 30u
            gtext.Position <- Vector2f(0f,0f)
            gtext.FillColor <- Color.Green
            window.Draw(gtext)
        | ActiveLevel levelState ->
            drawLevelState assets window gameState.WindowDimensions gameState.HudHeight gameState.BoardDimensions levelState
