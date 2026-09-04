struct vertex_input
{
	float2 position : POSITION;
	float3 color : COLOR;
};

struct vertex_output
{
	float4 position : SV_POSITION;
	float3 color : COLOR;
};

vertex_output vertex_main(vertex_input input)
{
	vertex_output output;

	output.position = float4(input.position, 0.0f, 1.0f);
	output.color = input.color;

	return output;
}

float4 PixelMain(vertex_output input) : SV_TARGET
{
	return float4(input.color, 1.0f);
}
