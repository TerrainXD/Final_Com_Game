using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FinalProject.Managers
{
    public class ParticleManager
    {
        private List<Particle> particles;

        public ParticleManager()
        {
            particles = new List<Particle>();
        }

        public void AddParticle(Particle particle)
        {
            particles.Add(particle);
        }

        public void Update()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update();
                if (particles[i].Opacity <= 0)
                {
                    particles.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var particle in particles)
            {
                particle.Draw(spriteBatch);
            }
        }

        public void Clear()
        {
            particles.Clear();
        }
    }
}