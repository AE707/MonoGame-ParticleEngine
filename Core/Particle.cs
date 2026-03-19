using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameParticleEngine.Core
{
    /// <summary>
    /// Represents a single particle in the simulation.
    /// Kept as a struct for cache-friendly memory layout and zero GC pressure.
    /// </summary>
    public struct Particle
    {
        // --- Transform ---
        public Vector2 Position;
        public Vector2 Velocity;
        public float   Rotation;       // radians
        public float   AngularVelocity; // radians per second

        // --- Appearance ---
        public Color   Color;
        public Color   EndColor;       // lerp target colour over lifetime
        public float   Scale;          // uniform scale
        public float   EndScale;       // lerp target scale over lifetime

        // --- Life ---
        public float   Lifetime;       // total lifetime in seconds
        public float   Age;            // elapsed time in seconds
        public bool    IsAlive;

        // --- Forces ---
        public Vector2 Acceleration;   // per-frame force accumulator

        // Normalised age [0..1] — cheap to compute, used heavily in lerps
        public float NormalizedAge => (Lifetime > 0f) ? (Age / Lifetime) : 1f;

        /// <summary>Reset to defaults so the pool can reuse this slot.</summary>
        public void Reset()
        {
            Position        = Vector2.Zero;
            Velocity        = Vector2.Zero;
            Rotation        = 0f;
            AngularVelocity = 0f;
            Color           = Color.White;
            EndColor        = Color.Transparent;
            Scale           = 1f;
            EndScale        = 0f;
            Lifetime        = 1f;
            Age             = 0f;
            IsAlive         = false;
            Acceleration    = Vector2.Zero;
        }
    }
}
