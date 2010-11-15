//-----------------------------------------------------------------------------
//           Name: TextureSplatting.fx
//         Author: Tim Jones
//  Last Modified: 26/05/05
//    Description: This effect combines four textures using a combined
//                 alpha map texture to determine the opacity of each layer.
//                 Thus, it is an implementation of texture splatting. 
//-----------------------------------------------------------------------------

#include "..\..\Shaders\ShadowMapping.fxh"
#include "..\..\Sky\AtmosphericScattering.fxh"


//-----------------------------------------------------------------------------
// variables
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

float2 offset;

float patchSize;
float heightMapSize;

const float3 LightDirection;
const float4 LightDiffuse;
const float4 LightAmbient;

// contains both normal map and shadow map. normal map is contained within
// the R, G and B channels, packed into 0 to 1. shadow map is in the A channel.
const texture LightTexture;

const texture LayerMap0Texture;
const texture LayerMap1Texture;
const texture LayerMap2Texture;
const texture BlendMapTexture;

//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

sampler LightSampler = sampler_state
{
	Texture = <LightTexture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

sampler LayerMap0Sampler = sampler_state
{
	Texture = <LayerMap0Texture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler LayerMap1Sampler = sampler_state
{
	Texture = <LayerMap1Texture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler LayerMap2Sampler = sampler_state
{
	Texture = <LayerMap2Texture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler BlendMapSampler = sampler_state
{
	Texture = <BlendMapTexture>;
	MagFilter = NONE;
	MinFilter = NONE;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VS_INPUT
{
	// this is slightly unusual because we have two vertex streams
	float2 posxz    : POSITION0;
	float  posy     : POSITION1;
};

struct VS_OUTPUT
{
	float4 position : POSITION;
	float2 UntiledTexCoords : TEXCOORD0;
	float2 TiledTexCoords   : TEXCOORD1;
	float4 rayleigh : TEXCOORD2;
	float4 mie      : TEXCOORD3;
};

struct PS_OUTPUT
{
	float4 colour : COLOR;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VS_OUTPUT VS(VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	// calculate position
	float4 vCombinedPos = float4(
		IN.posxz.x,
		IN.posy,
		IN.posxz.y,
		1);
	vCombinedPos.xz += offset;
	OUT.position = mul(vCombinedPos, WorldViewProjection);
	
	// calculate texture coordinates
	OUT.UntiledTexCoords.xy = (offset + IN.posxz) / heightMapSize;
	OUT.TiledTexCoords.xy = offset + IN.posxz;
	
	GetAtmosphericGroundDataVS(LightDirection, vCombinedPos.xyz, OUT.rayleigh, OUT.mie);

	return OUT;
}

PS_OUTPUT PS(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	
	// layer maps are tiled
	float3 c0 = tex2D(LayerMap0Sampler, IN.TiledTexCoords);
	float3 c1 = tex2D(LayerMap1Sampler, IN.TiledTexCoords);
	float3 c2 = tex2D(LayerMap2Sampler, IN.TiledTexCoords);
	
	// blendmap is not tiled
	float3 blend = tex2D(BlendMapSampler, IN.UntiledTexCoords);
	
	// find the inverse of all the blend weights so that we can
	// scale the total colour to the range [0, 1]
	float totalInverse = 1.0f / (blend.r + blend.g + blend.b);
	
	// calculate texture colour
	c0 *= blend.r * totalInverse;
	c1 *= blend.g * totalInverse;
	c2 *= blend.b * totalInverse;
	float4 finalTextureColour = float4((c0 + c1 + c2), 1);
	
	// unpack normal
	float3 normal = tex2D(LightSampler, IN.UntiledTexCoords + (0.5f / heightMapSize)).rgb;
	normal -= 0.5f;
	normal *= 2.0f;
	normal = normalize(normal);
	
	float3 lightDirection = LightDirection;
	//lightDirection.y *= -1;
	//lightDirection.z *= -1;
	
	float4 diffuse = (LightAmbient + LightDiffuse * saturate(dot(-lightDirection, normal))) * finalTextureColour;
	OUT.colour = GetAtmosphericGroundColourPS(diffuse, IN.rayleigh, IN.mie);
	OUT.colour.a = 1;
	return OUT;
}

PS_OUTPUT PS2(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	OUT.colour = float4(0.5, 0.5, 0.5, 0.1);
	return OUT;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------
technique NormalScene
{
	pass Pass0
	{
		FillMode = SOLID;
		ZEnable = true;
		CullMode = CCW;
		ZWriteEnable = true;
		
		Sampler[0] = <LightSampler>;
		
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
	
	/*pass Pass1
	{
		FillMode = WIREFRAME;
		ZEnable = true;
		CullMode = CCW;
		
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS2();
	}*/
}