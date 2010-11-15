//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const texture HeightMapTexture;

const float HeightMapSizeInverse;

const float GridSpacing;

const float2 WorldPosMin;

const float2 ToroidalOrigin;

const float ElevationTextureSize;

const texture CoarserLevelElevationTexture;
const float2 CoarserLevelTextureOffset;

#include "ScreenSpaceQuad.fxh"
#include "..\..\Maths\Noise.fxh"
#include "ZEncoding.fxh"

//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

uniform sampler HeightMapSampler = sampler_state
{
    Texture   = <HeightMapTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler CoarserLevelElevationSampler = sampler_state   
{
    Texture   = <CoarserLevelElevationTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU  = WRAP;
    AddressV  = WRAP;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

float4 UpdateElevationVS(half2 posxy : POSITION) : POSITION
{
	return getOutputPosition(posxy, ElevationTextureSize);
}

float GetFineHeightRandom(float2 vPos)
{	
	float2 texcoords = WorldPosMin + (vPos * GridSpacing);
	texcoords /= 2000.0f;
	
	float noise = hetero(texcoords, 5) - 0.6f;
	float normalisedNoise = noise / 0.776f;
	
	float f = 0.1f;
	float flattening = 3.0f;
	
	float u = (1.0f - f);
	normalisedNoise *= u * 2.0f;
	
	if (normalisedNoise < u)
	{
		normalisedNoise /= flattening;
	}
	else
	{
		normalisedNoise -= u - (u / flattening);
	}
		
	return normalisedNoise * 700;
}

float LookupHeight(float2 texcoord)
{
	//return tex2D(HeightMapSampler, texcoord + (0.5f * HeightMapSizeInverse)).x * 100.0f;// * 65535.0f;
	return tex2D(HeightMapSampler, texcoord).x;// * 65535.0f;
}

float4 UpdateElevationFromHeightMapTexturePS(float2 vPos : VPOS) : COLOR
{
	float2 texcoords = WorldPosMin + (vPos * GridSpacing);
	//float2 texcoords = WorldPosMin + (vPos * GridSpacing);
	texcoords *= HeightMapSizeInverse;
	
	// offset texture coordinates, see Directly Mapping Texels to Pixels
	// in the DirectX SDK helpfile
	float zf = LookupHeight(texcoords);
	
	// zf should always be an integer, since it gets packed into
	// the integer component of the floating-point texture
	zf = floor(zf);
	
	// compute zc by linearly interpolating the vertices
	// of the coarse-grid edge on which the sample lies.
	// we can do this by adding 0.5 to the texture coordinates,
	// and floor-ing them. if the fine texcoords were on an even coordinate,
	// they will not change. if they were odd, they will change
	// to the next texcoord in the coarser level.
	float2 uvDiv2 = vPos / 2;
	
	// calculate texcoords. if the fine texcoords are (x, y) then the coarse texcoords will be
	// as follows, assuming that (m, n) = floor((x, y) / 2) :
	// - even-even: (m, n), (m, n)
	// - even-odd:  (m, n), (m, n + 1)
	// - odd-even:  (m, n), (m + 1, n)
	// - odd-odd:   (m + 1, n), (m, n + 1)
	float2 zcTexCoord1 = floor(uvDiv2 + float2(0.5f, 0));
	float2 zcTexCoord2 = floor(uvDiv2 + float2(0, 0.5f));
	zcTexCoord1 = (zcTexCoord1 / ElevationTextureSize) + (CoarserLevelTextureOffset + 0.5f) / ElevationTextureSize;
	zcTexCoord2 = (zcTexCoord2 / ElevationTextureSize) + (CoarserLevelTextureOffset + 0.5f) / ElevationTextureSize;
	
	// sample coarser heights
	float zc1 = decodeHeight(tex2D(CoarserLevelElevationSampler, float4(zcTexCoord1, 0, 1)).r);
	float zc2 = decodeHeight(tex2D(CoarserLevelElevationSampler, float4(zcTexCoord2, 0, 1)).r);
	
	// average coarser heights
	float zc = (zc1 + zc2) / 2;
	
	// pack both fine and coarser heights into a single float
	//float zf_zd = encodeHeights(zf, zc);
	float zf_zd = 0;

	//return float4((zf / 5), 0, 0, 1);
	return float4(zf_zd, 0, 0, 0);
}

float4 UpdateElevationFromHeightMapTextureCoarsestPS(float2 vPos : VPOS) : COLOR
{
	float2 texcoords = WorldPosMin + (((vPos - ToroidalOrigin + ElevationTextureSize) % ElevationTextureSize) * GridSpacing);
	//float2 texcoords = WorldPosMin + (vPos * GridSpacing);
	texcoords *= HeightMapSizeInverse;
	
	// offset texture coordinates, see Directly Mapping Texels to Pixels
	// in the DirectX SDK helpfile
	float zf = LookupHeight(texcoords);
	
	// zf should always be an integer, since it gets packed into
	// the integer component of the floating-point texture
	zf = floor(zf);
	
	float zf_zd = zf + 0.5f;

	return float4(zf_zd, 0, 0, 0);
}


float4 UpdateElevationRandomPS(float2 vPos : VPOS) : COLOR
{
	float zf = GetFineHeightRandom(vPos);
	
	// compute zc by linearly interpolating the vertices
	// of the coarse-grid edge on which the sample lies.
	// we can do this by adding 0.5 to the texture coordinates,
	// and floor-ing them. if the fine texcoords were on an even coordinate,
	// they will not change. if they were odd, they will change
	// to the next texcoord in the coarser level.
	float2 uvDiv2 = vPos / 2;
	
	// calculate texcoords. if the fine texcoords are (x, y) then the coarse texcoords will be
	// as follows, assuming that (m, n) = floor((x, y) / 2) :
	// - even-even: (m, n), (m, n)
	// - even-odd:  (m, n), (m, n + 1)
	// - odd-even:  (m, n), (m + 1, n)
	// - odd-odd:   (m + 1, n), (m, n + 1)
	float2 zcTexCoord1 = floor(uvDiv2 + float2(0.5f, 0));
	float2 zcTexCoord2 = floor(uvDiv2 + float2(0, 0.5f));
	zcTexCoord1 = (zcTexCoord1 / ElevationTextureSize) + (CoarserLevelTextureOffset + 0.5f) / ElevationTextureSize;
	zcTexCoord2 = (zcTexCoord2 / ElevationTextureSize) + (CoarserLevelTextureOffset + 0.5f) / ElevationTextureSize;
	
	// sample coarser heights
	float zc1 = decodeHeight(tex2D(CoarserLevelElevationSampler, float4(zcTexCoord1, 0, 1)).r);
	float zc2 = decodeHeight(tex2D(CoarserLevelElevationSampler, float4(zcTexCoord2, 0, 1)).r);
	
	// average coarser heights
	float zc = (zc1 + zc2) / 2;
	
	// pack both fine and coarser heights into a single float
	//float zf_zd = encodeHeights(zf, zc);
	//return float4(zf_zd, 0, 0, 0);
	return float4(zf, zc - zf, 0, 0);
}

float4 UpdateElevationRandomCoarsestPS(float2 vPos : VPOS) : COLOR
{
	float zf = GetFineHeightRandom(vPos);
	//float zf_zd = zf + 0.5f;
	//return float4(zf_zd, 0, 0, 0);
	return float4(zf, 0, 0, 0);
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique UpdateElevationFromHeightMapTexture
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 UpdateElevationVS();
		PixelShader  = compile ps_3_0 UpdateElevationFromHeightMapTexturePS();
	}
}

technique UpdateElevationFromHeightMapTextureCoarsest
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 UpdateElevationVS();
		PixelShader  = compile ps_3_0 UpdateElevationFromHeightMapTextureCoarsestPS();
	}
}

technique UpdateElevationRandom
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 UpdateElevationVS();
		PixelShader  = compile ps_3_0 UpdateElevationRandomPS();
	}
}

technique UpdateElevationRandomCoarsest
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 UpdateElevationVS();
		PixelShader  = compile ps_3_0 UpdateElevationRandomCoarsestPS();
	}
}