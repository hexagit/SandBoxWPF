
#define USE_WORLDMATRIX	0

cbuffer Screen: register(b0)
{
	float4x4 Screen_ViewProjection;
	float4x4 Screen_View;
	float4x4 Screen_InverseViewProjection;
	float4x4 Screen_InverseView;

	float2	Screen_ScreenSize;
	float2	Screen_InverseScreenSize;
};
#if USE_WORLDMATRIX
cbuffer Mesh: register(b1)
{
	float4x4 Mesh_WorldTransform;
};
#endif

struct VertexShaderInput
{
	float3 position : POSITION;
	float4 normal : NORMAL;
};

struct VertexShaderOutput
{
	float4 position : SV_Position;
	float3 normal : NORMAL;
};

VertexShaderOutput main(VertexShaderInput input)
{
	VertexShaderOutput output;
#if 0
	output.position = mul(mul(float4(input.position, 1.0), Mesh_WorldTransform), Screen_ViewProjection);
#else
	output.position = mul(float4(input.position, 1.0), Screen_ViewProjection);
#endif
	output.normal = mul(input.normal.xyz, (float3x3)Screen_View);

	return output;
}

