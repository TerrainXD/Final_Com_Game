using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FinalProject
{
    public class PressurePlate
    {
        public int PlateID;
        public Rectangle Hitbox;
        public bool IsPressed { get; private set; }
        private Texture2D texture;
        private Color activeColor = Color.LimeGreen;
        private Color inactiveColor = Color.DarkRed;

        public PressurePlate(Rectangle rect, Texture2D tex, int id)
            {
                PlateID = id;
                Hitbox = new Rectangle(rect.X, rect.Y + 48, rect.Width, 16); 
                texture = tex;
            }

        public void Update(Player player, List<Box> boxes)
        {
            bool currentlyOccupied = false;

            // Check if player is standing on it
            if (player.Hitbox.Intersects(Hitbox))
            {
                currentlyOccupied = true;
            }

            // Check if any box is sitting on it
            foreach (var box in boxes)
            {
                if (box.Hitbox.Intersects(Hitbox))
                {
                    currentlyOccupied = true;
                    break;
                }
            }

            IsPressed = currentlyOccupied;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Hitbox, IsPressed ? activeColor : inactiveColor);
        }
    }
}