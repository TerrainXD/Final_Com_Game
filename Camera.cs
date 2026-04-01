using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProject;

public class Camera
{
    public Matrix Transform { get; private set; }

    public void Follow(Player target, GraphicsDevice graphicsDevice)
    {
        Transform = Matrix.Identity;
    }
}