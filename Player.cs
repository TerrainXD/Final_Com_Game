using FinalProject.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

public class Player
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

    private float speed = 5f;
    public int HP { get; set; } = 3;
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
        if (HP == 0)
        {
            Position = SpawnPoint;
            VisualPosition = SpawnPoint;
            Velocity = Vector2.Zero;
        }
        else
        {
            HP--;
        }
    }

    public void Update(List<Platform> platforms, List<Box> boxes)
    {
        Velocity.X = 0;
        if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left)) Velocity.X = -speed;
        if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right)) Velocity.X = speed;

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

        foreach (var box in boxes)
        {
            if (groundCheck.Intersects(box.Hitbox)) isGrounded = true;
            if (leftCheck.Intersects(box.Hitbox) && !isGrounded) isTouchingLeftWall = true;
            if (rightCheck.Intersects(box.Hitbox) && !isGrounded) isTouchingRightWall = true;
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
            if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left)) Velocity.X = -speed;
            if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right)) Velocity.X = speed;
        }

        bool justPressedSpace = InputManager.IsKeyDown(Keys.Space) && InputManager.IsKeyPressed(Keys.Space);

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
        CheckCollision(platforms, boxes, true);

        Position.Y += Velocity.Y;
        CheckCollision(platforms, boxes, false);

        VisualPosition.X = MathHelper.Lerp(VisualPosition.X, Position.X, 0.2f);
        VisualPosition.Y = MathHelper.Lerp(VisualPosition.Y, Position.Y, 0.2f);

    }

    private void CheckCollision(List<Platform> platforms, List<Box> boxes, bool isHorizontal)
    {
        // 1. เช็คการชนกับพื้น/กำแพง (Platform)
        foreach (var platform in platforms)
        {
            if (platform.IsSolid && Hitbox.Intersects(platform.Hitbox))
            {
                ResolveCollision(platform.Hitbox, isHorizontal);
            }
        }

        // 2. เช็คการชนกับกล่อง (Box) **ต้องแยกออกมาเป็นอีกลูปนึง**
        foreach (var box in boxes)
        {
            if (Hitbox.Intersects(box.Hitbox))
            {
                if (isHorizontal)
                {
                    // ดันกล่องไปตามความเร็วของตัวละคร
                    box.TryPush(Velocity.X, platforms);

                    // หยุดตัวละครไม่ให้เดินทะลุกล่อง
                    ResolveCollision(box.Hitbox, true);
                }
                else
                {
                    // ถ้าหล่นลงมาทับกล่อง (แกน Y) ให้เหยียบกล่องได้เหมือนเป็นพื้นปกติ
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
                isDoubleJump = false;
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