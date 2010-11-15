// Render Shadow Map

const matrix LightWorldViewProjection : WORLDVIEWPROJECTION;

struct SMVS_INPUT
{
	float4 Pos : POSITION;
};

struct SMVS_OUTPUT
{
	float4 Pos      : POSITION;
	float4 PixelPos : TEXCOORD0;
};

struct SMPS_OUTPUT
{
	float4 Color : COLOR0;
};

SMVS_OUTPUT ShadowMapVertexShader(SMVS_INPUT input)
{
	SMVS_OUTPUT output;
	
  // pass vertex position through as usual
  output.Pos = mul(input.Pos, LightWorldViewProjection);
  
  // output pixel pos
  output.PixelPos = output.Pos;
  output.PixelPos /= output.PixelPos.w;
  
  return output;
}

SMPS_OUTPUT ShadowMapPixelShader(SMVS_OUTPUT input)
{
  // write z coordinate (linearized depth) to texture
  SMPS_OUTPUT output;
  output.Color = input.PixelPos.z;
  return output;
}

technique ShadowMapTechnique
{
	pass Pass0
	{
		CullMode = CW;
		DepthBias = 0;
		AlphaBlendEnable = false;
		/*AlphaTestEnable = false;*/
		
		VertexShader = compile vs_2_0 ShadowMapVertexShader();
		PixelShader = compile ps_2_0 ShadowMapPixelShader();
	}
}



// Use Shadow Map

const texture ShadowMap;
const float ShadowMapSize;
const float ShadowMapSizeInverse;
const matrix ShadowMapProjector;

// no filtering in floating point texture
sampler2D ShadowMapSampler = sampler_state
{
	Texture = <ShadowMap>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = NONE;
  AddressU = BORDER;
  AddressV = BORDER;
  BorderColor = 0xFFFFFFFF;
};

float4 GetShadowTexCoords(float4 position)
{
  return mul(position, ShadowMapProjector);
}

float GetShadowPS(float4 shadowTexCoords)
{
	float fTexelSize = 1.0f / ShadowMapSize;
	
	shadowTexCoords.z += 1;

	// project texture coordinates
	//return float4(shadowTexCoords.x, 0, 0, 1);
	shadowTexCoords.xy /= shadowTexCoords.w;
	
	float shadowValue = tex2D(ShadowMapSampler, shadowTexCoords.xy).r;
	shadowTexCoords.z = saturate(shadowTexCoords.z);
	float lightingFactor = (shadowTexCoords.z <= shadowValue);

	/*// 2x2 PCF Filtering
	float shadowValues[4];
	shadowValues[0] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords.xy).r);
	shadowValues[1] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords.xy + float2(ShadowMapSizeInverse, 0)).r);
	shadowValues[2] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords.xy + float2(0, ShadowMapSizeInverse)).r);
	shadowValues[3] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords.xy + float2(ShadowMapSizeInverse, ShadowMapSizeInverse)).r);

	float2 lerpFactor = frac(ShadowMapSize * shadowTexCoords);
	float lightingFactor = lerp(lerp(shadowValues[0], shadowValues[1], lerpFactor.x),
															lerp(shadowValues[2], shadowValues[3], lerpFactor.x),
															lerpFactor.y);*/

	return lightingFactor;
}