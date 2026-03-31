using Microsoft.Xna.Framework.Graphics;

public class Animation
{
    public Texture2D Texture { get; private set; }
    public int FrameCount { get; private set; }
    public bool IsLooping { get; private set; }
    public int FrameWidth { get { return Texture.Width / FrameCount; } }
    public int FrameHeight { get { return Texture.Height; } }

    public Animation(Texture2D texture, int frameCount, bool isLooping = true)
    {
        Texture = texture;
        FrameCount = frameCount;
        IsLooping = isLooping;
    }
}