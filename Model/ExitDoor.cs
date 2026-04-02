
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ExitDoor
{
    public Rectangle Hitbox;
    private Texture2D texture;

    private int currentFrame;
    private int totleFrames = 6;
    private int columns = 3;
    private int rows = 2;

    private float frameTime = 0.1f;
    private float timer = 0f;

    private int frameWidth;
    private int frameHeight;

    public ExitDoor(Rectangle rect, Texture2D tex)
    {
        texture = tex;
        Hitbox = rect;

        frameWidth = texture.Width / columns;
        frameHeight = texture.Height / rows;
    }

    public void Update(GameTime gameTime)
    {
        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (timer > frameTime)
        {
            currentFrame++;
            if (currentFrame >= totleFrames)
            {
                currentFrame = 0;
            }
            timer = 0f;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int col = currentFrame % columns;
        int row = currentFrame / columns;

        Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);

        int drawWidth = 64;
        int drawHeight = 64;
        Rectangle drawRect = new Rectangle(Hitbox.Center.X - (drawWidth / 2), Hitbox.Bottom - drawHeight, drawWidth, drawHeight);
        spriteBatch.Draw(texture, drawRect, sourceRect, Color.White);
    }
}