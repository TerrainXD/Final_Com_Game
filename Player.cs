using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

public class Player
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

    private float speed = 5f;
    private float gravity = 0.5f; // ตัวละครต้องมีผลจากแรงโน้มถ่วง 
    private float jumpForce = -12f;
    private bool isGrounded;
    private Texture2D texture;

    public Vector2 SpawnPoint;
    public Vector2 VisualPosition;
    public Player(Vector2 startPos, Texture2D tex)
    {
        Position = startPos;
        texture = tex;
        SpawnPoint = startPos;
        VisualPosition = startPos;
    }

    public void Die()
    {
        Position = SpawnPoint;
        VisualPosition = SpawnPoint;
        Velocity = Vector2.Zero;
    }

    public void Update(List<Platform> platforms, List<Box> boxes) // <-- Add List<Box> here
        {
            KeyboardState kState = Keyboard.GetState();

            // 1. Horizontal Movement
            Velocity.X = 0;
            if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) Velocity.X = -speed;
            if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) Velocity.X = speed;

            // 2. Jumping
            if (kState.IsKeyDown(Keys.Space) && isGrounded)
            {
                Velocity.Y = jumpForce;
                isGrounded = false;
            }

            Velocity.Y += gravity;

            // 4. Update X and check collisions
            Position.X += Velocity.X;
            CheckCollision(platforms, boxes, true);

            // 5. Update Y and check collisions
            Position.Y += Velocity.Y;
            isGrounded = false; // Reset before checking
            CheckCollision(platforms, boxes, false);
            VisualPosition.X = MathHelper.Lerp(VisualPosition.X, Position.X, 0.2f);
            VisualPosition.Y = MathHelper.Lerp(VisualPosition.Y, Position.Y, 0.2f);
        }

    private void CheckCollision(List<Platform> platforms, List<Box> boxes, bool isHorizontal)
        {
            // Check collision against platforms
            foreach (var platform in platforms)
            {
                if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
                {
                    ResolveCollision(platform.Hitbox, isHorizontal);
                }
            }

            // Check collision against pushable boxes
            foreach (var box in boxes)
            {
                if (Hitbox.Intersects(box.Hitbox))
                {
                    if (isHorizontal)
                    {
                        // Push the box!
                        box.TryPush(Velocity.X, platforms);
                        
                        // Stop the player so they don't walk through the box
                        ResolveCollision(box.Hitbox, true);
                    }
                    else
                    {
                        // Treat the box like a normal platform on the Y axis (so you can stand on it)
                        ResolveCollision(box.Hitbox, false);
                    }
                }
            }
        }

    private void ResolveCollision(Rectangle targetHitbox, bool isHorizontal)
        {
            if (isHorizontal)
            {
                if (Velocity.X > 0) Position.X = targetHitbox.Left - Hitbox.Width;
                else if (Velocity.X < 0) Position.X = targetHitbox.Right;
            }
            else
            {
                if (Velocity.Y > 0)
                {
                    Position.Y = targetHitbox.Top - Hitbox.Height;
                    isGrounded = true; 
                }
                else if (Velocity.Y < 0)
                {
                    Position.Y = targetHitbox.Bottom;
                }
                Velocity.Y = 0; 
            }
        }

    public void CheckSpikeCollision(List<Spike> spikes)
    {
        foreach (var spike in spikes)
        {
            if (spike.IsDangerous && Hitbox.Intersects(spike.Hitbox))
            {
                Die();
                break;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
        {
            // NEW: Make sure your player's Draw method uses the VisualPosition!
            Rectangle drawRect = new Rectangle((int)VisualPosition.X, (int)VisualPosition.Y, 32, 32);
            spriteBatch.Draw(texture, drawRect, Color.White); 
        }
}