//-----------------------------------------------------------------------------
// Description: 
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection;

const float3 LightDirection;
const float4 LightDiffuse;
const float4 LightAmbient;

const float4 Diffuse;


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VertexShaderInput
{
	float3 Position  : POSITION;
	float3 Normal    : NORMAL;
	float2 TexCoords : TEXCOORD;
};

struct VertexShaderOutput
{
	float4 Position          : POSITION;
	float4 ShadowTexCoords   : TEXCOORD0;
	float3 Diffuse           : TEXCOORD1;
	float2 TexCoords         : TEXCOORD2;
};

struct PixelShaderOutput
{
	float4 Colour : COLOR;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VertexShaderOutput VS(VertexShaderInput input)
{
	float4 inputPos = float4(input.Position, 1);
	
	VertexShaderOutput output;
	
	// pass vertex position through as usual
  output.Position = mul(inputPos, WorldViewProjection);
  
  // calculate per vertex lighting
  output.Diffuse = LightDiffuse * saturate(dot(-LightDirection, input.Normal));

  // coordinates for texture
  output.TexCoords = input.TexCoords;
  
  output.ShadowTexCoords = 0;
  
  return output;
}

VertexShaderOutput VertexShaderShadowed(VertexShaderInput input)
{
	VertexShaderOutput output = VS(input);
	
  // coordinates for shadowmap
  float4 inputPos = float4(input.Position, 1);
  output.ShadowTexCoords = GetShadowTexCoords(inputPos);
  
  return output;
}

PixelShaderOutput PS(VertexShaderOutput input)
{
	PixelShaderOutput output;
	output.Colour = Diffuse.a;
	output.Colour.rgb = saturate(LightAmbient + input.Diffuse + Diffuse).rgb;
	return output;
}

PixelShaderOutput PixelShaderShadowed(VertexShaderOutput input)
{
	float4 lightingFactor = GetShadowPS(input.ShadowTexCoords);

	// multiply diffuse with shadowmap lookup value
	input.Diffuse *= lightingFactor;

	// final color
	PixelShaderOutput output = PS(input);
	return output;
}