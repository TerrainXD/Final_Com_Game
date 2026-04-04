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

        private int frameWidth = 28;
        private int frameHeight = 28;

        private Rectangle unpressedRect;
        private Rectangle pressedRect;

        private Vector2 position;

        public PressurePlate(Rectangle rect, Texture2D tex, int id)
        {
            PlateID = id;
            texture = tex;
            position = new Vector2(rect.X + 2, rect.Bottom - frameHeight);

            int hitboxHeight = 10;
            Hitbox = new Rectangle((int)position.X, rect.Bottom - hitboxHeight, frameWidth, hitboxHeight);

            unpressedRect = new Rectangle(7 * frameWidth, 0, frameWidth, frameHeight);
            pressedRect = new Rectangle(6 * frameWidth, 0, frameWidth, frameHeight);
        }

        public void Update(Player player, List<Box> boxes)
        {
            bool currentlyOccupied = false;

            // Check if player is standing on it
            if (player.Hitbox.Intersects(Hitbox))
            {
                currentlyOccupied = true;
            }

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
            Rectangle sourceRect = IsPressed ? pressedRect : unpressedRect;
            Rectangle drawRect = new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight);
            spriteBatch.Draw(texture, drawRect, sourceRect, Color.White);
        }
    }
}