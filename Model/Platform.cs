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
    public bool IsVisible { get; private set; } // ✨ เพิ่มตัวแปรเช็คการมองเห็น

    private Rectangle sorceRect;

    public Platform(Rectangle hitbox, TimeState activeTime, Texture2D tex, Rectangle srcRect, int plateID = -1)
    {
        Hitbox = hitbox;
        PlatformTimeState = activeTime;
        texture = tex;
        RequiredPlateID = plateID;
        sorceRect = srcRect;
    }

    public void Update(TimeState currentTime, List<PressurePlate> plates)
    {
        // ✨ ถ้าเป็นบล็อกถาวร หรือเป็นบล็อกตรงกับมิติเวลาปัจจุบัน
        if (PlatformTimeState == TimeState.Permanent || PlatformTimeState == currentTime)
        {
            IsSolid = true;     // ชนได้ เหยียบได้
            IsVisible = true;   // มองเห็นได้
        }
        else
        {
            // ✨ ถ้าอยู่คนละมิติเวลา
            IsSolid = false;    // ทะลุผ่านเลย
            IsVisible = false;  // หายวับไปจากหน้าจอ!
        }

        // 2. Check Pressure Plate Logic (ID System)
        if (RequiredPlateID != -1)
        {
            // Find the plate that matches this platform's ID
            var linkedPlate = plates.Find(p => p.PlateID == RequiredPlateID);

            // If the plate exists and is NOT pressed, the platform stays non-solid
            if (linkedPlate != null && !linkedPlate.IsPressed)
            {
                IsSolid = false;
            }
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
                spriteBatch.Draw(texture, Hitbox, sorceRect, Color.White * 0.3f);
            }

        }
    }
}
