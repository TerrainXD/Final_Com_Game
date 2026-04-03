using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

public class Box
{
    public Vector2 Position;
    public Vector2 Velocity;
    public int frameWidth = 28;
    public int frameHeight = 24;
    public int actualBoxWidth = 16;
    public int actualBoxHeight = 22;
    private float scale = 2.5f;

    public int HitboxWidth;
    public int HitboxHeight;
    public int DrawWidth;
    public int DrawHeight;

    public Vector2 VisualPosition;
    public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, HitboxWidth, HitboxHeight);

    private float gravity = 0.5f;
    private Texture2D texture;
    public Rectangle sourceRect;
    private int totalFrames = 4;
    public Box(Vector2 startPos, Texture2D tex)
    {
        Position = startPos;
        VisualPosition = startPos;
        texture = tex;
        DrawWidth = (int)(frameWidth * scale);
        DrawHeight = (int)(frameHeight * scale);

        HitboxWidth = (int)(actualBoxWidth * scale);
        HitboxHeight = (int)(actualBoxHeight * scale);
        int lastFrameX = (totalFrames - 1) * frameWidth;
        sourceRect = new Rectangle(lastFrameX, 0, frameWidth, frameHeight);
    }

    public void Update(List<Platform> platforms, Player player)
    {
        PushOutHorizontally(platforms);

        if (Hitbox.Intersects(player.Hitbox))
        {
            if (player.Position.X < Position.X)
            {
                player.Position.X = Hitbox.Left - player.Hitbox.Width;
            }
            else
            {
                player.Position.X = Hitbox.Right;
            }
        }

        Velocity.Y += gravity;
        Position.Y += Velocity.Y;

        foreach (var platform in platforms)
        {
            if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
            {
                if (Velocity.Y > 0) 
                {
                    Position.Y = platform.Hitbox.Top - Hitbox.Height;
                    Velocity.Y = 0;
                }
            }
        }

        VisualPosition.X = MathHelper.Lerp(VisualPosition.X, Position.X, 0.2f);
        VisualPosition.Y = MathHelper.Lerp(VisualPosition.Y, Position.Y, 0.2f);
    }

    public void TryPush(float pushAmount, List<Platform> platforms)
    {
        Position.X += pushAmount;

        foreach (var platform in platforms)
        {
            if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
            {
                if (pushAmount > 0) 
                    Position.X = platform.Hitbox.Left - Hitbox.Width;
                else if (pushAmount < 0) 
                    Position.X = platform.Hitbox.Right;
            }
        }
    }

    private void PushOutHorizontally(List<Platform> platforms)
    {
        foreach (var platform in platforms)
        {
            if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
            {
                float pushLeftDist = Hitbox.Right - platform.Hitbox.Left;
                float pushRightDist = platform.Hitbox.Right - Hitbox.Left;

                if (pushLeftDist < pushRightDist)
                {
                    Position.X -= pushLeftDist;
                }
                else
                {
                    Position.X += pushRightDist;
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int offsetX = (DrawWidth - HitboxWidth) / 2;
        int offsetY = DrawHeight - HitboxHeight;

        Rectangle drawRect = new Rectangle(
            (int)VisualPosition.X - offsetX,
            (int)VisualPosition.Y - offsetY,
            DrawWidth,
            DrawHeight);

        spriteBatch.Draw(texture, drawRect, sourceRect, Color.White);
    }
}