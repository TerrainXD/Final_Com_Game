using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FinalProject
{
    public class SwingingHazard
    {
        public Vector2 Anchor;       
        public Vector2 HeadPosition; 
        public Rectangle Hitbox;
        
        // TimeState Properties
        public TimeState HazardTimeState { get; private set; }
        public bool IsDangerous { get; private set; }
        private bool isVisible;

        private float angle;
        private float speed = 2.0f;    
        private float range = 1.2f;    
        private float length;          
        
        private Texture2D texture; 
        private Texture2D dummyTexture;
        public bool IsFullRotation = false;

        // Updated constructor to include TimeState
        public SwingingHazard(Vector2 anchor, float moveSpeed, float chainLength, Texture2D tex, Texture2D dummy, TimeState timeState)
        {
            Anchor = anchor;
            speed = moveSpeed;
            length = chainLength;
            texture = tex;
            dummyTexture = dummy;
            HazardTimeState = timeState; // Initialize the state
            HeadPosition = new Vector2(Anchor.X, Anchor.Y + length);
        }

        // Updated Update method to include currentTime check
        public void Update(GameTime gameTime, TimeState currentTime)
        {
            if (gameTime == null) return;

            // Logic matching the Spike class behavior
            if (HazardTimeState == TimeState.Permanent || HazardTimeState == currentTime)
            {
                IsDangerous = true;
                isVisible = true;

                // Only calculate movement if the hazard is active/visible
                float totalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;

                if (IsFullRotation)
                {
                    angle = totalSeconds * speed; 
                }
                else
                {
                    angle = (float)Math.Sin(totalSeconds * speed) * range;
                }

                HeadPosition.X = Anchor.X + (float)Math.Sin(angle) * length;
                HeadPosition.Y = Anchor.Y + (float)Math.Cos(angle) * length;

                int size = 40;
                Hitbox = new Rectangle((int)HeadPosition.X - size / 2, (int)HeadPosition.Y - size / 2, size, size);
            }
            else
            {
                IsDangerous = false;
                isVisible = false;
                // Optionally reset Hitbox to empty when not dangerous
                Hitbox = Rectangle.Empty;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Only draw if visible based on TimeState
            if (isVisible)
            {
                // 1. Draw the Chain
                float distance = Vector2.Distance(Anchor, HeadPosition);
                float chainRotation = (float)Math.Atan2(HeadPosition.Y - Anchor.Y, HeadPosition.X - Anchor.X);

                spriteBatch.Draw(dummyTexture, 
                    Anchor, 
                    null, 
                    Color.Gray, 
                    chainRotation - MathHelper.PiOver2, 
                    Vector2.Zero, 
                    new Vector2(2, distance), 
                    SpriteEffects.None, 
                    0f);

                // 2. Draw the Mace Head
                Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
                spriteBatch.Draw(texture, HeadPosition, null, Color.White, angle, origin, 1.0f, SpriteEffects.None, 0f);
            }
        }
    }
}