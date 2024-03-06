Shader "Hidden/Vista/Graph/Levels"
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
			float _InLow;
			float _InMid;
			float _InHigh;
			float _OutLow;
			float _OutHigh;

			float applyLevels(float v, float inLow, float inMid, float inHigh, float outLow, float outHigh)
			{
				if (v <= inMid)
				{
					v = inverseLerp(v, inLow, 2 * inMid - inLow);
				}
				else
				{
					v = inverseLerp(v, 2 * inMid - inHigh, inHigh);
				}

				v = lerp(outLow, outHigh, saturate(v));
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
				value = applyLevels(value, _InLow, _InMid, _InHigh, _OutLow, _OutHigh);
				return value;
			}
			ENDCG

		}
	}
}
