using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace FinalProject
{
    public class Enemy
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        // TimeState Properties
        public TimeState EnemyTimeState { get; private set; }
        public bool IsDangerous { get; private set; }
        private bool isVisible;

        private float speed = 2f;
        private Texture2D texture;
        private bool movingRight = false;

        // Added TimeState to the constructor
        public Enemy(Vector2 startPos, Texture2D tex, TimeState timeState)
        {
            Position = startPos;
            texture = tex;
            Velocity = new Vector2(-speed, 0); // Start moving left
            EnemyTimeState = timeState;
        }

        // Added TimeState currentTime parameter
        public void Update(List<Platform> platforms, List<Box> boxes, TimeState currentTime)
        {
            // 1. Time state logic
            if (EnemyTimeState == TimeState.Permanent || EnemyTimeState == currentTime)
            {
                IsDangerous = true;
                isVisible = true;
            }
            else
            {
                IsDangerous = false;
                isVisible = false;
            }

            // Do not move or check collisions if the enemy is not currently active
            if (!IsDangerous) return;

            // 2. Move horizontally
            Position.X += Velocity.X;

            // 3. Collision and Ledge Detection
            bool groundBelowFront = false;

            // Define a point in front of the enemy, slightly below its feet
            Vector2 frontEdgeCheck = movingRight 
                ? new Vector2(Hitbox.Right + 1, Hitbox.Bottom + 1) 
                : new Vector2(Hitbox.Left - 1, Hitbox.Bottom + 1);

            foreach (var platform in platforms)
            {
                if (!platform.IsSolid) continue;

                // Wall Collision: Turn around if hitting a side
                if (Hitbox.Intersects(platform.Hitbox))
                {
                    HandleCollision(platform.Hitbox);
                }

                // Ledge Detection: Check if this platform is under our "front edge"
                if (platform.Hitbox.Contains(frontEdgeCheck))
                {
                    groundBelowFront = true;
                }
            }

            // 4. If there's no ground in front, turn around to "stick" to the platform
            if (!groundBelowFront)
            {
                ReverseDirection();
            }
        }

        private void HandleCollision(Rectangle obstacle)
        {
            if (movingRight)
            {
                Position.X = obstacle.Left - Hitbox.Width;
            }
            else
            {
                Position.X = obstacle.Right;
            }
            ReverseDirection();
        }

        private void ReverseDirection()
        {
            movingRight = !movingRight;
            Velocity.X = movingRight ? speed : -speed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Only draw if the enemy is in the current time state
            if (isVisible)
            {
                spriteBatch.Draw(texture, Hitbox, Color.Red);
            }
        }
    }
}