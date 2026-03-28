using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public enum TimeState
{
    Present,
    Past
}

public class Platform
{
    public Rectangle Hitbox;
    public TimeState ActiveTime;
    public bool IsSolid;
    private Texture2D texture;

    public Platform(Rectangle hitbox, TimeState activeTime, Texture2D tex)
    {
        Hitbox = hitbox;
        ActiveTime = activeTime;
        texture = tex;
    }

    public void Update(TimeState currentTime)
    {
        // บล็อกจะแข็ง (ชนได้) ก็ต่อเมื่อเวลาปัจจุบันตรงกับเวลาของบล็อก
        IsSolid = (currentTime == ActiveTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsSolid)
        {
            spriteBatch.Draw(texture, Hitbox, Color.White); // สีเข้ม เหยียบได้
        }
        else
        {
            spriteBatch.Draw(texture, Hitbox, Color.White * 0.3f); // สีจาง ทะลุได้
        }
    }
}