#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
texture2D ModelTexture;
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD1;
};

sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    //output.Color = input.Color;
    //output.TextureCoordinate = input.TextureCoordinate;
    float4 worldPosition = mul(input.Position, World);
    output.TextureCoordinate = worldPosition;
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);


    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    textureColor.a = 1;
    return textureColor;
}

technique PlatformShading
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
