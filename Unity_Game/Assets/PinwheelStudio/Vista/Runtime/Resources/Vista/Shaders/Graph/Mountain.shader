Shader "Hidden/Vista/Graph/Mountain"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ColorMask R

		Pass
		{
			CGPROGRAM

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

			float2 _RandomOffset;
			float _Amplitude;
			int _LayerCount;
			float _ScaleMultiplier;
			float _AmplitudeMultiplier;
			float _DeformStrength;
			int _NoiseType;
			sampler2D _ShapeTexture;

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

			float generateNoise(float2 f, float baseAmplitude)
			{
				float2 pos;
				float2 noisePos;
				float scale = 1;
				float amplitude = baseAmplitude;
				float noise = 0;
				float result = 0;
				int sign = 1;

				for (int i = 0; i < 4; ++i)
				{
					pos = f / scale;
					noise = perlinNoise(pos);
					noise = (_NoiseType == PERLIN_RAW) * noise + (_NoiseType == PERLIN_01) * (noise * 0.5 + 0.5) + (_NoiseType == BILLOW) * abs(noise) + (_NoiseType == RIDGED) * (1 - abs(noise));
					noise *= amplitude;
					result += sign * noise * (i < _LayerCount);

					scale *= _ScaleMultiplier;
					amplitude *= _AmplitudeMultiplier;
					sign *= -1;
				}

				return result;
			}

			float frag(v2f input): SV_Target
			{
				float2 f = input.localPos * 2 - 1;
				float angleNoise = generateNoise(f + _RandomOffset.xy, 1);
				float angle = lerp(radians(-360), radians(360), angleNoise);
				float2 offset = float2(cos(angle), sin(angle)) * _DeformStrength * 0.01;

				float noise = generateNoise(f + offset +_RandomOffset.xy, _Amplitude);

				float falloff = 1 - saturate(length(f));
				float shape = tex2D(_ShapeTexture, float2(falloff, 0.5)).r;
				float h = shape * (1 + noise) * _Amplitude;

				return h;
			}
			ENDCG

		}
	}
}
