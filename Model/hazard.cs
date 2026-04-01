using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FinalProject
{
    public class Hazard
    {
        public Vector2 BasePosition;
        public Vector2 CurrentPosition;
        public Rectangle Hitbox;
        public bool IsDangerous;
        public bool IsUpsideDown = false;

        private float timer = 0f;
        private float cycleTime = 1.5f; 
        private bool isExtended = false;
        private float extendDistance = 180f; // Height of the "worm"
        
        private Texture2D texture;
        private TimeState activeTime;

        public Hazard(Vector2 basePos, TimeState time, Texture2D tex)
        {
            BasePosition = basePos;
            CurrentPosition = basePos;
            activeTime = time;
            texture = tex;
            // Initialize with a default size
            Hitbox = new Rectangle((int)basePos.X, (int)basePos.Y, 64, 64);
        }

        public void Update(GameTime gameTime, TimeState currentTime)
        {
            // Safety check to prevent NullReferenceException
            if (gameTime == null) return;

            // 1. Sync with TimeState (logic from Spike.cs)
            if (activeTime == TimeState.Permanent || currentTime == activeTime)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // 2. Toggle extended/retracted state based on timer
            if (timer >= cycleTime)
            {
                isExtended = !isExtended;
                timer = 0;
            }

            // 3. Calculate movement based on orientation
            float targetY;
            if (IsUpsideDown)
            {
                // Extend DOWN from the ceiling
                targetY = isExtended ? BasePosition.Y + extendDistance : BasePosition.Y;
            }
            else
            {
                // Extend UP from the floor
                targetY = isExtended ? BasePosition.Y - extendDistance : BasePosition.Y;
            }

            // Smoothly slide to the target position
            CurrentPosition.Y = MathHelper.Lerp(CurrentPosition.Y, targetY, 0.15f);

            // 4. Update Hitbox
            if (IsUpsideDown)
            {
                // Hitbox grows downwards; Top stays at BasePosition.Y
                int height = (int)(CurrentPosition.Y - BasePosition.Y) + 64;
                Hitbox = new Rectangle((int)CurrentPosition.X, (int)BasePosition.Y, 64, height);
            }
            else
            {
                // Hitbox grows upwards; Top moves with CurrentPosition.Y
                int height = (int)(BasePosition.Y - CurrentPosition.Y) + 64;
                Hitbox = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, 64, height);
            }

            // 5. Set Danger Status (Dangerous if moved away from base)
            IsDangerous = Math.Abs(BasePosition.Y - CurrentPosition.Y) > 5;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Follows your Spike.cs visual style for active/inactive
            Color drawColor = IsDangerous ? Color.Red : Color.Red * 0.5f;
            spriteBatch.Draw(texture, Hitbox, drawColor);
        }
    }
}