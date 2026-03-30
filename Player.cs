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
    private float gravity = 0.5f;
    private float jumpForce = -12f;
    private bool isGrounded;
    public bool isDoubleJump = false;
    private Texture2D texture;
    private KeyboardState previousKey;

    public Vector2 SpawnPoint;

    public Player(Vector2 startPos, Texture2D tex)
    {
        Position = startPos;
        texture = tex;
        SpawnPoint = startPos;
    }

    public void Die()
    {
        Position = SpawnPoint;
        Velocity = Vector2.Zero;
    }

    public void Update(List<Platform> platforms)
    {
        KeyboardState kState = Keyboard.GetState();

        // 1. การเคลื่อนที่ซ้าย-ขวา
        Velocity.X = 0;
        if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) Velocity.X = -speed;
        if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) Velocity.X = speed;

        Rectangle groundCheck = new Rectangle((int)Position.X, (int)Position.Y + Hitbox.Height, Hitbox.Width, 2);
        isGrounded = false;

        foreach (var platform in platforms)
        {
            if (platform.IsSolid && groundCheck.Intersects(platform.Hitbox))
            {
                isGrounded = true;
                isDoubleJump = false;
                break;
            }
        }
        bool justPressedSpace = kState.IsKeyDown(Keys.Space) && previousKey.IsKeyUp(Keys.Space);

        // 2. กระโดด (ทำได้เมื่ออยู่บนพื้น)
        if (justPressedSpace && isGrounded)
        {
            Velocity.Y = jumpForce;
        }
        // 2. Double Jump (ทำได้เมื่ออยู่กลางอากาศ)
        else if (justPressedSpace && !isGrounded && !isDoubleJump)
        {
            Velocity.Y = jumpForce;
            isDoubleJump = true;
        }

        Velocity.Y += gravity;

        Position.X += Velocity.X;
        CheckCollision(platforms, true);

        Position.Y += Velocity.Y;
        CheckCollision(platforms, false);

        previousKey = kState;
    }

    private void CheckCollision(List<Platform> platforms, bool isHorizontal)
    {
        foreach (var platform in platforms)
        {
            if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
            {
                if (isHorizontal)
                {
                    // ชนซ้าย-ขวา
                    if (Velocity.X > 0) Position.X = platform.Hitbox.Left - Hitbox.Width;
                    else if (Velocity.X < 0) Position.X = platform.Hitbox.Right;
                }
                else
                {
                    // ชนบน-ล่าง
                    if (Velocity.Y > 0)
                    {
                        Position.Y = platform.Hitbox.Top - Hitbox.Height;
                    }
                    else if (Velocity.Y < 0)
                    {
                        Position.Y = platform.Hitbox.Bottom;
                    }
                    Velocity.Y = 0; // หยุดความเร็วแกน Y เมื่อชน
                }
            }
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
        spriteBatch.Draw(texture, Hitbox, Color.Blue);
    }
}