#ifndef MATH_INCLUDED
#define MATH_INCLUDED

float inverseLerp(float value, float a, float b)
{
	float v = (value - a) / (b - a);
	float aeb = (a == b);
	return 0*aeb + v*(1-aeb);
}

float2 inverseLerp(float2 value, float2 a, float2 b)
{
	return float2(inverseLerp(value.x, a.x, b.x), inverseLerp(value.y, a.y, b.y));
}

float _randomValue(float seed)
{
	return frac(sin(dot(float2(seed, seed + 1), float2(12.98, 78.23))) * 43.54);
}

float _randomValue(float u, float v)
{
	return frac(sin(dot(float2(u, v), float2(12.9898, 78.233))) * 43758.5453);
}

float2 correctUV(float2 uv, float2 textureSize)
{
	float2 pixelPos = floor(uv * (textureSize));
	float fx = pixelPos.x / (textureSize.x - 1);
	float fy = pixelPos.y / (textureSize.y - 1);
	return float2(fx, fy);
}

float lerpUnclamped(float a, float b, float w)
{
	return a + w*(b-a);
}

#endif