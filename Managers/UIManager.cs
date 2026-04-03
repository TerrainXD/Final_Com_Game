using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FinalProject.Managers
{
    public class UIManager
    {
        private SpriteFont font;
        private Vector2 screenSize;

        private Texture2D playButtonTex;
        private Texture2D closeButtonTex;
        private Texture2D restartButtonTex;

        private Texture2D overlayTex;

        private Rectangle startButtonRect;
        private Rectangle exitButtonRect;
        private Rectangle restartButtonRect;
        private Rectangle backToMenuButtonRect;

        private Color menuTitleColor = new Color(100, 200, 255);
        private Color winTitleColor = Color.Gold;
        private Color statsColor = Color.White;
        private Color buttonHoverColor = Color.Gray;

        private int buttonScale = 4;

        public UIManager(SpriteFont font, ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.font = font;
            screenSize = new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            playButtonTex = content.Load<Texture2D>("UI/Play");
            closeButtonTex = content.Load<Texture2D>("UI/Close");
            restartButtonTex = content.Load<Texture2D>("UI/Restart");

            overlayTex = new Texture2D(graphicsDevice, 1, 1);
            overlayTex.SetData(new[] { Color.Black });

            InitializeButtonPositions();
        }

        private void InitializeButtonPositions()
        {
            int btnWidth = playButtonTex.Width * buttonScale;
            int btnHeight = playButtonTex.Height * buttonScale;
            int spacing = 30;
            int centerX = (int)(screenSize.X / 2 - btnWidth / 2);

            startButtonRect = new Rectangle(centerX, (int)(screenSize.Y / 2) - 20, btnWidth, btnHeight);
            exitButtonRect = new Rectangle(centerX, startButtonRect.Y + btnHeight + spacing, btnWidth, btnHeight);

            restartButtonRect = new Rectangle(centerX, (int)(screenSize.Y / 2 + 100), btnWidth, btnHeight);
            backToMenuButtonRect = new Rectangle(centerX, restartButtonRect.Y + btnHeight + spacing, btnWidth, btnHeight);
        }

        public void DrawMainMenu(SpriteBatch spriteBatch, MouseState mouseState)
        {
            spriteBatch.Draw(overlayTex, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.Black * 0.5f);

            DrawCenteredText(spriteBatch, "DIMENSIONAL SHIFT", screenSize.Y / 4 - 30, menuTitleColor, 3.5f);

            DrawButton(spriteBatch, playButtonTex, startButtonRect, mouseState);
            DrawButton(spriteBatch, closeButtonTex, exitButtonRect, mouseState);
        }

        public void DrawGameWon(SpriteBatch spriteBatch, MouseState mouseState, float totalTime, int failedCount)
        {
            spriteBatch.Draw(overlayTex, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.Black * 0.7f);

            DrawCenteredText(spriteBatch, "CONGRATULATIONS", screenSize.Y / 4 - 60, winTitleColor, 2.8f);

            TimeSpan time = TimeSpan.FromSeconds(totalTime);
            string timeText = "Total Time: " + time.ToString(@"mm\:ss\.fff");
            string failedText = "Total Failed: " + failedCount;

            DrawCenteredText(spriteBatch, timeText, screenSize.Y / 2 - 80, statsColor, 1.5f);
            DrawCenteredText(spriteBatch, failedText, screenSize.Y / 2 - 40, statsColor, 1.5f);

            DrawButton(spriteBatch, restartButtonTex, restartButtonRect, mouseState);
            DrawButton(spriteBatch, closeButtonTex, backToMenuButtonRect, mouseState);
        }

        public void DrawGameOver(SpriteBatch spriteBatch, MouseState mouseState)
        {
            spriteBatch.Draw(overlayTex, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.Black * 0.5f);

            DrawCenteredText(spriteBatch, "YOU FAIL!", screenSize.Y / 2 - 50, Color.Red, 3.5f);

            DrawCenteredText(spriteBatch, "Press ENTER to Restart", screenSize.Y / 2 + 50, Color.White, 1.5f);
        }
        private void DrawCenteredText(SpriteBatch spriteBatch, string text, float posY, Color color, float scale)
        {
            Vector2 textSize = font.MeasureString(text) * scale;
            Vector2 position = new Vector2((screenSize.X / 2) - (textSize.X / 2), posY);

            spriteBatch.DrawString(font, text, position + new Vector2(3 * scale, 3 * scale), Color.Black * 0.6f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawButton(SpriteBatch spriteBatch, Texture2D texture, Rectangle rect, MouseState mouseState)
        {
            Color drawColor = Color.White;
            if (rect.Contains(mouseState.Position)) drawColor = buttonHoverColor;
            spriteBatch.Draw(texture, rect, drawColor);
        }

        public bool IsStartClicked(MouseState mouse) => mouse.LeftButton == ButtonState.Pressed && startButtonRect.Contains(mouse.Position);
        public bool IsExitClicked(MouseState mouse) => mouse.LeftButton == ButtonState.Pressed && exitButtonRect.Contains(mouse.Position);
        public bool IsRestartClicked(MouseState mouse) => mouse.LeftButton == ButtonState.Pressed && restartButtonRect.Contains(mouse.Position);
        public bool IsBackClicked(MouseState mouse) => mouse.LeftButton == ButtonState.Pressed && backToMenuButtonRect.Contains(mouse.Position);
    }
}