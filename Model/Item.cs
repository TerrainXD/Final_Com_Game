using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Item
{
    public Rectangle Hitbox;
    public Texture2D texture;

    public Item(Rectangle hitbox, Texture2D tex)
    {
        Hitbox = hitbox;
        texture = tex;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, Hitbox, Color.Pink);
    }
}