//-----------------------------------------------------------------------------
// Description: Defines the shaders necessary for geoclipmapping on the GPU.
//              This single shader is used for all level footprints.
//              Parts of these shaders are taken from Asirvatham's and Hoppe's
//              paper "Terrain Rendering Using GPU-Based Geometry Clipmaps"
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

// ScaleFactor.xy: size of current patch
// ScaleFactor.zw: origin of current patch within world
const float4 ScaleFactor;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VS_INPUT
{
	float2 posxy    : POSITION0;
};

struct VS_OUTPUT
{
	float4 position : POSITION;
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
	
	// convert from grid xy to world xy coordinates
	float3 worldPos;
	worldPos.xz = (IN.posxy * ScaleFactor.xy) + ScaleFactor.zw;
	worldPos.y = 0;
	
	// transform position to screen space
	OUT.position = mul(WorldViewProjection, float4(worldPos, 1));
		
	return OUT;
}

PS_OUTPUT PS(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	OUT.colour = float4(.3, .3, .3, 0.7);
	return OUT;
}

PS_OUTPUT PS_2(VS_OUTPUT IN)
{
	PS_OUTPUT OUT;
	OUT.colour = float4(1, 0, 0, 0.7);
	return OUT;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique
{
	pass Pass0
	{
		//ZEnable = true;
		FillMode = SOLID;
		//CullMode = NONE;
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
	
	pass Pass1
	{
		FillMode = WIREFRAME;
		//ZEnable = false;
		
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_2_0 PS_2();
	}
}