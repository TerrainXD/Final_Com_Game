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
        private float phaseOffset = 0f;
        private static System.Random rand = new System.Random();

        public SwingingHazard(Vector2 anchor, float moveSpeed, float chainLength, Texture2D tex, Texture2D dummy, TimeState timeState)
        {
            Anchor = anchor;
            speed = moveSpeed;
            length = chainLength;
            texture = tex;
            dummyTexture = dummy;
            HazardTimeState = timeState;
            HeadPosition = new Vector2(Anchor.X, Anchor.Y + length);
            phaseOffset = (float)(rand.NextDouble() * Math.PI * 2);
        }

        public void Update(GameTime gameTime, TimeState currentTime)
        {
            if (gameTime == null) return;

            if (HazardTimeState == TimeState.Permanent || HazardTimeState == currentTime)
            {
                IsDangerous = true;
                isVisible = true;

                float totalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;

                if (IsFullRotation)
                {
                    angle = totalSeconds * speed + phaseOffset;
                }
                else
                {
                    angle = (float)Math.Sin((totalSeconds * speed + phaseOffset)) * range;
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
                Hitbox = Rectangle.Empty;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isVisible)
            {
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

                Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
                spriteBatch.Draw(texture, HeadPosition, null, Color.White, angle, origin, 1.0f, SpriteEffects.None, 0f);
            }
        }
    }
}