using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Hazard
{
    public Vector2 Position;
    public TimeState HazardTimeState { get; private set; }
    public bool IsDangerous
    {
        get
        {
            return isVisible && currentFrame >= 3 && currentFrame <= 10;
        }
    }
    private bool isVisible;
    private Texture2D texture;
    public bool IsUpsideDown { get; set; } = false;

    // --- ตั้งค่า Animation ---
    private int currentFrame;
    private int totalFrames;
    private int frameWidth;
    private int frameHeight;
    private float frameTime; // เวลาต่อเฟรม (วินาที)
    private float elapsedTime;
    private Rectangle sourceRect;
    private float scaleX = 1.0f;
    private float scaleY = 4.5f;

    private float GetHeightMultiplier()
    {
        switch (currentFrame)
        {
            case 0: case 1: case 2: return 0.1f; // เพิ่งเริ่ม/อยู่ใต้ดิน (สูง 10%)
            case 3: return 0.3f;                 // เริ่มโผล่ (สูง 30%)
            case 4: return 0.5f;                 // โผล่ครึ่งนึง (สูง 50%)
            case 5: return 0.75f;                // เกือบสุด (สูง 75%)
            case 6: case 7: case 8: return 0.95f;// พุ่งสุดความสูง (สูง 95%)
            case 9: return 0.7f;                 // เริ่มหดกลับ (สูง 70%)
            case 10: return 0.4f;                // หดลงครึ่งนึง (สูง 40%)
            case 11: return 0.1f;                // กำลังจะมิดดิน (สูง 10%)
            default: return 1.0f;
        }
    }
    public Rectangle Hitbox
    {
        get
        {
            int maxH = (int)(frameHeight * scaleY);

            int currentH = (int)(maxH * GetHeightMultiplier());

            int w = (int)(frameWidth * scaleX);

            int y = IsUpsideDown ? (int)Position.Y : (int)Position.Y + 32 - currentH;

            return new Rectangle((int)Position.X + 8, y, w - 16, currentH);
        }
    }
    public Hazard(Vector2 startPos, TimeState timeState, Texture2D tex)
    {
        Position = startPos;
        HazardTimeState = timeState;
        texture = tex;

        totalFrames = 12;
        frameWidth = 32;
        frameHeight = 32;
        frameTime = 0.1f;

        currentFrame = 0;
        elapsedTime = 0f;
        sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
    }

    // ✨ ✨ ✨ แก้ไข Update ให้รับค่า GameTime เพื่อใช้คำนวณเวลาแอนิเมชัน ✨ ✨ ✨
    public void Update(TimeState currentTime, GameTime gameTime)
    {
        if (HazardTimeState == TimeState.Permanent || HazardTimeState == currentTime)
        {
            isVisible = true;
        }
        else
        {
            isVisible = false;
        }

        // รันแอนิเมชันเฉพาะตอนที่มองเห็น
        if (isVisible)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedTime > frameTime)
            {
                currentFrame++;
                if (currentFrame >= totalFrames)
                {
                    currentFrame = 0;
                }
                elapsedTime = 0;

                sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (isVisible)
        {
            int drawWidth = (int)(frameWidth * scaleX);
            int drawHeight = (int)(frameHeight * scaleY);

            int drawY;
            if (IsUpsideDown)
            {
                drawY = (int)Position.Y;
            }
            else
            {
                drawY = (int)Position.Y + 32 - drawHeight;
            }

            int drawX = (int)Position.X - (drawWidth - 32) / 2;

            Rectangle drawRect = new Rectangle(drawX, drawY, drawWidth, drawHeight);

            SpriteEffects flipEffect = IsUpsideDown ? SpriteEffects.FlipVertically : SpriteEffects.None;

            spriteBatch.Draw(texture, drawRect, sourceRect, Color.White, 0f, Vector2.Zero, flipEffect, 0f);
        }
    }
}
