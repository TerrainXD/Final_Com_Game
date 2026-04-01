
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Particle
{
    public Rectangle DrawRect;
    public Rectangle SourceRect;
    public SpriteEffects FlipEffect;
    public Texture2D Texture;
    public float Opacity;
    public Color ParticleColor;
    public float FadeRate;


    public Particle(Rectangle drawRect, Rectangle source, SpriteEffects flip, Texture2D tex, Color color, float startOpacity = 0.6f, float fadeRate = 0.05f)
    {
        DrawRect = drawRect;
        SourceRect = source;
        FlipEffect = flip;
        Texture = tex;
        ParticleColor = color;
        Opacity = startOpacity;
        FadeRate = fadeRate;
    }

    public void Update()
    {
        Opacity -= FadeRate;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Opacity > 0)
        {
            spriteBatch.Draw(Texture, DrawRect, SourceRect, ParticleColor * Opacity, 0f, Vector2.Zero, FlipEffect, 0f);
        }
    }
}