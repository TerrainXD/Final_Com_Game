using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FinalProject
{
    public class Enemy
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public TimeState EnemyTimeState { get; private set; }
        public bool IsDangerous { get; private set; }
        public bool isVisible { get; private set; }

        private float speed = 1.2f;
        private float gravity = 0.5f;
        private bool isGrounded = false;
        private Texture2D texture;
        private bool movingRight = false;

        // --- ตั้งค่า Animation ---
        private int currentFrame;
        private int totalFrames = 2;
        private int frameWidth;
        private int frameHeight;
        private float frameTimer;
        private float frameInterval = 0.2f;
        private Rectangle sourceRect;
        
        private int drawWidth;
        private int drawHeight;

        // ==========================================
        // ✨ ปรับขนาดสไลม์ (Scale)
        // ==========================================
        private float scale = 2.5f; // ปรับเลขนี้ให้ใหญ่ขึ้นตามต้องการ

        private int offsetX = 10;
        private int offsetY => drawHeight / 2; 
        private int emptyBottomSpace => (int)(2 * scale); 

        public Rectangle Hitbox => new Rectangle(
            (int)Position.X + offsetX, 
            (int)Position.Y + offsetY, 
            drawWidth - (offsetX * 2), 
            (drawHeight / 2) - emptyBottomSpace
        );

        public Enemy(Vector2 startPos, Texture2D tex, TimeState timeState)
        {
            texture = tex;
            Velocity = new Vector2(-speed, 0);
            EnemyTimeState = timeState;

            // ✨ คำนวณขนาดเฟรมและขนาดที่จะวาด
            frameWidth = texture.Width / 4;
            frameHeight = texture.Height / 3;
            drawWidth = (int)(frameWidth * scale);
            drawHeight = (int)(frameHeight * scale);

            // ปรับตำแหน่งเกิดให้ไม่จมดิน
            Position = new Vector2(startPos.X, startPos.Y - (drawHeight - 32));
            sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
        }

        public void Update(GameTime gameTime, List<Platform> platforms, List<Box> boxes, TimeState currentTime)
        {
            // ตรวจสอบมิติเวลา
            if (EnemyTimeState == TimeState.Permanent || EnemyTimeState == currentTime)
            {
                IsDangerous = true;
                isVisible = true;
            }
            else
            {
                IsDangerous = false;
                isVisible = false;
            }

            if (!IsDangerous) return;

            // รันแอนิเมชัน
            frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (frameTimer > frameInterval)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                frameTimer = 0f;
                sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            }

            // แรงโน้มถ่วง
            Velocity.Y += gravity;
            Position.Y += Velocity.Y;
            isGrounded = false;

            // เช็คชนพื้น (แกน Y)
            foreach (var platform in platforms)
            {
                if (!platform.IsSolid) continue;
                if (Hitbox.Intersects(platform.Hitbox))
                {
                    if (Velocity.Y > 0 && Hitbox.Bottom - Velocity.Y <= platform.Hitbox.Top)
                    {
                        Position.Y = platform.Hitbox.Top - Hitbox.Height - offsetY;
                        Velocity.Y = 0;
                        isGrounded = true;
                    }
                }
            }

            // เดินซ้าย-ขวา (แกน X)
            Position.X += Velocity.X;
            bool groundBelowFront = false;
            float sensorX = movingRight ? Hitbox.Right + 5 : Hitbox.Left - 5;
            Vector2 sensorPoint = new Vector2(sensorX, Hitbox.Bottom + 5);

            // เช็คชนกำแพง/ขอบเหว
            foreach (var platform in platforms)
            {
                if (!platform.IsSolid) continue;
                if (Hitbox.Intersects(platform.Hitbox))
                {
                    if (movingRight) Position.X -= (Hitbox.Right - platform.Hitbox.Left);
                    else Position.X += (platform.Hitbox.Right - Hitbox.Left);
                    ReverseDirection();
                }
                if (platform.Hitbox.Contains(sensorPoint)) groundBelowFront = true;
            }

            // ✨ เช็คเดินชนกล่อง (Boxes)
            foreach (var box in boxes)
            {
                if (Hitbox.Intersects(box.Hitbox))
                {
                    if (movingRight) Position.X -= (Hitbox.Right - box.Hitbox.Left);
                    else Position.X += (box.Hitbox.Right - Hitbox.Left);
                    ReverseDirection();
                }
            }

            if (isGrounded && !groundBelowFront) ReverseDirection();
        }

        private void ReverseDirection()
        {
            movingRight = !movingRight;
            Velocity.X = movingRight ? speed : -speed;
        }

        public void Draw(SpriteBatch spriteBatch, TimeState currentTime)
        {
            if (isVisible)
            {
                Rectangle drawRect = new Rectangle((int)Position.X, (int)Position.Y, drawWidth, drawHeight);
                SpriteEffects flip = movingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                
                // ✨ ✨ ✨ เงื่อนไขเปลี่ยนสี: ถ้าเป็น Past ให้เป็นสีส้ม ✨ ✨ ✨
                Color slimeColor = (currentTime == TimeState.Past) ? Color.Orange : Color.White;

                spriteBatch.Draw(texture, drawRect, sourceRect, slimeColor, 0f, Vector2.Zero, flip, 0f);
            }
        }
    }
}