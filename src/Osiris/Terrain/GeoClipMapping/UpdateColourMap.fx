#include "ScreenSpaceQuad.fxh"

//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const texture ElevationTexture;
const float ElevationTextureSizeInverse;

const float ColourMapTextureSize;

const texture HeightColourMappingTexture;

const texture ColourTexture1;
const texture ColourTexture2;
const texture ColourTexture3;
const texture ColourTexture4;

const float4 ColourBorders = { 0, 0.25f, 0.5f, 0.75f};


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

uniform sampler HeightColourMappingSampler = sampler_state           
{
    Texture   = <HeightColourMappingTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler ColourSampler1 = sampler_state           
{
    Texture   = <ColourTexture1>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler ColourSampler2 = sampler_state           
{
    Texture   = <ColourTexture2>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler ColourSampler3 = sampler_state           
{
    Texture   = <ColourTexture3>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler ColourSampler4 = sampler_state           
{
    Texture   = <ColourTexture4>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

float4 UpdateColourMapVS(half2 posxy : POSITION) : POSITION
{
	return getOutputPosition(posxy, ColourMapTextureSize);
}

float4 UpdateColourMapPS(float2 vPos : VPOS) : COLOR
{
	// get height at this point
	vPos = floor(vPos) + 0.5f;
	float2 texcoord = float2(vPos) * ElevationTextureSizeInverse;
	float z = tex2D(ElevationSampler, texcoord).x;
	
	// use normalised height to look up interpolated weights
	float4 weights = tex1D(HeightColourMappingSampler, z / 2000);
	
	// read colour samplers
	float2 colourTexcoord = vPos / 100.0f;
	float3 c1 = tex2D(ColourSampler1, colourTexcoord).rgb;
	float3 c2 = tex2D(ColourSampler2, colourTexcoord).rgb;
	float3 c3 = tex2D(ColourSampler3, colourTexcoord).rgb;
	float3 c4 = tex2D(ColourSampler4, colourTexcoord).rgb;
	
	return float4(c1 * weights.x + c2 * weights.y + c3 * weights.z + c4 * weights.w, 1);
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique UpdateColourMap
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 UpdateColourMapVS();
		PixelShader  = compile ps_3_0 UpdateColourMapPS();
	}
}