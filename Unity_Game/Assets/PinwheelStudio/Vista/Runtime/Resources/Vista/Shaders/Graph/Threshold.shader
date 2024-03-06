Shader "Hidden/Vista/Graph/Threshold"
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
			float _Low;
			float _High;
			float _Mode;

			float applyThresholdLow(float v, float low, float mode)
			{
				float v0 = v * (v >= low) + 0 * (v < low);
				float v1 = v * (v > low) + 0 * (v <= low);
				v = v0 * (mode == 0) + v1 * (mode == 1);
				return v;
			}

			float applyThresholdHigh(float v, float high, float mode)
			{
				float v0 = v * (v <= high) + 1 * (v > high);
				float v1 = v * (v < high) + 1 * (v >= high);
				v = v0 * (mode == 0) + v1 * (mode == 1);
				return v;
			}

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
				float value = tex2D(_MainTex, input.localPos).r;
				value = applyThresholdLow(value, _Low, _Mode);
				value = applyThresholdHigh(value, _High, _Mode);
				return value;
			}
			ENDCG
		}
	}
}
