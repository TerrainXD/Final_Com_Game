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
    public bool hasWallJump = true;
    private float wallJumpForce = 5f;  // ความแรงในการพุ่งออกข้าง (ปรับให้เยอะกว่า speed ปกติได้)
    private int wallJumpTimer = 0;     // ตัวนับเวลาพุ่ง
    private int wallJumpLockTime = 12; // ระยะเวลาล็อคปุ่ม (12 เฟรม = ประมาณ 0.2 วินาที)
    private float forcedDirection = 0f;// ทิศทางที่ถูกบังคับพุ่ง (ซ้ายหรือขวา)
    private bool isTouchingLeftWall;
    private bool isTouchingRightWall;
    private float wallSlideWall = 2f;
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

        Velocity.X = 0;
        if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) Velocity.X = -speed;
        if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) Velocity.X = speed;

        Rectangle groundCheck = new Rectangle((int)Position.X, (int)Position.Y + 1, Hitbox.Width, Hitbox.Height);
        isGrounded = false;
        Rectangle leftCheck = new Rectangle((int)Position.X - 1, (int)Position.Y, Hitbox.Width, Hitbox.Height);
        isTouchingLeftWall = false;
        Rectangle rightCheck = new Rectangle((int)Position.X + 1, (int)Position.Y, Hitbox.Width, Hitbox.Height);
        isTouchingRightWall = false;

        foreach (var platform in platforms)
        {
            if (platform.IsSolid)
            {
                if (groundCheck.Intersects(platform.Hitbox)) isGrounded = true;
                if (leftCheck.Intersects(platform.Hitbox) && !isGrounded) isTouchingLeftWall = true;
                if (rightCheck.Intersects(platform.Hitbox) && !isGrounded) isTouchingRightWall = true;
            }
        }

        if (isGrounded)
            isDoubleJump = false;

        if (wallJumpTimer > 0)
        {
            // ถ้ากำลังอยู่ในช่วง Wall Jump จะบังคับพุ่งด้วยความแรง wallJumpForce
            Velocity.X = forcedDirection * wallJumpForce;
            wallJumpTimer--; // นับเวลาถอยหลัง
        }
        else
        {
            // ถ้าเวลาพุ่งหมดแล้ว กลับมาเดินตามปุ่มกดปกติ
            Velocity.X = 0;
            if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) Velocity.X = -speed;
            if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) Velocity.X = speed;
        }

        bool justPressedSpace = kState.IsKeyDown(Keys.Space) && previousKey.IsKeyUp(Keys.Space);

        if (justPressedSpace)
        {
            if (isGrounded)
                Velocity.Y = jumpForce;
            else if (hasWallJump && isTouchingLeftWall)
            {
                Velocity.Y = jumpForce;
                wallJumpTimer = wallJumpLockTime;
                forcedDirection = 1f;
                Velocity.X = speed;
            }
            else if (hasWallJump && isTouchingRightWall)
            {
                Velocity.Y = jumpForce;
                wallJumpTimer = wallJumpLockTime;
                forcedDirection = -1f;
                Velocity.X = -speed;
            }
            else if (!isGrounded && !isDoubleJump)
            {
                Velocity.Y = jumpForce;
                isDoubleJump = true;
            }
        }

        if (hasWallJump && (isTouchingLeftWall || isTouchingRightWall) && Velocity.Y > 0)
        {
            Velocity.Y = wallSlideWall;
        }
        else
        {
            Velocity.Y += gravity;
        }

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