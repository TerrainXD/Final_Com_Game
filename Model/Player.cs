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
    private float wallJumpForce = 6f;

    private bool isGrounded;

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

    public void Update(List<Platform> platforms, List<Box> boxes, ParticleManager particleManager)
    {
        if (IsDead)
        {
            Velocity = Vector2.Zero;
            return;
        }
        {
            // ==========================================
            // 1. จัดการ Timer (Dash, อมตะ, สตัน)
            // ==========================================
            if (dashCooldownTimer > 0) dashCooldownTimer--;
            if (invincibilityTimer > 0)
            {
                invincibilityTimer--;
                if (invincibilityTimer <= 0) isInvincible = false;
            }
            if (stunTimer > 0) stunTimer--;


            // ==========================================
            // 2. รับ Input แกน X และ Dash
            // ==========================================
            if (InputManager.IsKeyPressed(Keys.LeftControl) && dashCooldownTimer <= 0 && stunTimer <= 0)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                dashDirection = facingRight ? 1f : -1f;
            }

            if (stunTimer <= 0 && !isDashing)
            {
                if (wallJumpTimer > 0)
                {
                    Velocity.X = forcedDirection * wallJumpForce;
                    wallJumpTimer--;
                }
                else
                {
                    Velocity.X = 0;
                    if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left)) Velocity.X = -speed;
                    if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right)) Velocity.X = speed;
                }
            }

            if (isDashing)
            {
                Velocity.X = dashDirection * dashSpeed;
                Velocity.Y = 0;
                dashTimer--;
                if (dashTimer <= 0) isDashing = false;
            }


            // ==========================================
            // 3. เรดาร์เช็คสภาพแวดล้อม (ต้องทำก่อนไปคำนวณแอนิเมชัน)
            // ==========================================
            Rectangle groundCheck = new Rectangle((int)Position.X, (int)Position.Y + 1, Hitbox.Width, Hitbox.Height);
            isGrounded = false;

            // เช็คพื้นก่อนเสมอ
            foreach (var platform in platforms)
                if (platform.IsSolid && groundCheck.Intersects(platform.Hitbox)) isGrounded = true;
            foreach (var box in boxes)
                if (groundCheck.Intersects(box.Hitbox)) isGrounded = true;

            if (isGrounded) isDoubleJump = false;

            Rectangle leftCheck = new Rectangle((int)Position.X - 1, (int)Position.Y, Hitbox.Width, Hitbox.Height);
            isTouchingLeftWall = false;
            Rectangle rightCheck = new Rectangle((int)Position.X + 1, (int)Position.Y, Hitbox.Width, Hitbox.Height);
            isTouchingRightWall = false;

            // ✨ แก้บั๊กสั่น: "จะเกาะกำแพงได้ ต้องลอยอยู่กลางอากาศเท่านั้น" (ไม่เช็คเรดาร์กำแพงถ้าเหยียบพื้นอยู่)
            if (!isGrounded)
            {
                foreach (var platform in platforms)
                {
                    if (platform.IsSolid)
                    {
                        if (leftCheck.Intersects(platform.Hitbox)) isTouchingLeftWall = true;
                        if (rightCheck.Intersects(platform.Hitbox)) isTouchingRightWall = true;
                    }
                }
                foreach (var box in boxes)
                {
                    if (leftCheck.Intersects(box.Hitbox)) isTouchingLeftWall = true;
                    if (rightCheck.Intersects(box.Hitbox)) isTouchingRightWall = true;
                }
            }

            bool isWallSliding = hasWallJump && (isTouchingLeftWall || isTouchingRightWall) && Velocity.Y > 0 && !isDashing;


            // ==========================================
            // 4. ลอจิกการกระโดด
            // ==========================================
            bool justPressedSpace = InputManager.IsKeyPressed(Keys.Space);
            if (justPressedSpace && stunTimer <= 0 && !isDashing)
            {
                if (isGrounded)
                    Velocity.Y = jumpForce;
                else if (hasWallJump && isTouchingLeftWall)
                {
                    Velocity.Y = jumpForce;
                    wallJumpTimer = wallJumpLockTime;
                    forcedDirection = 1f;
                    Velocity.X = wallJumpForce;
                    isWallSliding = false;
                }
                else if (hasWallJump && isTouchingRightWall)
                {
                    Velocity.Y = jumpForce;
                    wallJumpTimer = wallJumpLockTime;
                    forcedDirection = -1f;
                    Velocity.X = -wallJumpForce;
                    isWallSliding = false;
                }
                else if (!isGrounded && !isDoubleJump)
                {
                    Velocity.Y = jumpForce;
                    isDoubleJump = true;
                }
            }


            // ==========================================
            // 5. แรงโน้มถ่วง และ อัปเดตตำแหน่ง + การชน
            // ==========================================
            if (!isDashing)
            {
                if (isWallSliding) Velocity.Y = wallSlideWall;
                else Velocity.Y += gravity;
            }

            Position.X += Velocity.X;
            CheckCollision(platforms, boxes, true);

            Position.Y += Velocity.Y;
            CheckCollision(platforms, boxes, false);

            VisualPosition.X = MathHelper.Lerp(VisualPosition.X, Position.X, 0.2f);
            VisualPosition.Y = MathHelper.Lerp(VisualPosition.Y, Position.Y, 0.2f);


            // ==========================================
            // 6. ระบบแอนิเมชัน (ย้ายมาไว้ล่างสุด เพื่อให้ได้ค่าตำแหน่งล่าสุดเป๊ะๆ)
            // ==========================================
            if (isWallSliding)
            {
                if (isTouchingLeftWall) facingRight = false;
                else if (isTouchingRightWall) facingRight = true;
            }
            else if (!isDashing && Velocity.X != 0)
            {
                if (Velocity.X > 0) facingRight = true;
                else if (Velocity.X < 0) facingRight = false;
            }

            PlayerState newState = PlayerState.Idle;

            if (IsDead) newState = PlayerState.Die;
            else if (stunTimer > 0) newState = PlayerState.Hurt;
            else if (isDashing) newState = PlayerState.Dashing;
            else if (isWallSliding) newState = PlayerState.Jumping; // ค้างท่ากระโดดรูปแรก
            else if (!isGrounded)
            {
                if (isDoubleJump) newState = PlayerState.DoubleJumping;
                else newState = PlayerState.Jumping;
            }
            else if (Velocity.X != 0) newState = PlayerState.Running;
            else newState = PlayerState.Idle;

            if (newState != currentState)
            {
                currentState = newState;
                currentAnimation = animations[currentState];
                currentFrame = 0;
                frameCounter = 0;
            }

            if (isWallSliding)
            {
                currentFrame = 0; // แช่เฟรม
            }
            else
            {
                frameCounter++;
                if (frameCounter >= frameDelay)
                {
                    frameCounter = 0;
                    currentFrame++;

                    if (currentFrame >= currentAnimation.FrameCount)
                    {
                        if (currentAnimation.IsLooping) currentFrame = 0;
                        else currentFrame = currentAnimation.FrameCount - 1;
                    }
                }
            }

            if (isDashing && dashTimer % 2 == 0)
            {
                float Scale = 1.5f;
                int drawWidth = (int)(currentAnimation.FrameWidth * Scale);
                int drawHeight = (int)(currentAnimation.FrameHeight * Scale);
                Rectangle sourceRect = new Rectangle(currentFrame * currentAnimation.FrameWidth, 0, currentAnimation.FrameWidth, currentAnimation.FrameHeight);

                Rectangle drawRect = new Rectangle(
                    (int)VisualPosition.X + (Hitbox.Width / 2) - (drawWidth / 2),
                    (int)VisualPosition.Y + Hitbox.Height - drawHeight,
                    drawWidth,
                    drawHeight);

                SpriteEffects flipEffect = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                particleManager.AddParticle(new Particle(drawRect, sourceRect, flipEffect, currentAnimation.Texture, Color.Cyan, 0.6f, 0.05f));
            }
        }
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