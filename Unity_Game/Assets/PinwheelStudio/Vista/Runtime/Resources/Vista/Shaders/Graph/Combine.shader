Shader "Hidden/Vista/Graph/Combine"
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

			sampler2D _Background;
			float _BackgroundMultiplier;

			sampler2D _Foreground;
			float _ForegroundMultiplier;

			sampler2D _Mask;
			float _MaskMultiplier;

			int _Mode;

			#define ADD 0
			#define SUB 1
			#define MUL 2
			#define MAX 3
			#define MIN 4
			#define LINEAR 5
			#define DIFF 6

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.localPos = v.vertex;
				return o;
			}

			float CalculateValue(float bg, float fg)
			{
				return(bg + fg) * (_Mode == ADD) +
				(bg - fg) * (_Mode == SUB) +
				(bg * fg) * (_Mode == MUL) +
				max(bg, fg) * (_Mode == MAX) +
				min(bg, fg) * (_Mode == MIN) +
				fg * (_Mode == LINEAR) +
				abs(bg - fg) * (_Mode == DIFF);
			}

			float frag(v2f input): SV_Target
			{
				float bg = tex2D(_Background, input.localPos).r * _BackgroundMultiplier;
				float fg = tex2D(_Foreground, input.localPos).r * _ForegroundMultiplier;
				float mask = tex2D(_Mask, input.localPos).r * _MaskMultiplier;

				float blend = CalculateValue(bg, fg);
				float result = lerp(bg, blend, mask);

				return result;
			}
			ENDCG

		}
	}
}
