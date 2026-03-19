using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameParticleEngine.Core;

namespace MonoGameParticleEngine.Demos
{
    /// <summary>
    /// ExplosionDemo — spawns a burst explosion at the mouse click position.
    ///
    /// Technique highlights:
    ///   - Click anywhere to detonate a new explosion
    ///   - Each explosion is a one-shot burst: emitter fires then goes silent
    ///   - Three layered burst types per click:
    ///       1. Flash    : instant white bloom (very short life)
    ///       2. Shrapnel : high-speed point particles with gravity
    ///       3. Smoke    : large slow grey particles that drift upward
    ///   - Supports up to 8 simultaneous explosions before pool recycles
    ///   - Demonstrates one-shot vs continuous emitter patterns
    /// </summary>
    public class ExplosionDemo : IDemo
    {
        private const int MaxExplosions = 8;

        private readonly List<ExplosionInstance> _active = new();
        private Texture2D _particleTex;
        private MouseState _prevMouse;

        public string Name => "Explosion";

        public void Load(GraphicsDevice graphicsDevice)
        {
            _particleTex = TextureHelper.CreateCircleTexture(graphicsDevice, 24);
        }

        public void SetPosition(Vector2 position) { /* triggered by click, not drag */ }

        public void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            // Spawn new explosion on left-click
            if (mouse.LeftButton == ButtonState.Pressed &&
                _prevMouse.LeftButton == ButtonState.Released)
            {
                SpawnExplosion(new Vector2(mouse.X, mouse.Y));
            }
            _prevMouse = mouse;

            // Update all active explosions; remove fully dead ones
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                _active[i].Update(gameTime);
                if (_active[i].IsDead)
                {
                    _active[i].Unload();
                    _active.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var exp in _active)
                exp.Draw(spriteBatch, _particleTex);
        }

        public void Unload()
        {
            foreach (var exp in _active) exp.Unload();
            _active.Clear();
            _particleTex?.Dispose();
        }

        private void SpawnExplosion(Vector2 position)
        {
            if (_active.Count >= MaxExplosions)
            {
                _active[0].Unload();
                _active.RemoveAt(0);
            }
            _active.Add(new ExplosionInstance(position));
        }
    }

    // -------------------------------------------------------------------------
    // Helper: one self-contained explosion (3 emitters, all one-shot burst)
    // -------------------------------------------------------------------------
    internal class ExplosionInstance
    {
        private ParticlePool    _flashPool,    _shrapPool,    _smokePool;
        private ParticleEmitter _flashEmitter, _shrapEmitter, _smokeEmitter;
        private bool _fired;

        public bool IsDead =>
            _flashPool.ActiveCount == 0 &&
            _shrapPool.ActiveCount == 0 &&
            _smokePool.ActiveCount == 0 &&
            _fired;

        public ExplosionInstance(Vector2 position)
        {
            // Flash
            _flashPool    = new ParticlePool(200);
            _flashEmitter = new ParticleEmitter(_flashPool, new EmitterConfig
            {
                EmissionRate  = 0f,   // burst only
                Shape         = EmitterShape.Point,
                SpeedMin      = 20f,  SpeedMax  = 80f,
                LifetimeMin   = 0.1f, LifetimeMax = 0.25f,
                StartColor    = new Color(255, 255, 220),
                EndColor      = new Color(255, 200, 50, 0),
                StartScale    = 3f,   EndScale  = 0f,
                GlobalForce   = Vector2.Zero
            }) { Position = position, IsEmitting = false };

            // Shrapnel
            _shrapPool    = new ParticlePool(600);
            _shrapEmitter = new ParticleEmitter(_shrapPool, new EmitterConfig
            {
                EmissionRate  = 0f,
                Shape         = EmitterShape.Point,
                SpeedMin      = 150f, SpeedMax  = 400f,
                LifetimeMin   = 0.6f, LifetimeMax = 1.4f,
                StartColor    = new Color(255, 140, 20),
                EndColor      = new Color(80, 30, 10, 0),
                StartScale    = 0.8f, EndScale  = 0.1f,
                RotationSpeedMin = -6f, RotationSpeedMax = 6f,
                GlobalForce   = new Vector2(0f, 180f)  // gravity
            }) { Position = position, IsEmitting = false };

            // Smoke
            _smokePool    = new ParticlePool(300);
            _smokeEmitter = new ParticleEmitter(_smokePool, new EmitterConfig
            {
                EmissionRate  = 0f,
                Shape         = EmitterShape.Point,
                SpeedMin      = 20f,  SpeedMax  = 60f,
                LifetimeMin   = 1.5f, LifetimeMax = 3f,
                StartColor    = new Color(120, 120, 120, 180),
                EndColor      = new Color(60, 60, 60, 0),
                StartScale    = 2f,   EndScale  = 4f,
                RotationSpeedMin = -1f, RotationSpeedMax = 1f,
                GlobalForce   = new Vector2(0f, -30f)  // smoke rises
            }) { Position = position, IsEmitting = false };

            // Immediately burst all three emitters
            BurstEmitter(_flashEmitter,  _flashPool,  150);
            BurstEmitter(_shrapEmitter,  _shrapPool,  400);
            BurstEmitter(_smokeEmitter,  _smokePool,  120);
            _fired = true;
        }

        private static void BurstEmitter(ParticleEmitter emitter, ParticlePool pool, int count)
        {
            // Temporarily enable and force-spawn `count` particles in one frame
            emitter.IsEmitting = true;
            emitter.Config.EmissionRate = count * 60f; // will spawn count particles in 1/60s
            emitter.Update(new GameTime(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1.0 / 60.0)));
            emitter.IsEmitting = false;
            emitter.Config.EmissionRate = 0f;
        }

        public void Update(GameTime gameTime)
        {
            _flashEmitter.Update(gameTime);
            _shrapEmitter.Update(gameTime);
            _smokeEmitter.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tex)
        {
            _flashEmitter.Draw(spriteBatch, tex);
            _shrapEmitter.Draw(spriteBatch, tex);
            _smokeEmitter.Draw(spriteBatch, tex);
        }

        public void Unload()
        {
            _flashPool.Clear();
            _shrapPool.Clear();
            _smokePool.Clear();
        }
    }
}
