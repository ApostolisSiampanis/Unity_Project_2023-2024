Shader "Hidden/Vista/Graph/Noise"
{
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag

	#include "UnityCG.cginc"
	#include "../Includes/Math.hlsl"
	#include "../Includes/PatternGenerator.hlsl"

	struct appdata
	{
		float4 vertex: POSITION;
		float2 uv: TEXCOORD0;
	};

	struct v2f
	{
		float2 uv: TEXCOORD0;
		float4 vertex: SV_POSITION;
		float4 localPos: TEXCOORD1;
	};

	float4 _WorldBounds;
	float2 _Offset;
	float _Scale;
	float _LayerCount;
	float _Lacunarity;
	float _Persistence;
	float2 _RandomOffset;
	int _NoiseType;
	float2 _TextureSize;
	sampler2D _RemapTex;
	int _Seed;

	#define PERLIN_RAW 0
	#define PERLIN_01 1
	#define BILLOW 2
	#define RIDGED 3

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
	}

	float2 localToWorldPos(float2 localPos)
	{
		float2 origin = _WorldBounds.xy;
		float2 size = _WorldBounds.zw;
		return lerp(origin, origin + size, localPos.xy);
	}

	float generateNoise(float2 localPos, float baseAmplitude)
	{
		float2 pos;
		float2 noisePos;
		float scale = _Scale;
		float amplitude = baseAmplitude;
		float noise = 0;
		float result = 0;
		int s = 1;

		for (int i = 1; i <= 4; ++i)
		{
			pos = localToWorldPos(localPos) + _Offset;// +_RandomOffset;
			pos = pos / scale;
			noise = perlinNoise(pos, _Seed);
			noise = (_NoiseType == PERLIN_RAW) * noise + (_NoiseType == PERLIN_01) * (1-(noise * 0.5 + 0.5)) + (_NoiseType == BILLOW) * abs(noise) + (_NoiseType == RIDGED) * (1 - abs(noise));
			noise *= amplitude;
			result += s * noise * (i <= _LayerCount);
			
			scale /= pow(_Lacunarity, i);
			amplitude *= pow(_Persistence, i);
			//s = -s;
		}

		return result;
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ColorMask R

		Pass
		{
			Name "No Warp"
			CGPROGRAM

			half frag(v2f input): SV_Target
			{
				float2 uv = correctUV(input.uv, _TextureSize);// + _RandomOffset;
				float noise = generateNoise(uv, 1);
				
				noise = sign(noise) * tex2D(_RemapTex, abs(noise.xx)).r;
				return noise;
			}
			
			ENDCG

		}
		Pass
		{
			Name "Angular Warp"
			CGPROGRAM

			float _WarpAngleMin;
			float _WarpAngleMax;
			float _WarpIntensity;

			float frag(v2f input): SV_Target
			{
				float2 uv = correctUV(input.uv, _TextureSize);// + _RandomOffset;
				float angleNoise = abs(generateNoise(uv, 1));
				float angle = lerp(_WarpAngleMin, _WarpAngleMax, angleNoise);
				float2 offset = float2(cos(angle), sin(angle)) * _WarpIntensity * 0.001;
				float noise = generateNoise(uv + offset, 1);
				noise = sign(noise) * tex2D(_RemapTex, abs(noise.xx)).r;
				return noise;
			}
			
			ENDCG

		}
		Pass
		{
			Name "Directional Warp"
			CGPROGRAM

			#include "../Includes/Geometry.hlsl"

			float2 _TexelSize;
			float _WarpIntensity;

			float3 sampleWorldPos(float2 localPos)
			{
				float2 posWS = localToWorldPos(localPos);
				float h = generateNoise(localPos, 1);
				return float3(posWS.x, h, posWS.y);
			}

			float frag(v2f input): SV_Target
			{
				float2 posLS = correctUV(input.localPos, _TextureSize);
				float2 texel = _TexelSize;

				float3 center = sampleWorldPos(posLS);
				float3 left = sampleWorldPos(posLS + float2(-texel.x, 0));
				float3 leftUp = sampleWorldPos(posLS + float2(-texel.x, texel.y));
				float3 up = sampleWorldPos(posLS + float2(0, texel.y));
				float3 upRight = sampleWorldPos(posLS + float2(texel.x, texel.y));
				float3 right = sampleWorldPos(posLS + float2(texel.x, 0));
				float3 rightDown = sampleWorldPos(posLS + float2(texel.x, -texel.y));
				float3 down = sampleWorldPos(posLS + float2(0, -texel.y));
				float3 downLeft = sampleWorldPos(posLS + float2(-texel.x, -texel.y));

				float3 normal = calculateNormal(center, left, leftUp, up, upRight, right, rightDown, down, downLeft);

				float2 offset = normal.xz * _WarpIntensity;
				float noise = generateNoise(posLS /*+ _RandomOffset*/ + offset, 1);
				noise = sign(noise) * tex2D(_RemapTex, abs(noise.xx)).r;
				return noise;
			}
			
			ENDCG

		}
	}
}
