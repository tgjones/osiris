float4 getOutputPosition(half2 posxy, float2 screenSize)
{
	float2 uv = posxy / screenSize;
	uv = (float2(uv.x, -uv.y) + float2(-0.5f, 0.5f)) * 2.0f;
	return float4(uv, 0.0f, 1.0f);
}