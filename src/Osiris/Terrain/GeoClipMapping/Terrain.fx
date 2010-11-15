//-----------------------------------------------------------------------------
// Description: Defines the shaders necessary for geoclipmapping on the GPU.
//              This single shader is used for all level footprints.
//              Parts of these shaders are taken from Asirvatham's and Hoppe's
//              paper "Terrain Rendering Using GPU-Based Geometry Clipmaps"
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------

#include "ZEncoding.fxh"
#include "..\..\Maths\Noise.fxh"
#include "..\..\Sky\AtmosphericScattering.fxh"

#include "..\..\Shaders\ShadowMapping.fxh"

//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

// ScaleFactor.xy: grid spacing of current level
// ScaleFactor.zw: origin of current block within world
const float4 ScaleFactor;

// shading colour used during debugging to differentiate between
// blocks, ring fix-ups, interior trim and outer degenerate triangles
const float4 Shading;

// FineBlockOrig.xy: 1/(w, h) of texture
// FineBlockOrig.zw: origin of block in texture
const float4 FineBlockOrig;

const float2 FineBlockOrig2;

// 2D texture that stores heights at this level's resolution.
// it is a floating pointing texture, with data packed into the float
const texture ElevationTexture;

// 2D texture that stores the normal data. This texture has twice
// the resolution of the elevation texture
const texture NormalMapTexture;

// position of viewer in world coordinates
const float2 ViewerPos;

// this is ((n - 1) / 2) - w - 1, where
// n = grid size
// w = transition width
const float2 AlphaOffset;

// this needs to be the inverse of the transition width, which we choose
// as n / 10
const float2 OneOverWidth;

// used when calculating the grid position of the vertex
const float GridSize;

// vector for the direction of sunlight
const float3 LightDirection;
const float4 LightAmbient;
const float4 LightDiffuse;

const float NormalMapTextureSize;
const float NormalMapTextureSizeInverse;

const texture GrassTexture;

const texture ColourMapTexture;
const float ColourMapTextureSize;

// ToroidalOffsets.xy: toroidal origin in texture coordinates
// ToroidalOffsets.zw: size of grid inside texture, used for modulation
const float4 ToroidalOffsets;

const float ElevationTextureSize;

const texture HeightColourMappingTexture;

const texture ColourTexture1;
const texture ColourTexture2;
const texture ColourTexture3;
const texture ColourTexture4;


//#define FAKESPHERE

const float SphereRadius = 200000.0;

float3 makesphere(float2 pos,float z)
{
#ifndef FAKESPHERE
  return (float3(pos.x,z,pos.y));
#else
	float3 center    = float3(0,-SphereRadius,0);
	float3 spherepos = center + normalize(float3(pos.x,0,pos.y) - center)*(SphereRadius + z);
	return (spherepos);
#endif
}


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

sampler ElevationSampler = 
sampler_state
{
	Texture = <ElevationTexture>;
	MipFilter = NONE;
	MinFilter = POINT;
	MagFilter = POINT;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler NormalMapSampler = 
sampler_state
{
	Texture = <NormalMapTexture>;
	MipFilter = NONE;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
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
    MipFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler ColourSampler2 = sampler_state           
{
    Texture   = <ColourTexture2>;
    MipFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler ColourSampler3 = sampler_state           
{
    Texture   = <ColourTexture3>;
    MipFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    AddressU  = WRAP;
    AddressV  = WRAP;
};

uniform sampler ColourSampler4 = sampler_state           
{
    Texture   = <ColourTexture4>;
    MipFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    AddressU  = WRAP;
    AddressV  = WRAP;
};


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VS_INPUT
{
	half2 posxy    : POSITION0;
};

struct VS_OUTPUT
{
	float4 position : POSITION;
	float4 uvShadow : TEXCOORD0;
	float2 uv       : TEXCOORD1;
	float2 alpha    : TEXCOORD2;
	float3 worldPos : TEXCOORD3;
	float4 rayleigh : TEXCOORD4;
	float4 mie      : TEXCOORD5;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VS_OUTPUT VS(VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	// convert from grid xy to world xy coordinates
	float2 worldPos = (IN.posxy * ScaleFactor.xy) + ScaleFactor.zw;
	
	// compute coordinates for vertex texture
	float2 uv = (IN.posxy * FineBlockOrig.xy) + FineBlockOrig.zw;
	float2 offsetUv = uv + ToroidalOffsets.xy;
	
	float2 zf_zd = tex2Dlod(ElevationSampler, float4(offsetUv + (0.5f / ElevationTextureSize), 0, 1)).xy;
	float zf = zf_zd.x;
	float zd = zf_zd.y;
	
	// compute alpha (transition parameter) and blend elevation
	float2 pos = IN.posxy + (FineBlockOrig2.xy * GridSize);
	float2 alpha = clamp((abs(pos - ViewerPos) - AlphaOffset) * OneOverWidth, 0, 1);
	alpha.x = max(alpha.x, alpha.y);
	float z = scaleHeight(zf + (alpha.x * zd));
	
	float3 spherepos = makesphere(worldPos,z);
	float4 inppos    = float4(spherepos.xyz,1.0);
	
	// transform position to screen space
	OUT.position = mul(inppos, WorldViewProjection);
	OUT.uv = uv;
	OUT.alpha = float2(alpha.x, 0);
	OUT.worldPos = inppos.xyz;
	
	//GetAtmosphericGroundDataVS(LightDirection, inppos.xyz, OUT.rayleigh, OUT.mie);
	OUT.rayleigh = 0;
	OUT.mie = 0;
	
	OUT.uvShadow = GetShadowTexCoords(inppos);
	
	return OUT;
}

float4 PS(VS_OUTPUT IN) : COLOR
{
	// do a texture lookup to get the normal in current level
	float4 normalfc = tex2D(NormalMapSampler, IN.uv + (0.5f / NormalMapTextureSize));
	
	// normalfc.xy contains normal at current (fine) level
	// normalfc.zw contains normal at coarser level
	// blend normals using alpha computed in vertex shader
	float3 normal = float3(lerp(normalfc.xy, normalfc.zw, IN.alpha.xx) * 2.0f - 1.0f, 1.0f);
	//normal.z = sqrt(1.0f - dot(normal.xy, normal.xy));
	normal = normalize(normal.xzy);

	//float noise = noise(IN.worldPos.x % IN.worldPos.z + IN.worldPos.z % IN.worldPos.y);
	//normal = normalize(normal + noise * 0.5f);
	
	// get texture relevant to this height
	
	// use normalised height to look up interpolated weights
	float4 weights = tex1D(HeightColourMappingSampler, IN.worldPos.y / 1000);
	
	// read colour samplers
	float2 colourTexcoord = (IN.worldPos.xz) / 10.0f;
	float3 c1 = tex2D(ColourSampler1, colourTexcoord).rgb;
	float3 c2 = tex2D(ColourSampler2, colourTexcoord).rgb;
	float3 c3 = tex2D(ColourSampler3, colourTexcoord).rgb;
	float3 c4 = tex2D(ColourSampler4, colourTexcoord).rgb;
	
	float4 detail = tex2D(ColourSampler4, IN.worldPos.xz / 1769).rrra;
	float4 material = float4(c1 * weights.x + c2 * weights.y + c3 * weights.z + c4 * weights.w, 1);
	//float4 material = float4(c1 * weights.x + c2 * weights.y, 1);
	
	// compute simple diffuse lighting
	float s = saturate(dot(normal, LightDirection));
	float shadow = GetShadowPS(IN.uvShadow);
	s *= shadow;
	
	float4 diffuse = (LightAmbient + LightDiffuse * s) * material * detail;
	
	//float4 result = GetAtmosphericGroundColourPS(diffuse, IN.rayleigh, IN.mie);
	//result.a = 1;
	
	//return result;
	return diffuse;
}

float4 PS_2(VS_OUTPUT IN) : COLOR
{
	return float4(0.3f, 0.3f, 0.3f, 1.0f);
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique ShadowedScene
{
	pass Pass0
	{
		AlphaBlendEnable = false;
		ZEnable = true;
		FillMode = SOLID;
		DepthBias = 0;
		CullMode = CW;
		
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
	
	/*pass Pass1
	{
		AlphaBlendEnable = true;
		FillMode = WIREFRAME;
		DepthBias = -0.00001f;
		
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_2_0 PS_2();
	}*/
}