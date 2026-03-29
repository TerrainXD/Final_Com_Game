using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProject; // ให้ตรงกับชื่อ namespace ของคุณครับ

public class Camera
{
    public Matrix Transform { get; private set; }

    public void Follow(Player target, GraphicsDevice graphicsDevice)
    {
        // คำนวณให้กล้องขยับไปที่ตำแหน่งตัวละคร (จัดให้อยู่กึ่งกลางหน้าจอ)
        var position = Matrix.CreateTranslation(
            -target.Position.X - (target.Hitbox.Width / 2),
            -target.Position.Y - (target.Hitbox.Height / 2),
            0);

        // คำนวณระยะออฟเซ็ตของหน้าจอ
        var offset = Matrix.CreateTranslation(
            graphicsDevice.Viewport.Width / 2,
            graphicsDevice.Viewport.Height / 2,
            0);

        // รวมค่าเข้าด้วยกันเพื่อส่งให้ SpriteBatch
        Transform = position * offset;
    }
}