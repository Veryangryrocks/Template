#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 MatrixTransform;

texture SpriteTexture;

sampler2D SpriteSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;

    output.Position = mul(input.Position, MatrixTransform);

    output.TexCoord = input.TexCoord;
    output.Color = input.Color;

    return output;
}

float4 MainPS(VSOutput input) : COLOR0
{
    float4 textureColor = tex2D(
        SpriteSampler,
        input.TexCoord);

    return textureColor * input.Color;
}

technique Basic
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}