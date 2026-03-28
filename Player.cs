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
    private float jumpForce = -10f;
    private bool isGrounded;
    private Texture2D texture;

    public Player(Vector2 startPos, Texture2D tex)
    {
        Position = startPos;
        texture = tex;
    }

    public void Update(List<Platform> platforms)
    {
        KeyboardState kState = Keyboard.GetState();

        // 1. การเคลื่อนที่ซ้าย-ขวา
        Velocity.X = 0;
        if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) Velocity.X = -speed;
        if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) Velocity.X = speed;

        // 2. กระโดด (ทำได้เมื่ออยู่บนพื้น)
        if (kState.IsKeyDown(Keys.Space) && isGrounded)
        {
            Velocity.Y = jumpForce;
            isGrounded = false;
        }

        Velocity.Y += gravity;

        // 4. อัปเดตตำแหน่งแกน X และเช็คชน
        Position.X += Velocity.X;
        CheckCollision(platforms, true);

        // 5. อัปเดตตำแหน่งแกน Y และเช็คชน
        Position.Y += Velocity.Y;
        isGrounded = false; // รีเซ็ตก่อนเช็คชน
        CheckCollision(platforms, false);
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
                        isGrounded = true; // เหยียบพื้นแล้ว
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

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, Hitbox, Color.Blue);
    }
}