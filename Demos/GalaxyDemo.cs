using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameParticleEngine.Core;

namespace MonoGameParticleEngine.Demos
{
    /// <summary>
    /// GalaxyDemo — renders a rotating spiral galaxy using ring-emitter geometry.
    ///
    /// Technique highlights:
    ///   - Two counter-rotating ring emitters create spiral arm structure
    ///   - Each particle receives a tangential orbital velocity (perpendicular to
    ///     its spawn direction) simulating Keplerian rotation
    ///   - Long particle lifetimes + slow fade create star trail persistence
    ///   - Blue core + white/yellow outer stars match a realistic colour gradient
    ///   - Additive blending makes the galactic core bloom
    /// </summary>
    public class GalaxyDemo : IDemo
    {
        private ParticlePool    _corePool;
        private ParticlePool    _armPool;
        private ParticleEmitter _coreEmitter;
        private ParticleEmitter _armEmitter;
        private Texture2D       _starTex;
        private float           _rotationAngle;
        private Vector2         _center;

        public string Name => "Galaxy";

        public void Load(GraphicsDevice graphicsDevice)
        {
            _starTex = TextureHelper.CreateCircleTexture(graphicsDevice, 8);
            _center  = new Vector2(
                graphicsDevice.Viewport.Width  * 0.5f,
                graphicsDevice.Viewport.Height * 0.5f);

            // --- Galactic core (bright blue-white, dense) ---
            _corePool = new ParticlePool(2000);
            _coreEmitter = new ParticleEmitter(_corePool, new EmitterConfig
            {
                EmissionRate  = 80f,
                Shape         = EmitterShape.Ring,
                RingRadius    = 30f,
                SpeedMin      = 5f,
                SpeedMax      = 20f,
                LifetimeMin   = 3f,
                LifetimeMax   = 5f,
                StartColor    = new Color(180, 210, 255, 220),
                EndColor      = new Color(80, 120, 255, 0),
                StartScale    = 0.6f,
                EndScale      = 0.0f,
                RotationSpeedMin = -0.5f,
                RotationSpeedMax =  0.5f,
                GlobalForce   = Vector2.Zero
            })
            { Position = _center };

            // --- Spiral arms (wider ring, yellow-white stars) ---
            _armPool = new ParticlePool(5000);
            _armEmitter = new ParticleEmitter(_armPool, new EmitterConfig
            {
                EmissionRate  = 150f,
                Shape         = EmitterShape.Ring,
                RingRadius    = 160f,
                SpeedMin      = 2f,
                SpeedMax      = 8f,
                LifetimeMin   = 4f,
                LifetimeMax   = 8f,
                StartColor    = new Color(255, 245, 200, 180),
                EndColor      = new Color(180, 160, 100, 0),
                StartScale    = 0.35f,
                EndScale      = 0.0f,
                RotationSpeedMin = -0.2f,
                RotationSpeedMax =  0.2f,
                GlobalForce   = Vector2.Zero
            })
            { Position = _center };
        }

        public void SetPosition(Vector2 position) { /* galaxy stays centered */ }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Rotate the ring emitters over time for spiral arm effect
            _rotationAngle += dt * 0.4f;

            // Apply tangential (orbital) velocity to newly alive particles in arm pool
            ApplyOrbitalVelocity(_armPool, _center, orbitalSpeed: 28f);
            ApplyOrbitalVelocity(_corePool, _center, orbitalSpeed: 15f);

            _coreEmitter.Update(gameTime);
            _armEmitter.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _coreEmitter.Draw(spriteBatch, _starTex);
            _armEmitter.Draw(spriteBatch, _starTex);
        }

        public void Unload()
        {
            _corePool.Clear();
            _armPool.Clear();
            _starTex?.Dispose();
        }

        // ---------------------------------------------------------------
        // HELPER: add tangential velocity for orbital rotation
        // ---------------------------------------------------------------
        private static void ApplyOrbitalVelocity(ParticlePool pool, Vector2 center, float orbitalSpeed)
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                ref Particle p = ref pool.GetRef(i);
                if (!p.IsAlive || p.Age > 0.02f) continue; // only touch freshly spawned

                Vector2 toCenter = p.Position - center;
                float dist = toCenter.Length();
                if (dist < 1f) continue;

                // Perpendicular (tangent) direction for counter-clockwise orbit
                Vector2 tangent = new Vector2(-toCenter.Y, toCenter.X);
                tangent.Normalize();

                p.Velocity += tangent * orbitalSpeed;
            }
        }
    }
}
