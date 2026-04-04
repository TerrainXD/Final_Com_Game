using FinalProject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

public enum TimeState
{
    Permanent,
    Present,
    Past
}

public class Platform
{
    public TimeState ActiveTime;
    public int RequiredPlateID = -1;
    private Texture2D texture;
    public Rectangle Hitbox { get; private set; }
    public TimeState PlatformTimeState { get; private set; }

    public bool IsSolid { get; private set; }
    public bool IsVisible { get; private set; }

    public bool IsMoving { get; private set; } = false;
    private float minX;
    private float maxX;
    private float moveSpeed;
    private int moveDirection = 1;
    private float currentX;

    private Rectangle sorceRect;

    private int totalFrames = 1;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private float frameInterval = 0.1f;
    private int frameWidth;
    private int frameHeight;

    public Platform(Rectangle hitbox, TimeState activeTime, Texture2D tex, Rectangle srcRect, int plateID = -1)
    {
        Hitbox = hitbox;
        PlatformTimeState = activeTime;
        texture = tex;
        RequiredPlateID = plateID;
        sorceRect = srcRect;
    }

    public void SetAnimation(int frames, int width, int height, float interval = 0.1f)
    {
        totalFrames = frames;
        frameWidth = width;
        frameHeight = height;
        frameInterval = interval;
    }
    public void SetMoving(float distanceX, float speed)
    {
        IsMoving = true;
        currentX = Hitbox.X;
        moveSpeed = speed;

        if (distanceX > 0)
        {
            minX = currentX;
            maxX = currentX + distanceX;
            moveDirection = 1;
        }
        else
        {
            minX = currentX + distanceX;
            maxX = currentX;
            moveDirection = -1;
        }
    }

    public void Update(TimeState currentTime, List<PressurePlate> plates, GameTime gameTime)
    {
        if (PlatformTimeState == TimeState.Permanent || PlatformTimeState == currentTime)
        {
            IsSolid = true;
            IsVisible = true;   
        }
        else
        {
        
            IsSolid = false;  
            IsVisible = false; 
        }

        if (RequiredPlateID != -1)
        {
            var linkedPlate = plates.Find(p => p.PlateID == RequiredPlateID);

            if (linkedPlate != null && linkedPlate.IsPressed)
            {
                IsSolid = false;
                IsVisible = false;
            }
        }

        if (totalFrames > 1 && IsVisible)
        {
            frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (frameTimer > frameInterval)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                frameTimer = 0f;
                sorceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            }
        }

        if (IsMoving && IsVisible)
        {
            currentX += moveSpeed * moveDirection;

            if (currentX >= maxX)
            {
                currentX = maxX;
                moveDirection = -1;
            }
            else if (currentX <= minX)
            {
                currentX = minX;
                moveDirection = 1;
            }
            
            Hitbox = new Rectangle((int)currentX, Hitbox.Y, Hitbox.Width, Hitbox.Height);
        }


    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsVisible)
        {
            spriteBatch.Draw(texture, Hitbox, sorceRect, Color.White);
        }
        else
        {
            if (RequiredPlateID != -1)
            {
                spriteBatch.Draw(texture, Hitbox, sorceRect, Color.Red * 0.0f);
            }

        }
    }
}
