Shader "Hidden/Vista/Graph/Voronoi"
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
	float _AmplitudeExponent;
	float2 _RandomOffset;
	float _Inverse;
	float2 _TextureSize;
	sampler2D _RemapTex;

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

	float2 generateNoise(float2 localPos, float baseAmplitude)
	{
		float2 pos;
		float2 noisePos;
		float scale = _Scale;
		float amplitude = baseAmplitude;
		float2 noise = 0;
		float2 result = 0;
		
		int s = 1;

		for (int i = 1; i <= 4; ++i)
		{
			pos = localToWorldPos(localPos) + _Offset;// +_RandomOffset;
			pos = pos / scale;
			noise = voronoi(pos, 45, 1);
			noise = pow(noise, abs(_AmplitudeExponent));
			noise = _Inverse * (1 - noise) + (1 - _Inverse) * noise;
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

		Pass
		{
			Name "No Warp"
			CGPROGRAM

			float2 frag(v2f input): SV_Target
			{
				float2 uv = correctUV(input.uv, _TextureSize) + _RandomOffset;
				float2 noise = generateNoise(uv, 1);
				noise.x = sign(noise.x) * tex2D(_RemapTex, abs(noise.xx)).r;
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

			float2 frag(v2f input): SV_Target
			{
				float2 uv = correctUV(input.uv, _TextureSize) + _RandomOffset;
				float angleNoise = abs(generateNoise(uv, 1).x);
				float angle = lerp(_WarpAngleMin, _WarpAngleMax, angleNoise);
				float2 offset = float2(cos(angle), sin(angle)) * _WarpIntensity * 0.001;
				float2 noise = generateNoise(uv + offset, 1);
				
				noise.x = sign(noise.x) * tex2D(_RemapTex, abs(noise.xx)).r;
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
				float h = generateNoise(localPos, 1).x;
				return float3(posWS.x, h, posWS.y);
			}

			float2 frag(v2f input): SV_Target
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
				float noise = generateNoise(posLS + offset +_RandomOffset, 1);
				noise.x = sign(noise.x) * tex2D(_RemapTex, abs(noise.xx)).r;
				return noise;
			}
			
			ENDCG

		}

		Pass
		{
			Name "Output Cells"
			CGPROGRAM

			sampler2D _VoronoiTex;

			float frag(v2f input): SV_Target
			{
				float v = tex2D(_VoronoiTex, input.uv).x;
				return v;
			}
			ENDCG

		}
		
		Pass
		{
			Name "Output Raw Cells"
			CGPROGRAM

			sampler2D _VoronoiTex;

			float frag(v2f input): SV_Target
			{
				float v = tex2D(_VoronoiTex, input.uv).y;
				return v;
			}
			ENDCG

		}

		Pass
		{
			Name "Output Outline"
			CGPROGRAM

			sampler2D _VoronoiTex;
			float4 _VoronoiTex_TexelSize;

			float frag(v2f input): SV_Target
			{
				float2 texel = _VoronoiTex_TexelSize.xy;
				float2 uv = input.uv;

				float center = tex2D(_VoronoiTex, uv).y;

				float2 uvL = uv + float2(-texel.x, 0);
				float left = tex2D(_VoronoiTex, uvL).y;

				float2 uvLT = uv + float2(-texel.x, texel.y);
				float leftTop = tex2D(_VoronoiTex, uvLT).y;

				float2 uvT = uv + float2(0, texel.y);
				float top = tex2D(_VoronoiTex, uvT).y;

				float2 uvTR = uv + float2(texel.x, texel.y);
				float topRight = tex2D(_VoronoiTex, uvTR).y;

				float2 uvR = uv + float2(texel.x, 0);
				float right = tex2D(_VoronoiTex, uvR).y;

				float2 uvRB = uv + float2(texel.x, -texel.y);
				float rightBottom = tex2D(_VoronoiTex, uvRB).y;

				float2 uvB = uv + float2(0, -texel.y);
				float bottom = tex2D(_VoronoiTex, uvB).y;

				float2 uvBL = uv + float2(-texel.x, -texel.y);
				float bottomLeft = tex2D(_VoronoiTex, uvBL).y;

				float score = 0;
				score += (center != left) * (uvL.x >= 0);
				score += (center != leftTop) * (uvLT.x >= 0) * (uvLT.y <= 1);
				score += (center != top) * (uvT.y <= 1);
				score += (center != topRight) * (uvTR.x <= 1) * (uvTR.y <= 1);
				score += (center != right) * (uvR.x <= 1);
				score += (center != rightBottom) * (uvRB.x <= 1) * (uvRB.y >= 0);
				score += (center != bottom) * (uvB.y >= 0);
				score += (center != bottomLeft) * (uvBL.x >= 0) * (uvBL.y >= 0);

				return score > 0;
			}
			ENDCG

		}
	}
}
