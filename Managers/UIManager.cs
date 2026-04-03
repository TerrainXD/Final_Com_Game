using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class UIManager
{
    private SpriteFont font;

    public UIManager(SpriteFont font)
    {
        this.font = font;
    }

    public void Draw(SpriteBatch spriteBatch, Player player, TimeState timeState)
    {
        spriteBatch.DrawString(font, "Double Jump Used: " + player.isDoubleJump, new Vector2(20, 60), Color.Black);
    }

    // ✨ อัปเดตเมธอด DrawGameOver ใน UIManager.cs
    public void DrawGameOver(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        string text = "YOU FAIL!";
        string subText = "Press ENTER to Restart";

        float mainTextScale = 4.0f;
        float subTextScale = 1.0f;

        Vector2 textBounds = font.MeasureString(text);
        Vector2 subTextBounds = font.MeasureString(subText);

        Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);


        Vector2 textOrigin = textBounds / 2;
        Vector2 subTextOrigin = subTextBounds / 2;


        spriteBatch.DrawString(font, text, screenCenter + new Vector2(4 * mainTextScale, 4 * mainTextScale),
            Color.Black, 0f, textOrigin, mainTextScale, SpriteEffects.None, 0f);

        spriteBatch.DrawString(font, text, screenCenter,
            Color.Red, 0f, textOrigin, mainTextScale, SpriteEffects.None, 0f);

        Vector2 subTextPos = screenCenter + new Vector2(0, 80);

        spriteBatch.DrawString(font, subText, subTextPos + new Vector2(2, 2),
            Color.Black, 0f, subTextOrigin, subTextScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, subText, subTextPos,
            Color.White, 0f, subTextOrigin, subTextScale, SpriteEffects.None, 0f);
    }
}