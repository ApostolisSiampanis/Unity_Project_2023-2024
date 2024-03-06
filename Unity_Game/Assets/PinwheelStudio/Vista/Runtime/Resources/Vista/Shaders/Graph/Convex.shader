Shader "Hidden/Vista/Graph/Convex"
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

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _Epsilon;
			float _Tolerance;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.localPos = v.vertex;
				return o;
			}

			float frag(v2f input): SV_Target
			{
				float2 texel = _MainTex_TexelSize.xy;
				float2 uv = input.localPos;

				float center = tex2D(_MainTex, uv).r;

				float2 uvL = uv + float2(-texel.x, 0);
				float left = tex2D(_MainTex, uvL).r;

				float2 uvLT = uv + float2(-texel.x, texel.y);
				float leftTop = tex2D(_MainTex, uvLT).r;

				float2 uvT = uv + float2(0, texel.y);
				float top = tex2D(_MainTex, uvT).r;

				float2 uvTR = uv + float2(texel.x, texel.y);
				float topRight = tex2D(_MainTex, uvTR).r;

				float2 uvR = uv + float2(texel.x, 0);
				float right = tex2D(_MainTex, uvR).r;

				float2 uvRB = uv + float2(texel.x, -texel.y);
				float rightBottom = tex2D(_MainTex, uvRB).r;

				float2 uvB = uv + float2(0, -texel.y);
				float bottom = tex2D(_MainTex, uvB).r;

				float2 uvBL = uv + float2(-texel.x, -texel.y);
				float bottomLeft = tex2D(_MainTex, uvBL).r;

				float e = _Epsilon/1000;
				float score = 0;
				score += (center > left - e) * (uvL.x >= 0);
				score += (center > leftTop - e) * (uvLT.x >= 0) * (uvLT.y <= 1);
				score += (center > top -e) * (uvT.y <= 1);
				score += (center > topRight - e) * (uvTR.x <= 1) * (uvTR.y <= 1);
				score += (center > right - e) * (uvR.x <= 1);
				score += (center > rightBottom - e) * (uvRB.x <= 1) * (uvRB.y >= 0);
				score += (center > bottom - e) * (uvB.y >= 0);
				score += (center > bottomLeft - e) * (uvBL.x >= 0) * (uvBL.y >= 0);

				score += _Tolerance;

				return score >= 8;
			}
			ENDCG

		}
	}
}
