using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameParticleEngine.Demos;

namespace MonoGameParticleEngine
{
    /// <summary>
    /// Main MonoGame entry point for the Particle System Engine demo.
    ///
    /// Controls:
    ///   1 / 2 / 3     — Switch between Fire / Galaxy / Explosion demos
    ///   Mouse move    — Move fire/explosion emitter position (demos 1 and 3)
    ///   Left click    — Detonate explosion at cursor (demo 3)
    ///   Escape        — Quit
    ///
    /// Architecture:
    ///   All demos implement IDemo. ParticleGame holds an IDemo[] array
    ///   and swaps the active demo on keypress, calling Unload/Load cleanly.
    ///   SpriteBatch is opened with BlendState.Additive so every Draw call
    ///   from any demo benefits from the glow blending without extra setup.
    /// </summary>
    public class ParticleGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch           _spriteBatch;
        private SpriteFont            _font;         // for HUD overlay

        private IDemo[] _demos;
        private int     _activeIndex;
        private IDemo   ActiveDemo => _demos[_activeIndex];

        private KeyboardState _prevKeys;
        private MouseState    _prevMouse;

        public ParticleGame()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth  = 1280,
                PreferredBackBufferHeight = 720,
                SynchronizeWithVerticalRetrace = true
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title   = "MonoGame Particle System Engine";
        }

        protected override void Initialize()
        {
            _demos = new IDemo[]
            {
                new FireDemo(),
                new GalaxyDemo(),
                new ExplosionDemo()
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load all demos (each creates its own pools and textures)
            foreach (var demo in _demos)
                demo.Load(GraphicsDevice);

            // Set initial emitter position to screen centre for Fire demo
            var centre = new Vector2(
                GraphicsDevice.Viewport.Width  * 0.5f,
                GraphicsDevice.Viewport.Height * 0.75f);
            ActiveDemo.SetPosition(centre);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keys  = Keyboard.GetState();
            MouseState    mouse = Mouse.GetState();

            // --- Quit ---
            if (keys.IsKeyDown(Keys.Escape))
                Exit();

            // --- Switch demo ---
            if (WasPressed(keys, _prevKeys, Keys.D1)) SwitchDemo(0);
            if (WasPressed(keys, _prevKeys, Keys.D2)) SwitchDemo(1);
            if (WasPressed(keys, _prevKeys, Keys.D3)) SwitchDemo(2);

            // --- Pass mouse position to active demo ---
            ActiveDemo.SetPosition(new Vector2(mouse.X, mouse.Y));

            // --- Update active demo ---
            ActiveDemo.Update(gameTime);

            _prevKeys  = keys;
            _prevMouse = mouse;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Additive blending: overlapping particles brighten each other (glow)
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                null, null, null, null);

            ActiveDemo.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void SwitchDemo(int index)
        {
            if (index == _activeIndex) return;
            _activeIndex = index;
            // Reset emitter to centre
            ActiveDemo.SetPosition(new Vector2(
                GraphicsDevice.Viewport.Width  * 0.5f,
                GraphicsDevice.Viewport.Height * 0.75f));
        }

        private static bool WasPressed(KeyboardState current, KeyboardState previous, Keys key)
            => current.IsKeyDown(key) && previous.IsKeyUp(key);
    }

    // -----------------------------------------------------------------------
    // Program entry point
    // -----------------------------------------------------------------------
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new ParticleGame();
            game.Run();
        }
    }
}
