using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using FinalProject.Managers;

namespace FinalProject.Managers
{
    public class VFXManager
    {
        private ParticleManager particleManager;
        private Texture2D particleTexture;
        private Random random;

        public VFXManager(ParticleManager pManager, Texture2D tex)
        {
            particleManager = pManager;
            particleTexture = tex;
            random = new Random();
        }

        public void CreateItemPickupEffect(Rectangle itemHitbox, Color effectColor)
        {
            int particleCount = 15;
            Vector2 center = new Vector2(itemHitbox.Center.X, itemHitbox.Center.Y);

            for (int i = 0; i < particleCount; i++)
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float speed = (float)(random.NextDouble() * 3f + 1f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);

                int size = random.Next(4, 9);
                float fadeRate = (float)(random.NextDouble() * 0.05f + 0.03f);

                Rectangle drawRect = new Rectangle((int)center.X, (int)center.Y, size, size);
                Rectangle sourceRect = new Rectangle(0, 0, particleTexture.Width, particleTexture.Height);

                Particle p = new Particle(
                    drawRect,
                    sourceRect,
                    SpriteEffects.None,
                    particleTexture,
                    effectColor,
                    1.0f,
                    fadeRate,
                    velocity
                );

                particleManager.AddParticle(p);
            }
        }
    }
}