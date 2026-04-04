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
return isVisible && currentFrame >= 1 && currentFrame <= 6;
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
            case 0: return 0.0f;  
            case 1: return 0.60f; 
            case 2: return 0.85f; 
            case 3: return 0.95f; 
            case 4: return 0.95f; 
            case 5: return 0.95f; 
            case 6: return 0.40f; 
            case 7: return 0.0f;  
            case 8: return 0.0f;  
            case 9: return 0.0f;  
            case 10: return 0.0f; 
            case 11: return 0.0f; 
            default: return 0.0f; 
        }
    }
  public Rectangle Hitbox
    {
        get
        {
            int maxH = (int)(frameHeight * scaleY);
            
            int currentH = (int)(maxH * GetHeightMultiplier());
            
            int hitboxWidth = 8;
            int hitboxX = (int)Position.X + 12;

            int hitboxY = IsUpsideDown ? (int)Position.Y : (int)Position.Y + 32 - currentH;
            
            return new Rectangle(hitboxX, hitboxY, hitboxWidth, currentH);
        }
    }    public Hazard(Vector2 startPos, TimeState timeState, Texture2D tex)
    {
        Position = startPos;
        HazardTimeState = timeState;
        texture = tex;

        totalFrames = 12;
        frameWidth = 32;
        frameHeight = 32;
        frameTime = 0.15f;

        currentFrame = 0;
        elapsedTime = 0f;
        sourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
    }

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
