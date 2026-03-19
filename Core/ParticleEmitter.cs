using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameParticleEngine.Core
{
    public enum EmitterShape { Point, Cone, Ring }

    /// <summary>
    /// Configuration profile for a particle emitter.
    /// Each demo creates one or more EmitterConfig instances and passes them to ParticleEmitter.
    /// </summary>
    public class EmitterConfig
    {
        // Emission
        public float EmissionRate     = 100f;  // particles per second
        public EmitterShape Shape     = EmitterShape.Point;
        public float ConeAngle        = MathHelper.PiOver4; // half-angle for cone shape
        public float RingRadius       = 50f;

        // Speed
        public float SpeedMin         = 80f;
        public float SpeedMax         = 150f;

        // Life
        public float LifetimeMin      = 1.0f;
        public float LifetimeMax      = 2.5f;

        // Appearance
        public Color StartColor       = Color.White;
        public Color EndColor         = Color.Transparent;
        public float StartScale       = 1.0f;
        public float EndScale         = 0.0f;

        // Rotation
        public float RotationSpeedMin = -2f;
        public float RotationSpeedMax =  2f;

        // Global force applied to every particle each second (e.g. gravity)
        public Vector2 GlobalForce    = Vector2.Zero;
    }

    /// <summary>
    /// Manages spawning, updating, and rendering of particles using a shared <see cref="ParticlePool"/>.
    /// Rendering uses MonoGame SpriteBatch with additive blending for glow effects.
    /// </summary>
    public class ParticleEmitter
    {
        private readonly ParticlePool _pool;
        private readonly Random       _rng = new Random();
        private float _emissionAccumulator;

        public EmitterConfig Config   { get; set; }
        public Vector2 Position       { get; set; }
        public bool    IsEmitting     { get; set; } = true;

        public ParticleEmitter(ParticlePool pool, EmitterConfig config)
        {
            _pool  = pool;
            Config = config;
        }

        // ---------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------
        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // --- Emit new particles ---
            if (IsEmitting)
            {
                _emissionAccumulator += Config.EmissionRate * dt;
                int toSpawn = (int)_emissionAccumulator;
                _emissionAccumulator -= toSpawn;

                for (int i = 0; i < toSpawn; i++)
                    SpawnOne();
            }

            // --- Update live particles ---
            for (int i = 0; i < _pool.Capacity; i++)
            {
                ref Particle p = ref _pool.GetRef(i);
                if (!p.IsAlive) continue;

                p.Age += dt;
                if (p.Age >= p.Lifetime) { _pool.Kill(i); continue; }

                float t = p.NormalizedAge;

                // Integrate velocity
                p.Velocity  += (p.Acceleration + Config.GlobalForce) * dt;
                p.Position  += p.Velocity * dt;
                p.Rotation  += p.AngularVelocity * dt;

                // Lerp colour and scale
                p.Color      = Color.Lerp(Config.StartColor, Config.EndColor, t);
                p.Scale      = MathHelper.Lerp(Config.StartScale, Config.EndScale, t);
            }
        }

        // ---------------------------------------------------------------
        // DRAW
        // ---------------------------------------------------------------
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            for (int i = 0; i < _pool.Capacity; i++)
            {
                ref Particle p = ref _pool.GetRef(i);
                if (!p.IsAlive) continue;

                spriteBatch.Draw(
                    texture,
                    p.Position,
                    null,
                    p.Color,
                    p.Rotation,
                    new Vector2(texture.Width * 0.5f, texture.Height * 0.5f),
                    p.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        // ---------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------
        private void SpawnOne()
        {
            int idx = _pool.Spawn();
            if (idx < 0) return;

            ref Particle p = ref _pool.GetRef(idx);

            // Direction based on shape
            Vector2 dir = Config.Shape switch
            {
                EmitterShape.Cone  => RandomConeDir(Config.ConeAngle),
                EmitterShape.Ring  => RandomRingDir(),
                _                  => RandomCircleDir()
            };

            float speed    = Lerp(Config.SpeedMin, Config.SpeedMax, RandF());
            p.Position     = Position + (Config.Shape == EmitterShape.Ring
                                ? dir * Config.RingRadius
                                : Vector2.Zero);
            p.Velocity     = dir * speed;
            p.Lifetime     = Lerp(Config.LifetimeMin, Config.LifetimeMax, RandF());
            p.AngularVelocity = Lerp(Config.RotationSpeedMin, Config.RotationSpeedMax, RandF());
            p.Rotation     = RandF() * MathHelper.TwoPi;
            p.Color        = Config.StartColor;
            p.EndColor     = Config.EndColor;
            p.Scale        = Config.StartScale;
            p.EndScale     = Config.EndScale;
            p.Acceleration = Vector2.Zero;
        }

        private Vector2 RandomCircleDir()
        {
            float angle = RandF() * MathHelper.TwoPi;
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private Vector2 RandomConeDir(float halfAngle)
        {
            // Emit upward by default (-Y), spread by halfAngle
            float angle = -MathHelper.PiOver2 + Lerp(-halfAngle, halfAngle, RandF());
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private Vector2 RandomRingDir()
        {
            float angle = RandF() * MathHelper.TwoPi;
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private float RandF()                      => (float)_rng.NextDouble();
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
