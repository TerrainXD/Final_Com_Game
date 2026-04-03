using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProject
{
    public class SawHazard
    {
        public Vector2 Position;
        public TimeState HazardTimeState { get; private set; }
        public bool IsDangerous { get; private set; }
        public bool IsVisible { get; private set; }

        private Texture2D texture;
        private Texture2D chainTexture;

        private int currentFrame = 0;
        private int totalFrames = 8;
        private float frameTimer = 0f;
        private float frameInterval = 0.03f;
        private int frameWidth = 38;
        private int frameHeight = 38;
        private float scale = 2.0f;
        private int offsetX = 6;
        private int offsetY = 6;
        private bool isMoving;
        private Vector2 startPos;
        private Vector2 endPos;
        private float speed;
        private float moveProgress = 0f; 
        private int moveDir = 1;

        public Rectangle Hitbox => new Rectangle(
                  (int)Position.X + (int)(offsetX * scale),
                  (int)Position.Y + (int)(offsetY * scale),
                  (int)((frameWidth - (offsetX * 2)) * scale),
                  (int)((frameHeight - (offsetY * 2)) * scale)
        );


        public SawHazard(Vector2 startPosition, TimeState timeState, Texture2D tex, Texture2D chainTex)
        {
            Position = startPosition;
            startPos = startPosition;
            HazardTimeState = timeState;
            texture = tex;
            chainTexture = chainTex;
            isMoving = false;
        }


        public void SetMoving(Vector2 targetPosition, float moveSpeed)
        {
            isMoving = true;
            endPos = targetPosition;
            speed = moveSpeed;
        }

        public void Update(TimeState currentTime, GameTime gameTime)
        {
            if (HazardTimeState == TimeState.Permanent || HazardTimeState == currentTime)
            {
                IsDangerous = true;
                IsVisible = true;
            }
            else
            {
                IsDangerous = false;
                IsVisible = false;
            }

            if (!IsVisible) return;

            frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (frameTimer > frameInterval)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                frameTimer = 0f;
            }

            if (isMoving)
            {
                float distance = Vector2.Distance(startPos, endPos);
                float step = (speed * 60f * (float)gameTime.ElapsedGameTime.TotalSeconds) / distance;

                moveProgress += step * moveDir;

                if (moveProgress >= 1f)
                {
                    moveProgress = 1f;
                    moveDir = -1; 
                }
                else if (moveProgress <= 0f)
                {
                    moveProgress = 0f;
                    moveDir = 1; 
                }

                Position = Vector2.Lerp(startPos, endPos, moveProgress);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            if (isMoving && chainTexture != null)
            {
                float distance = Vector2.Distance(startPos, endPos);
                Vector2 direction = endPos - startPos;
                if (direction != Vector2.Zero) direction.Normalize();

                Vector2 centerOffset = new Vector2((frameWidth * scale) / 2f, (frameHeight * scale) / 2f);
                Vector2 chainCenterOffset = new Vector2(chainTexture.Width / 2f, chainTexture.Height / 2f);

                for (float i = 0; i <= distance; i += 16f)
                {
                    Vector2 chainPos = startPos + (direction * i) + centerOffset - chainCenterOffset;
                    spriteBatch.Draw(chainTexture, chainPos, Color.White);
                }
            }

            Rectangle sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);

            Color tint = (HazardTimeState == TimeState.Past) ? Color.Red : Color.White;
            spriteBatch.Draw(texture, Position, sourceRect, tint, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}