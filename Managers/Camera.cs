using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProject;

public class Camera
{
    public Matrix Transform { get; private set; }
    private Viewport viewport;

    public Camera(Viewport newViewport)
    {
        viewport = newViewport;
    }

    public void Update(Vector2 targetPosition, int mapWidth, int mapHeight)
    {
        float x = targetPosition.X - (viewport.Width / 2f);
        float y = targetPosition.Y - (viewport.Height / 2f) + 100f;

        float limitRight = mapWidth - viewport.Width;
        float limitBottom = mapHeight - viewport.Height;

        if (limitRight < 0) limitRight = 0;
        if (limitBottom < 0) limitBottom = 0;

        if (x < 0) x = 0;
        if (x > limitRight) x = limitRight;

        if (y < 0) y = 0;
        if (y > limitBottom) y = limitBottom;

        Transform = Matrix.CreateTranslation(new Vector3(-x, -y, 0));
    }
}