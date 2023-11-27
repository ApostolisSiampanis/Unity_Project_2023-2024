#ifndef PATTERN_GENERATOR_INCLUDED
	#define PATTERN_GENERATOR_INCLUDED

	#include "Math.hlsl"

	float2 _perlinNoiseDir(float2 p, int seed)
	{
		int k = 289 + seed;
		p = p % k;
		float x = (34 * p.x + 1) * p.x % k + p.y;
		x = (34 * x + 1) * x % k;
		x = frac(x / 41) * 2 - 1;
		return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
	}

	float perlinNoise(float2 p, int seed)
	{
		float2 ip = floor(p);
		float2 fp = frac(p);
		float d00 = dot(_perlinNoiseDir(ip, seed), fp);
		float d01 = dot(_perlinNoiseDir(ip + float2(0, 1), seed), fp - float2(0, 1));
		float d10 = dot(_perlinNoiseDir(ip + float2(1, 0), seed), fp - float2(1, 0));
		float d11 = dot(_perlinNoiseDir(ip + float2(1, 1), seed), fp - float2(1, 1));
		fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
		float f = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
		return f;
	}

	float perlinNoise(float2 p)
	{
		return perlinNoise(p, 0);
	}

	float2 _voronoiRandomVector(float2 uv, float offset)
	{
		float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
		uv = frac(sin(mul(uv, m)) * 46839.32);
		return float2(sin(uv.y * + offset) * 0.5 + 0.5, cos(uv.x * offset) * 0.5 + 0.5);
	}

	float2 voronoi(float2 uv, float angleOffset, float cellDensity)
	{
		float2 g = floor(uv * cellDensity);
		float2 f = frac(uv * cellDensity);
		float t = 8.0;
		float3 res = float3(8.0, 0.0, 0.0);
		float2 noiseValue = 0;

		for (int y = -1; y <= 1; y++)
		{
			for (int x = -1; x <= 1; x++)
			{
				float2 lattice = float2(x, y);
				float2 offset = _voronoiRandomVector(lattice + g, angleOffset);
				float d = distance(lattice + offset, f);
				if (d < res.x)
				{
					res = float3(d, offset.x, offset.y);
					noiseValue = res.xy;
				}
			}
		}

		return noiseValue;
	}

	void _Hash_Tchou_2_1_uint(uint2 v, out uint o)
	{
		// ~6 alu (2 mul)
		v.y ^= 1103515245U;
		v.x += v.y;
		v.x *= v.y;
		v.x ^= v.x >> 5u;
		v.x *= 0x27d4eb2du;
		o = v.x;
	}

	void _Hash_Tchou_2_1_float(float2 i, out float o)
	{
		uint r;
		uint2 v = (uint2) (int2) round(i);
		_Hash_Tchou_2_1_uint(v, r);
		o = r * (1.0 / float(0xffffffff));
	}

	float _Unity_SimpleNoise_ValueNoise_Deterministic_float (float2 uv)
	{
		float2 i = floor(uv);
		float2 f = frac(uv);
		f = f * f * (3.0 - 2.0 * f);
		uv = abs(frac(uv) - 0.5);
		float2 c0 = i + float2(0.0, 0.0);
		float2 c1 = i + float2(1.0, 0.0);
		float2 c2 = i + float2(0.0, 1.0);
		float2 c3 = i + float2(1.0, 1.0);
		float r0; _Hash_Tchou_2_1_float(c0, r0);
		float r1; _Hash_Tchou_2_1_float(c1, r1);
		float r2; _Hash_Tchou_2_1_float(c2, r2);
		float r3; _Hash_Tchou_2_1_float(c3, r3);
		float bottomOfGrid = lerp(r0, r1, f.x);
		float topOfGrid = lerp(r2, r3, f.x);
		float t = lerp(bottomOfGrid, topOfGrid, f.y);
		return t;
	}

	void _Unity_SimpleNoise_Deterministic_float(float2 UV, float Scale, out float Out)
	{
		float freq, amp;
		Out = 0.0f;
		freq = pow(2.0, float(0));
		amp = pow(0.5, float(3-0));
		Out += _Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
		freq = pow(2.0, float(1));
		amp = pow(0.5, float(3-1));
		Out += _Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
		freq = pow(2.0, float(2));
		amp = pow(0.5, float(3-2));
		Out += _Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
	}

	float simpleNoise(float2 uv, float scale)
	{
		float result;
		_Unity_SimpleNoise_Deterministic_float(uv, scale, result);
		return result;
	}

	float simpleNoise_Scale10k(float2 uv)
	{
		float result;
		_Unity_SimpleNoise_Deterministic_float(uv, 10000.0, result);
		return result;
	}
#endif