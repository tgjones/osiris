//-----------------------------------------------------------------------------
// Description: 
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------

#include "..\..\Shaders\ShadowMapping.fxh"


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

#include "..\..\Shaders\PositionNormalTextured.fxh"


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
		PixelShader = compile ps_2_0 PixelShaderShadowed();
	}
}

technique NormalScene
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = CCW;
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
}