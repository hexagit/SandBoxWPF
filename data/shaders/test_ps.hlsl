
#define USE_GBUFFER	0

struct VertexShaderOutput
{
	float4 position : SV_Position;
	float3 normal : NORMAL;
};

typedef VertexShaderOutput PixelShaderInput;

struct PixelShaderOutput
{
	float4 rtv0 : SV_Target0;
#if USE_GBUFFER
	uint rtv1 : SV_Target1;
#endif
};
#if USE_GBUFFER
uint PackRTV1(float2 normal, float roughness, float metalic)
{
	uint2 uiNormalXY = (uint2)((normal * 0.5 + 0.5) * 1023.0);	// to 10bit uint
	uint uiRoughness = (uint)(roughness * 127.0);
	uint uiMetalic = (uint)(metalic * 31.0);
	return (uiNormalXY.x << 22) | (uiNormalXY.y << 12) | (uiRoughness << 5) | uiMetalic;
};
#endif
PixelShaderOutput main(PixelShaderInput input)
{
	PixelShaderOutput output;
#if USE_GBUFFER
	output.rtv0 = float4(0.5, 0.5, 0.5, 0.0);
	output.rtv1 = PackRTV1(normalize(input.normal).xy, 0.7, 0.0);
#else
	output.rtv0 = float4(input.normal, 1.0);
#endif
	return output;
}
