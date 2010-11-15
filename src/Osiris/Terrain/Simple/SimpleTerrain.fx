//-----------------------------------------------------------------------------
// Description: 
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------

#include "..\..\Shaders\ShadowMapping.fxh"


//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const Texture GrassTexture;

const float TerrainWidth;
const float TerrainHeight;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

sampler GrassSampler = sampler_state
{
	Texture = <GrassTexture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	MipFilter = ANISOTROPIC;
	AddressU = WRAP;
	AddressV = WRAP;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

#include "..\..\Shaders\PositionNormalTextured.fxh"

PixelShaderOutput PixelShaderShadowedTextured(VertexShaderOutput input)
{
	PixelShaderOutput output = PixelShaderShadowed(input);
	float2 texCoords = input.TexCoords;
	texCoords.x *= TerrainWidth;
	texCoords.y *= TerrainHeight;
	output.Colour *= tex2D(GrassSampler, texCoords);
	return output;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique ShadowedScene
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = CCW;
		VertexShader = compile vs_3_0 VertexShaderShadowed();
		PixelShader = compile ps_2_0 PixelShaderShadowedTextured();
	}
}