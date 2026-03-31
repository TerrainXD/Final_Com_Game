using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class UIManager
{
    private SpriteFont font;
    private Texture2D heartTexture;

    public UIManager(SpriteFont font, Texture2D heartTexture)
    {
        this.font = font;
        this.heartTexture = heartTexture;
    }

    public void Draw(SpriteBatch spriteBatch, Player player, TimeState timeState)
    {
        for (int i = 0; i < player.HP; i++)
        {
            Rectangle heartRect = new Rectangle(20 + i * 40, 50, 32, 32);
            spriteBatch.Draw(heartTexture, heartRect, Color.White);
        }

        spriteBatch.DrawString(font, "Double Jump Used: " + player.isDoubleJump, new Vector2(20, 60), Color.Black);
    }
}