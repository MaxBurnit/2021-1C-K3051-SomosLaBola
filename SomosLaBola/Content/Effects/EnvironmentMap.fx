#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float4x4 World;
float4x4 InverseTransposeWorld;

float3 ambientColor; // Light's Ambient Color
float3 diffuseColor; // Light's Diffuse Color
float3 specularColor; // Light's Specular Color
float KAmbient; 
float KDiffuse; 
float KSpecular;
float shininess; 
float3 lightPosition;
float3 eyePosition;
float reflectionLevel;

texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture environmentMap;
samplerCUBE environmentMapSampler = sampler_state
{
    Texture = (environmentMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
	float4 Normal : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.WorldPosition = mul(input.Position, World);
    output.Normal = mul(float4(normalize(input.Normal.xyz), 1.0), InverseTransposeWorld);
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}

float4 EnvironmentMapPS(VertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);

	// Get the texture texel
    float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    
	// Calculate the diffuse light
    float NdotL = saturate(dot(input.Normal.xyz, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(input.Normal.xyz, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * specularColor * pow(saturate(NdotH), shininess);

    float4 finalLightColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);

	//Normalizar vectores
	float3 normal = normalize(input.Normal.xyz);
 
	//Obtener texel de CubeMap
	float3 view = normalize(eyePosition.xyz - input.WorldPosition.xyz);
	float3 reflection = reflect(view, normal);
	float3 reflectionColor = texCUBE(environmentMapSampler, reflection).rgb;
    return float4(lerp(finalLightColor, reflectionColor, reflectionLevel), 1);
}

VertexShaderOutput SphereVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPosition = mul(input.Position, World);
    output.Normal = mul(float4(normalize(input.Position.xyz), 1.0), InverseTransposeWorld);
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}

technique EnvironmentMap
{
    pass Pass0
    {
		VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL EnvironmentMapPS();
    }
};

technique EnvironmentMapSphere
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL SphereVS();
        PixelShader = compile PS_SHADERMODEL EnvironmentMapPS();
    }
};