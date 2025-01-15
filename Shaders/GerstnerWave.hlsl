void gerstner_float(float3 position, float3 direction, float phase, float time, float gravity, float depth, float amplitude, out float3 result)
{
	float magnitude = length(direction);

	float freq = sqrt((gravity * length(direction)) * (tanh(depth * magnitude)));
	float theta = (direction.x * position.x + direction.z * position.z) - freq * time - phase;

	float x = -(amplitude / ((float)(tanh(magnitude * depth))) * direction.x / magnitude * sin(theta));
    float y = cos(theta) * amplitude;
    float z = -(amplitude / ((float)(tanh(magnitude * depth))) * direction.z / magnitude * sin(theta));

	result = float3(x,y,z);
}
