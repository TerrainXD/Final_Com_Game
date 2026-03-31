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
    public int MaxHP { get; } = 3;
    public bool isInvincible = false;
    private int invincibilityTimer = 0;
    private int stunTimer = 0;
    private float gravity = 0.5f;
    private float jumpForce = -12f;
    private bool isGrounded;

    private float wallJumpForce = 5f;  // ความแรงในการพุ่งออกข้าง (ปรับให้เยอะกว่า speed ปกติได้)
    private int wallJumpTimer = 0;     // ตัวนับเวลาพุ่ง
    private float wallSlideWall = 2f;
    private int wallJumpLockTime = 12; // ระยะเวลาล็อคปุ่ม (12 เฟรม = ประมาณ 0.2 วินาที)
    private float forcedDirection = 0f;// ทิศทางที่ถูกบังคับพุ่ง (ซ้ายหรือขวา)
    private bool isTouchingLeftWall;
    private bool isTouchingRightWall;
    public bool isDoubleJump = false;
    public bool hasWallJump = true;
    public bool IsDead = false;
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

    public void Heal(int amount)
    {
        HP += amount;
        if (HP > MaxHP) HP = MaxHP;
    }

    public void TakeDamage(int knockDuration)
    {
        if (isInvincible) return;

        HP--;

        if (HP <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = 90;
            stunTimer = 15;

            Velocity.Y = -6f;
            Velocity.X = knockDuration * 6f;
        }

    }

    public void Die()
    {
        IsDead = true;
    }

    public void Update(List<Platform> platforms, List<Box> boxes)
    {
        // 1. เช็คระยะเวลาอมตะ
        if (invincibilityTimer > 0)
        {
            invincibilityTimer--;
            if (invincibilityTimer <= 0) isInvincible = false;
        }
        // 2. ลอจิกการเคลื่อนที่แกน X (จัดลำดับความสำคัญ: Stun -> WallJump -> เดินปกติ)
        if (stunTimer > 0)
        {
            stunTimer--;
            // ตอนติด Stun กระเด็น ห้ามเปลี่ยนค่า Velocity.X ปล่อยให้มันลอยไปตามแรงกระแทก
        }
        else if (wallJumpTimer > 0)
        {
            Velocity.X = forcedDirection * wallJumpForce;
            wallJumpTimer--; // ล็อคปุ่มเดินตามระยะเวลา Wall Jump
        }
        else
        {
            // เดินปกติ
            Velocity.X = 0;
            if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left)) Velocity.X = -speed;
            if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right)) Velocity.X = speed;
        }
        // 3. เรดาร์เช็คสภาพแวดล้อม
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

        // 4. ลอจิกการกระโดด
        bool justPressedSpace = InputManager.IsKeyPressed(Keys.Space); // ใช้ IsKeyPressed ตัวเดียวพอครับ

        // ถ้าติด Stun อยู่ จะกดกระโดดไม่ได้
        if (justPressedSpace && stunTimer <= 0)
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

        // 5. แรงโน้มถ่วง และ การไถลกำแพง
        if (hasWallJump && (isTouchingLeftWall || isTouchingRightWall) && Velocity.Y > 0)
        {
            Velocity.Y = wallSlideWall;
        }
        else
        {
            Velocity.Y += gravity;
        }

        // 6. เช็คการชน
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
                int pushDir = (Position.X < spike.Hitbox.X) ? -1 : 1;
                TakeDamage(pushDir);
                break;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color drawColor = Color.White;
        if (isInvincible && (invincibilityTimer / 10) % 2 == 0)
        {
            drawColor = Color.Red * 0.5f;
        }
        Rectangle drawRect = new Rectangle((int)VisualPosition.X, (int)VisualPosition.Y, 32, 32);
        spriteBatch.Draw(texture, drawRect, drawColor);
    }
}