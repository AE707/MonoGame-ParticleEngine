using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameParticleEngine.Demos
{
    /// <summary>
    /// Common interface for all particle demos.
    /// ParticleGame uses this to switch between Fire, Galaxy, and Explosion
    /// scenes at runtime without any coupling to concrete demo types.
    /// </summary>
    public interface IDemo
    {
        string Name { get; }
        void Load(GraphicsDevice graphicsDevice);
        void SetPosition(Vector2 position);
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
        void Unload();
    }
}
