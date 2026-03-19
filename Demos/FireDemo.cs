using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameParticleEngine.Core;

namespace MonoGameParticleEngine.Demos
{
    /// <summary>
    /// FireDemo — simulates a campfire using two layered emitters:
    ///   1. Flame core  : hot white-orange cone, fast short-lived particles
    ///   2. Ember drift : dark-orange slow particles that drift upward and sideways
    ///
    /// Technique highlights:
    ///   - Additive blending makes overlapping particles brighter (glow effect)
    ///   - Upward GlobalForce simulates heat convection
    ///   - Two separate ParticlePools so flames and embers never compete for slots
    ///   - Mouse dragging moves the fire source in real-time
    /// </summary>
    public class FireDemo : IDemo
    {
        private ParticlePool    _flamePool;
        private ParticlePool    _emberPool;
        private ParticleEmitter _flameEmitter;
        private ParticleEmitter _emberEmitter;
        private Texture2D       _particleTex;

        public string Name => "Fire";

        public void Load(GraphicsDevice graphicsDevice)
        {
            // Generate a soft circular glow texture procedurally (no content pipeline needed)
            _particleTex = TextureHelper.CreateCircleTexture(graphicsDevice, 32);

            // --- Flame core ---
            _flamePool = new ParticlePool(3000);
            _flameEmitter = new ParticleEmitter(_flamePool, new EmitterConfig
            {
                EmissionRate  = 250f,
                Shape         = EmitterShape.Cone,
                ConeAngle     = MathHelper.ToRadians(20f),
                SpeedMin      = 60f,
                SpeedMax      = 130f,
                LifetimeMin   = 0.4f,
                LifetimeMax   = 0.9f,
                StartColor    = new Color(255, 200, 80),   // bright yellow-orange
                EndColor      = new Color(180, 30,   0, 0),// fade to dark red transparent
                StartScale    = 1.2f,
                EndScale      = 0.1f,
                RotationSpeedMin = -1f,
                RotationSpeedMax =  1f,
                GlobalForce   = new Vector2(0f, -90f)     // upward heat
            });

            // --- Ember drift ---
            _emberPool = new ParticlePool(1000);
            _emberEmitter = new ParticleEmitter(_emberPool, new EmitterConfig
            {
                EmissionRate  = 30f,
                Shape         = EmitterShape.Cone,
                ConeAngle     = MathHelper.ToRadians(35f),
                SpeedMin      = 20f,
                SpeedMax      = 60f,
                LifetimeMin   = 1.5f,
                LifetimeMax   = 3.0f,
                StartColor    = new Color(255, 120, 20, 200),
                EndColor      = new Color(80, 10, 0, 0),
                StartScale    = 0.4f,
                EndScale      = 0.05f,
                RotationSpeedMin = -3f,
                RotationSpeedMax =  3f,
                GlobalForce   = new Vector2(12f, -60f)    // slight sideways drift
            });
        }

        public void SetPosition(Vector2 position)
        {
            _flameEmitter.Position = position;
            _emberEmitter.Position = position;
        }

        public void Update(GameTime gameTime)
        {
            _flameEmitter.Update(gameTime);
            _emberEmitter.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _flameEmitter.Draw(spriteBatch, _particleTex);
            _emberEmitter.Draw(spriteBatch, _particleTex);
        }

        public void Unload()
        {
            _flamePool.Clear();
            _emberPool.Clear();
            _particleTex?.Dispose();
        }
    }
}
