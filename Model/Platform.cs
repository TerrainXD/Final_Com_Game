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
    public Rectangle Hitbox;
    public TimeState ActiveTime;
    public bool IsSolid;
    public int RequiredPlateID = -1;
    private Texture2D texture;

    public Platform(Rectangle hitbox, TimeState activeTime, Texture2D tex, int plateID = -1)
    {
        Hitbox = hitbox;
        ActiveTime = activeTime;
        texture = tex;
        RequiredPlateID = plateID;
    }

    public void Update(TimeState currentTime, List<PressurePlate> plates)
    {
        if (ActiveTime == TimeState.Permanent)
        {
            IsSolid = true;
        }
        else
        {
            IsSolid = (currentTime == ActiveTime);
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
        if (IsSolid)
        {
            spriteBatch.Draw(texture, Hitbox, Color.White); // สีเข้ม เหยียบได้
        }
        else
            {
                if (RequiredPlateID != -1)
                {
                    spriteBatch.Draw(texture, Hitbox, Color.White * 0.3f);
                }
                else
                {
                    spriteBatch.Draw(texture, Hitbox, Color.White * 0.5f); // สีจาง เหยียบไม่ได้
                }
            }
    }
}