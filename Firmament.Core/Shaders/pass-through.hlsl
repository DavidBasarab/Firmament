struct vertex_input
{
	float2 position : POSITION;
	float3 color : COLOR;
};

struct vertex_output
{
	float4 position : SV_POSITION;
	noperspective float3 color : COLOR;
};

vertex_output vertex_main(vertex_input input)
{
	vertex_output output;

	output.position = float4(input.position, 0.0f, 1.0f);
	output.color = input.color;

	return output;
}

float4 pixel_main(vertex_output input) : SV_TARGET
{
	// float highestWeight = max(input.color.r, max(input.color.g, input.color.b));
	//
	// return float4(step(highestWeight, input.color), 1.0f);

	// return float4(input.color, 1.0f);

	return float4(floor(input.color * 16.0f) / 16.0f, 1.0f);
}
