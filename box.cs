using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

public class Box
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 VisualPosition;
    // We make the hitbox the exact size of a tile (64x64) based on your level design
    public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 64, 64);

    private float gravity = 0.5f;
    private Texture2D texture;

    public Box(Vector2 startPos, Texture2D tex)
    {
        Position = startPos;
        VisualPosition = startPos;
        texture = tex;
    }

    public void Update(List<Platform> platforms, Player player) 
        {
            // 1. First, check if a platform just appeared and push the box out Left/Right
            PushOutHorizontally(platforms);

            // 2. NEW: Check if the box was just pushed INTO the player
            if (Hitbox.Intersects(player.Hitbox))
            {
                // If the player is to the left of the box, push the player left
                if (player.Position.X < Position.X)
                {
                    player.Position.X = Hitbox.Left - player.Hitbox.Width;
                }
                // If the player is to the right of the box, push the player right
                else 
                {
                    player.Position.X = Hitbox.Right;
                }
            }

            // 3. Apply gravity so the box can fall
            Velocity.Y += gravity;
            Position.Y += Velocity.Y;

            // 4. Check Y-axis collision so the box lands on platforms properly
            foreach (var platform in platforms)
            {
                if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
                {
                    if (Velocity.Y > 0) // If falling down
                    {
                        Position.Y = platform.Hitbox.Top - Hitbox.Height;
                        Velocity.Y = 0;
                    }
                }
            }

            VisualPosition.X = MathHelper.Lerp(VisualPosition.X, Position.X, 0.2f);
            VisualPosition.Y = MathHelper.Lerp(VisualPosition.Y, Position.Y, 0.2f);
        }

    // This gets called by the Player when they walk into the box horizontally
    public void TryPush(float pushAmount, List<Platform> platforms)
    {
        Position.X += pushAmount;

        // Make sure the box stops if it gets pushed into a wall
        foreach (var platform in platforms)
        {
            if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
            {
                if (pushAmount > 0) // Pushed Right
                    Position.X = platform.Hitbox.Left - Hitbox.Width;
                else if (pushAmount < 0) // Pushed Left
                    Position.X = platform.Hitbox.Right;
            }
        }
    }

    private void PushOutHorizontally(List<Platform> platforms)
    {
        foreach (var platform in platforms)
        {
            // If the platform is solid and the box is currently overlapping it
            if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
            {
                // Calculate how much the box is overlapping on the left and right sides
                float pushLeftDist = Hitbox.Right - platform.Hitbox.Left;
                float pushRightDist = platform.Hitbox.Right - Hitbox.Left;

                // Push the box in the direction that has the shortest distance to escape
                if (pushLeftDist < pushRightDist)
                {
                    // It's closer to the left edge, so push it Left
                    Position.X -= pushLeftDist;
                }
                else
                {
                    // It's closer to the right edge, so push it Right
                    Position.X += pushRightDist;
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle drawRect = new Rectangle((int)VisualPosition.X, (int)VisualPosition.Y, 64, 64);
        spriteBatch.Draw(texture, drawRect, Color.SaddleBrown);
    }
}