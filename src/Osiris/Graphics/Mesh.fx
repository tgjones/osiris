//-----------------------------------------------------------------------------
// Description: 
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------

#include "..\Shaders\ShadowMapping.fxh"
#include "..\Shaders\PositionNormalTextured.fxh"


//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const Texture Texture;

const bool UseTexture;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

sampler Sampler = sampler_state
{
	Texture = <Texture>;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

PixelShaderOutput PixelShaderShadowedTextured(VertexShaderOutput input)
{
	PixelShaderOutput output = PixelShaderShadowed(input);
	
	float4 diffuse = (UseTexture) ? tex2D(Sampler, input.TexCoords) : Diffuse;
	output.Colour *= diffuse;
		
	return output;
}

PixelShaderOutput PixelShaderTextured(VertexShaderOutput input)
{
	PixelShaderOutput output = PixelShader(input);
	
	float4 diffuse = (UseTexture) ? tex2D(Sampler, input.TexCoords) : Diffuse;
	output.Colour *= diffuse;
		
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
		CullMode = None;
		VertexShader = compile vs_3_0 VertexShaderShadowed();
		PixelShader = compile ps_2_0 PixelShaderShadowedTextured();
	}
}

technique NormalScene
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = None;
		VertexShader = compile vs_3_0 VertexShader();
		PixelShader = compile ps_2_0 PixelShaderTextured();
	}
}