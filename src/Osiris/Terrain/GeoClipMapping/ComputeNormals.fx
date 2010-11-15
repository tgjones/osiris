#include "ScreenSpaceQuad.fxh"
#include "ZEncoding.fxh"

//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const texture ElevationTexture;
const float ElevationTextureSizeInverse;

const float NormalMapTextureSize;
const texture CoarserNormalMapTexture;
const float2 CoarserLevelTextureOffset;

float NormalScaleFactor; // 1.0 / ZMax for finest level, 1.0 / ( Zmax * grid unit level l )


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

uniform sampler CoarserNormalMapSampler = sampler_state           
{
    Texture   = <CoarserNormalMapTexture>;
    MipFilter = NONE;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

float4 ComputeNormalsVS(half2 posxy : POSITION) : POSITION
{
	return getOutputPosition(posxy, NormalMapTextureSize);
}

float2 GetFineNormal(float2 p_uv)
{
	p_uv = floor(p_uv) + 0.5f;

	float2 texcoord0 = float2(p_uv.x - 1, p_uv.y) * ElevationTextureSizeInverse;
	//float z00        = scaleHeight(decodeHeight(tex2D(ElevationSampler, texcoord0).x) * NormalScaleFactor);
	float z00        = tex2D(ElevationSampler, texcoord0).x;
	float2 texcoord1 = float2(p_uv.x + 1, p_uv.y) * ElevationTextureSizeInverse;
	//float z10        = scaleHeight(decodeHeight(tex2D(ElevationSampler, texcoord1).x) * NormalScaleFactor);
	float z10        = tex2D(ElevationSampler, texcoord1).x;

	texcoord0        = float2(p_uv.x, p_uv.y - 1) * ElevationTextureSizeInverse;
	//float z01        = scaleHeight(decodeHeight(tex2D(ElevationSampler, texcoord0).x) * NormalScaleFactor);
	float z01        = tex2D(ElevationSampler, texcoord0).x;
	texcoord1        = float2(p_uv.x, p_uv.y + 1) * ElevationTextureSizeInverse;
	//float z11        = scaleHeight(decodeHeight(tex2D(ElevationSampler, texcoord1).x) * NormalScaleFactor);
	float z11        = tex2D(ElevationSampler, texcoord1).x;

	float dx0 = (z10-z00);
	float dx1 = (z11-z01);
	/*float dx  = (dx0+dx1) * 0.5;

	float dy0 = (z01-z00);
	float dy1 = (z11-z10);
	float dy  = (dy0+dy1) * 0.5;

	float3 normalf = float3( -dx , -dy , 1.0 );
	normalf = normalize(normalf);
	normalf.xy = normalf.xy * 0.5 + 0.5;
	
	return normalf.xy;*/
	
	float3 dx = float3(2, dx0, 0);
	float3 dy = float3(0, dx1, 2);
	
	float3 normalf = normalize(cross(dx, dy));
	normalf.xz /= normalf.y;
	normalf.xz = normalf.xz * 0.5f + 0.5f;

	return normalf.xz;
}

float4 ComputeNormalsPS(float2 vPos : VPOS) : COLOR
{
	float2 fineNormal = GetFineNormal(vPos);
	
	float2 vPosDiv2 = vPos / 2.0f;
	float2 texcoordc = (vPosDiv2 + CoarserLevelTextureOffset + 0.5f) / NormalMapTextureSize;
	float2 normalc = tex2D(CoarserNormalMapSampler, texcoordc).rg;
	
	return float4(fineNormal, normalc);
}

float4 ComputeNormalsCoarsestPS(float2 vPos : VPOS) : COLOR
{
	float2 fineNormal = GetFineNormal(vPos);
	return float4(fineNormal, fineNormal);
}



technique ComputeNormals
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 ComputeNormalsVS();
		PixelShader  = compile ps_3_0 ComputeNormalsPS();
	}
}

technique ComputeNormalsCoarsest
{
	pass P0
	{
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
		CullMode = NONE;
		FillMode = SOLID;
		DepthBias = 0;

		VertexShader = compile vs_3_0 ComputeNormalsVS();
		PixelShader  = compile ps_3_0 ComputeNormalsCoarsestPS();
	}
}