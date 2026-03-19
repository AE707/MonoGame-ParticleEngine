using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameParticleEngine
{
    /// <summary>
    /// Utility for generating particle textures at runtime.
    /// Avoids the MonoGame Content Pipeline for maximum portability.
    /// All textures are simple soft-edged circles — ideal for additive blending.
    /// </summary>
    public static class TextureHelper
    {
        /// <summary>
        /// Creates a square texture of size (diameter x diameter) containing
        /// a radially soft white circle. Alpha falls off smoothly from the centre
        /// to the edge using a smoothstep curve.
        /// </summary>
        /// <param name="graphicsDevice">The active GraphicsDevice.</param>
        /// <param name="radius">Radius of the circle in pixels. Texture will be (2*radius x 2*radius).</param>
        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius)
        {
            int diameter = radius * 2;
            Texture2D tex = new Texture2D(graphicsDevice, diameter, diameter);
            Color[] data    = new Color[diameter * diameter];

            Vector2 centre = new Vector2(radius, radius);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), centre);
                    float t    = dist / radius;             // [0..1] from centre
                    float alpha = 1f - SmoothStep(0.0f, 1.0f, t); // 1 at centre, 0 at edge
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);

                    byte a = (byte)(alpha * 255);
                    data[y * diameter + x] = new Color(a, a, a, a); // premultiplied white
                }
            }

            tex.SetData(data);
            return tex;
        }

        // Standard GLSL smoothstep implementation
        private static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = MathHelper.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return t * t * (3f - 2f * t);
        }
    }
}
