#include "ScreenSpaceQuad.fxh"
#include "ZEncoding.fxh"

//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const texture ElevationTexture;
const texture NormalMapTexture;
const float TextureSize;

const float2 HeightReadbackTextureSize;
const texture OffsetsTexture; // offset into elevation and normal textures

//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

uniform sampler ElevationSampler = sampler_state           
{
    Texture   = <ElevationTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler NormalMapSampler = sampler_state           
{
    Texture   = <NormalMapTexture>;
    MipFilter = NONE;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler OffsetsSampler = sampler_state           
{
    Texture   = <OffsetsTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

float4 HeightReadbackVS(half2 posxy : POSITION) : POSITION
{
	return getOutputPosition(posxy, HeightReadbackTextureSize);
}

float4 HeightReadbackPS(float2 vPos : VPOS) : COLOR
{
	float2 textureOffset = tex2D(OffsetsSampler, (vPos + 0.5f) / HeightReadbackTextureSize);
	
	// do four texture lookups to get interpolated height - can do this in hardware
	// on 8800GT but not on 7600GS
	float2 uv = vPos + 0.5f + textureOffset;
	float2 uvElevationI = floor(uv);
	float2 uvElevationF = uv - uvElevationI;
	
	float2 uvNormalMap = uv / TextureSize;
	
	// get height at points
	float zf1 = tex2D(ElevationSampler, uvElevationI / TextureSize).x;
	float zf2 = tex2D(ElevationSampler, (uvElevationI + float2(1, 0)) / TextureSize).x;
	float zf3 = tex2D(ElevationSampler, (uvElevationI + float2(0, 1)) / TextureSize).x;
	float zf4 = tex2D(ElevationSampler, (uvElevationI + float2(1, 1)) / TextureSize).x;
	
	// interpolate heights
	float zf = lerp(lerp(zf1, zf2, uvElevationF.x), lerp(zf3, zf4, uvElevationF.x), uvElevationF.y);
	
	// calculate normal at point
	float4 normalfc = tex2D(NormalMapSampler, uvNormalMap);
	float3 normal = float3(normalfc.xy * 2.0f - 1.0f, 1.0f);
	normal.z = sqrt(1.0f - dot(normal.xy, normal.xy));
	normal = normalize(normal.xzy);
	
	return float4(zf, normal);
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique HeightReadback
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 HeightReadbackVS();
		PixelShader  = compile ps_3_0 HeightReadbackPS();
	}
}