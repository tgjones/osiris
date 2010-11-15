#include "AtmosphericScattering.fxh"

const float3 LightDirection;
const float4 LightDiffuse;
const float4 LightAmbient;

// vertex shader constants
const matrix WorldViewProjection : WORLDVIEWPROJECTION;

struct VS_INPUT
{
	float3 Position : POSITION0;
};

struct VS_OUTPUT
{
	float4 Position  : POSITION0;
	float3 Direction : TEXCOORD0;
	float4 Rayleigh  : TEXCOORD1;
	float4 Mie       : TEXCOORD2;
};


VS_OUTPUT RenderSkyVS(VS_INPUT input)
{
	VS_OUTPUT output;
	input.Position.xz += v3CameraPos.xz;
	GetAtmosphericSkyDataVS(LightDirection, input.Position, output.Direction, output.Rayleigh, output.Mie);
	input.Position.y -= fInnerRadius;
	output.Position = mul(float4(input.Position, 1), WorldViewProjection);
	output.Position.z = output.Position.w;
	return output;
}

float4 RenderSkyPS(VS_OUTPUT input) : COLOR
{
	return GetAtmosphericSkyColourPS(LightDirection, input.Direction, input.Rayleigh, input.Mie);
}

technique NormalScene
{
	pass p0
	{
		VertexShader = compile vs_2_0 RenderSkyVS();
		PixelShader = compile ps_2_0 RenderSkyPS();
		ZWriteEnable = true;
		ZEnable = true;
		FillMode = SOLID;
		CullMode = NONE;
	}
}