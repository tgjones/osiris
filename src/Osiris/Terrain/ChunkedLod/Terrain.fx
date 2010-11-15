//-----------------------------------------------------------------------------
//           Name: Terrain.fx
//         Author: Tim Jones
//  Last Modified: 26/05/05
//    Description: This effect combines four textures using a combined
//                 alpha map texture to determine the opacity of each layer.
//                 Thus, it is an implementation of texture splatting. 
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// variables
//-----------------------------------------------------------------------------

const matrix WorldViewProjection : WORLDVIEWPROJECTION;

// contains vertex heights
const texture ElevationTexture;
const float ElevationTextureSize;

// chunk offset and scale
const float2 ChunkOffset;
const float ChunkScale;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

sampler ElevationSampler = 
sampler_state
{
	Texture = <ElevationTexture>;
	MipFilter = POINT;
	MinFilter = POINT;
	MagFilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VertexShaderInput
{
	float2 posxz : POSITION0;
};

struct VertexShaderOutput
{
	float4 pos : POSITION;
	float3 worldPos : TEXCOORD0;
};

//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VertexShaderOutput VertexShader(VertexShaderInput input)
{
	// get height from elevation texture
	float2 uv = input.posxz / (ElevationTextureSize - 1.0f);
	float y = tex2Dlod(ElevationSampler, float4(uv, 0, 0)).x;
	
	// combine height with offset and scaled x and z coordinates
	float2 xz = (input.posxz * ChunkScale) + ChunkOffset;
	float4 combinedPos = float4(xz.x, y, xz.y, 1);
	
	VertexShaderOutput output;
	output.pos = mul(combinedPos, WorldViewProjection);
	output.worldPos = combinedPos.xyz;
	
	return output;
}

float4 PixelShader(VertexShaderOutput input) : COLOR
{
	return float4(input.worldPos.x / 5, input.worldPos.z / 5, 0, 1);
}

//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------
technique NormalScene
{
	pass Pass0
	{
		FillMode = WIREFRAME;
		ZEnable = true;
		CullMode = CW;
		
		VertexShader = compile vs_3_0 VertexShader();
		PixelShader = compile ps_2_0 PixelShader();
	}
}