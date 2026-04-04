using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Spike
{
    public Rectangle Hitbox { get; private set; }
    public Rectangle DrawRect { get; private set; }
    public TimeState SpikeTimeState { get; private set; }

    public bool IsDangerous { get; private set; }
    private bool isVisible;

    private Texture2D texture;

    public float Rotatin { get; set; }
    public Vector2 Origin { get; set; }

    public Spike(Rectangle hitbox, Rectangle drawRect, TimeState timeState, Texture2D tex, float rotation = 0f, Vector2? origin = null)
    {
        Hitbox = hitbox;
        DrawRect = drawRect;
        SpikeTimeState = timeState;
        texture = tex;
        Rotatin = rotation;
        Origin = origin ?? Vector2.Zero;
    }

    public void Update(TimeState currentTime)
    {
        // 
        if (SpikeTimeState == TimeState.Permanent || SpikeTimeState == currentTime)
        {
            IsDangerous = true;
            isVisible = true;
        }
        else
        {
            IsDangerous = false;
            isVisible = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (isVisible)
        {
            spriteBatch.Draw(texture, DrawRect, null, Color.White, Rotatin, Origin, SpriteEffects.None, 0f);
        }
    }
}