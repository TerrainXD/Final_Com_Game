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
        
        private float angle;
        private float speed = 2.0f;    
        private float range = 1.2f;    
        private float length;          
        
        private Texture2D texture; 
        private Texture2D dummyTexture;
        public bool IsFullRotation = false;

        public SwingingHazard(Vector2 anchor, float moveSpeed, float chainLength, Texture2D tex, Texture2D dummy)
        {
            Anchor = anchor;
            speed = moveSpeed;
            length = chainLength;
            texture = tex;
            dummyTexture = dummy;
            HeadPosition = new Vector2(Anchor.X, Anchor.Y + length);
        }

        public void Update(GameTime gameTime)
            {
                if (gameTime == null) return;

                float totalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;

                if (IsFullRotation)
                {
                    // 360 Mode: Angle just keeps spinning
                    angle = totalSeconds * speed; 
                }
                else
                {
                    // Normal Mode: Sine wave for pendulum motion
                    angle = (float)Math.Sin(totalSeconds * speed) * range;
                }

                // Calculate the position of the ball (Math remains the same for both)
                HeadPosition.X = Anchor.X + (float)Math.Sin(angle) * length;
                HeadPosition.Y = Anchor.Y + (float)Math.Cos(angle) * length;

                // Update Hitbox
                int size = 40;
                Hitbox = new Rectangle((int)HeadPosition.X - size / 2, (int)HeadPosition.Y - size / 2, size, size);
            }

        // public void Draw(SpriteBatch spriteBatch)
        //     {
        //         // 1. Draw the Anchor Block FIRST (so it sits behind the chain)
        //         // We center the block on the anchor point
        //         Rectangle anchorDrawRect = new Rectangle((int)Anchor.X - 16, (int)Anchor.Y - 16, 32, 32);
        //         spriteBatch.Draw(maceBlockTexture, anchorDrawRect, anchorSourceRect, Color.White);

        //         // 2. Draw the Chain
        //         float distance = Vector2.Distance(Anchor, HeadPosition);
        //         float chainRotation = (float)Math.Atan2(HeadPosition.Y - Anchor.Y, HeadPosition.X - Anchor.X);

        //         spriteBatch.Draw(dummyTexture, 
        //             Anchor, 
        //             null, 
        //             Color.Gray, 
        //             chainRotation - MathHelper.PiOver2, 
        //             Vector2.Zero, 
        //             new Vector2(2, distance), 
        //             SpriteEffects.None, 
        //             0f);

        //         // 3. Draw the Mace Head
        //         Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
        //         spriteBatch.Draw(texture, HeadPosition, null, Color.White, angle, origin, 1.0f, SpriteEffects.None, 0f);
        //     }
        public void Draw(SpriteBatch spriteBatch)
        {
            // REMOVE OR COMMENT OUT THIS PART:
            // Rectangle anchorDrawRect = new Rectangle((int)Anchor.X - 16, (int)Anchor.Y - 16, 32, 32);
            // spriteBatch.Draw(maceBlockTexture, anchorDrawRect, anchorSourceRect, Color.White);

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