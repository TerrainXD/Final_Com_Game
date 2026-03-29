using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


public class Spike
{
    public Rectangle Hitbox;
    public TimeState ActiveTime;
    public bool IsDangerous;
    private Texture2D texture;

    public Spike(Rectangle hitbox, TimeState activeTime, Texture2D tex)
    {
        Hitbox = hitbox;
        ActiveTime = activeTime;
        texture = tex;
    }

    public void Update(TimeState currentTime)
    {
        if (ActiveTime == TimeState.Permanent)
        {
            IsDangerous = true;
        }
        else
        {
            IsDangerous = (currentTime == ActiveTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsDangerous)
        {
            spriteBatch.Draw(texture, Hitbox, Color.Red); // สีเข้ม เหยียบได้
        }
        else
        {
            spriteBatch.Draw(texture, Hitbox, Color.Red * 0.3f); // สีจาง ทะลุได้
        }
    }
}