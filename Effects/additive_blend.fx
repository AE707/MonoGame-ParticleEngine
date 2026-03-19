// =============================================================================
// additive_blend.fx
// MonoGame HLSL Effect for additive-blended particle rendering.
//
// Additive blending: destination colour = src + dst
// This creates glowing fire/electricity/galaxy effects by brightening
// overlapping particles instead of covering them.
//
// Usage:
//   spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, effect: _additiveEffect);
//   emitter.Draw(spriteBatch, _particleTexture);
//   spriteBatch.End();
// =============================================================================

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// ----------------------------------------------------------------------------
// Uniforms
// ----------------------------------------------------------------------------
matrix WorldViewProjection;   // set automatically by MonoGame SpriteBatch
float  GlobalAlpha = 1.0f;    // master opacity multiplier [0..1]
float  SoftEdge    = 0.4f;    // radius at which alpha begins to fade (0 = hard edge)

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture   = <SpriteTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

// ----------------------------------------------------------------------------
// Vertex shader I/O
// ----------------------------------------------------------------------------
struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// ----------------------------------------------------------------------------
// Vertex shader  (pass-through; SpriteBatch handles transforms)
// ----------------------------------------------------------------------------
VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color    = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// ----------------------------------------------------------------------------
// Pixel shader
// ----------------------------------------------------------------------------
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 texColor = tex2D(SpriteTextureSampler, input.TexCoord);

    // Soft-edge radial fade: compute distance from centre of sprite quad [0..1]
    float2 centred  = input.TexCoord - float2(0.5f, 0.5f); // [-0.5 .. +0.5]
    float  dist     = length(centred) * 2.0f;               // [0 .. ~1.4]
    float  radAlpha = 1.0f - smoothstep(SoftEdge, 1.0f, dist);

    float4 finalColor = texColor * input.Color;
    finalColor.a     *= radAlpha * GlobalAlpha;

    // Pre-multiply alpha so additive blend works correctly with per-particle opacity
    finalColor.rgb   *= finalColor.a;

    return finalColor;
}

// ----------------------------------------------------------------------------
// Technique
// ----------------------------------------------------------------------------
technique AdditiveParticle
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader  = compile PS_SHADERMODEL MainPS();
    }
}
