using FinalProject.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
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

    // --- Dash Variables ---
    private float dashSpeed = 25f;
    private int dashDuration = 10;
    private int dashCooldown = 40;
    private int dashTimer = 0;
    private int dashCooldownTimer = 0;
    public bool isDashing = false;
    private float dashDirection = 0f;
    private bool isTouchingLeftWall;
    private bool isTouchingRightWall;
    public bool isDoubleJump = false;
    public bool hasWallJump = true;
    public bool IsDead = false;
    private Texture2D texture;

    public Vector2 SpawnPoint;
    public Vector2 VisualPosition;

    //Animation
    public enum PlayerState { Idle, Running, Jumping, DoubleJumping, Hurt, Die, Dashing }
    private PlayerState currentState = PlayerState.Idle;
    private Dictionary<PlayerState, Animation> animations;
    private Animation currentAnimation;

    private int currentFrame = 0;
    private int frameDelay = 6;
    private int frameCounter = 0;
    private bool facingRight = true;
    public Player(Vector2 startPos, Dictionary<PlayerState, Animation> anims)
    {
        Position = startPos;
        SpawnPoint = startPos;
        VisualPosition = startPos;

        animations = anims;
        currentAnimation = animations[PlayerState.Idle];
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

    public void PushOutHorizontally(List<Platform> platforms)
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
    public void Update(List<Platform> platforms, List<Box> boxes)
    {
        PushOutHorizontally(platforms);
        if (dashCooldownTimer > 0) dashCooldownTimer--;
        if (InputManager.IsKeyPressed(Keys.LeftShift) && dashCooldownTimer <= 0 && stunTimer <= 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            dashDirection = facingRight ? 1f : -1f;
            isInvincible = true;
            invincibilityTimer = dashDuration; 
        }
        // --- 1. เช็คทิศทางการหันหน้า ---
        if (Velocity.X > 0) facingRight = true;
        else if (Velocity.X < 0) facingRight = false;

        // --- 2. ตัดสินใจว่าตอนนี้อยู่ State ไหน ---
        PlayerState newState = PlayerState.Idle;

        if (IsDead) newState = PlayerState.Die;
        else if (stunTimer > 0) newState = PlayerState.Hurt;
        else if (isDashing) newState = PlayerState.Dashing;
        else if (!isGrounded)
        {
            if (isDoubleJump) newState = PlayerState.DoubleJumping;
            else newState = PlayerState.Jumping;
        }
        else if (Velocity.X != 0) newState = PlayerState.Running;
        else newState = PlayerState.Idle;

        // --- 3. เปลี่ยนแอนิเมชันถ้าระบบสั่งเปลี่ยน State ---
        if (newState != currentState)
        {
            currentState = newState;
            currentAnimation = animations[currentState];
            currentFrame = 0; // เริ่มเล่นเฟรมแรกใหม่
            frameCounter = 0;
        }

        // --- 4. รันเฟรมแอนิเมชัน ---
        frameCounter++;
        if (frameCounter >= frameDelay)
        {
            frameCounter = 0;
            currentFrame++;

            if (currentFrame >= currentAnimation.FrameCount)
            {
                if (currentAnimation.IsLooping)
                    currentFrame = 0; // วนกลับไปเฟรมแรก
                else
                    currentFrame = currentAnimation.FrameCount - 1; // ค้างที่เฟรมสุดท้าย (ใช้กับท่าตาย)
            }
        }
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
        else if (isDashing)
        {
            Velocity.X = dashDirection * dashSpeed;
            Velocity.Y = 0; // Freeze gravity during dash
            dashTimer--;
            if (dashTimer <= 0) isDashing = false;
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
        // if (hasWallJump && (isTouchingLeftWall || isTouchingRightWall) && Velocity.Y > 0)
        // {
        //     Velocity.Y = wallSlideWall;
        // }
        // else
        // {
        //     Velocity.Y += gravity;
        // }
        if (!isDashing)
        {
            if (hasWallJump && (isTouchingLeftWall || isTouchingRightWall) && Velocity.Y > 0)
            {
                Velocity.Y = wallSlideWall;
            }
            else
            {
                Velocity.Y += gravity;
            }
        }

        if (!isDashing)
                {
                    if (Velocity.X > 0) facingRight = true;
                    else if (Velocity.X < 0) facingRight = false;
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

        float scale = 1.5f;
        int drawWidth = (int)(currentAnimation.FrameWidth * scale);
        int drawHeight = (int)(currentAnimation.FrameHeight * scale);
        //หากรอบตัดภาพ Sprite
        Rectangle sourceRect = new Rectangle(currentFrame * currentAnimation.FrameWidth, 0, currentAnimation.FrameWidth, currentAnimation.FrameHeight);
        // หาตำแหน่งวาด (ปรับ Offset ให้อยู่ตรงกลาง Hitbox นิดหน่อย)
        Rectangle drawRect = new Rectangle(
            (int)VisualPosition.X + (Hitbox.Width / 2) - (drawWidth / 2),
            (int)VisualPosition.Y + Hitbox.Height - drawHeight,
            drawWidth,
            drawHeight);

        // ถ้าหันซ้าย ให้พลิกภาพ
        SpriteEffects flipEffect = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(currentAnimation.Texture, drawRect, sourceRect, drawColor, 0f, Vector2.Zero, flipEffect, 0f);
    }
}