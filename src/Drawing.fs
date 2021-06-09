namespace Swensen.SFML.Game

open SFML.System
open SFML.Graphics

module Drawing =
    ///Draw the heads up display
    let drawHud assets (window: PollableWindow) gameState levelState =
        let hudPos =
            Vector2f(
                0f,
                ((snd >> float32) gameState.WindowDimensions)
                - (float32 gameState.HudHeight)
            )

        do
            use hud =
                new RectangleShape(
                    Vector2f((fst >> float32) gameState.BoardDimensions, float32 gameState.HudHeight),
                    FillColor = Color.Cyan,
                    Position = hudPos
                )

            window.Draw(hud)

        do
            use hudText = new Text()

            let eatenEnemies =
                levelState.Enemies
                |> Seq.filter (fun e -> e.Eaten)
                |> Seq.length

            hudText.Font <- assets.Fonts.DejaVuSansMono

            hudText.DisplayedString <-
                sprintf
                    $"Level: %i{gameState.CurrentLevelIndex + 1}/%i{assets.Levels.Length}   \
                      Eaten: %i{eatenEnemies}   \
                      Time: %i{levelState.ElapsedMs / 1000L}s"

            hudText.CharacterSize <- 30u
            hudText.Position <- hudPos
            hudText.FillColor <- Color.Black
            window.Draw(hudText)

    ///Draw players and enemies on the board
    let drawBoard (window: PollableWindow) levelState =
        for enemy in levelState.Enemies do
            use e =
                new CircleShape(enemy.Radius, FillColor = enemy.Color, Position = enemy.Position)

            window.Draw(e)

        use player =
            new CircleShape(
                levelState.Player.Radius,
                FillColor = levelState.Player.Color,
                Position = levelState.Player.Position
            )

        window.Draw(player)

    /// Draw active level state to the Window (but don't clear or draw the window itself)
    let drawState assets (window: PollableWindow) gameState =
        let drawText text =
            use gtext = new Text()
            gtext.Font <- assets.Fonts.DejaVuSansMono
            gtext.DisplayedString <- text
            gtext.CharacterSize <- 30u
            gtext.Position <- Vector2f(0f, 0f)
            gtext.FillColor <- Color.Green
            window.Draw(gtext)

        match gameState.PlayState with
        | StartGame text
        | StartLevel text -> drawText text
        | EndGame (text, levelState, _)
        | EndLevel (text, levelState) ->
            drawHud assets window gameState levelState
            drawText text
        | ActiveLevel levelState ->
            drawBoard window levelState
            drawHud assets window gameState levelState
        | PausedLevel (text, levelState) ->
            drawBoard window levelState
            drawHud assets window gameState levelState
            drawText text
