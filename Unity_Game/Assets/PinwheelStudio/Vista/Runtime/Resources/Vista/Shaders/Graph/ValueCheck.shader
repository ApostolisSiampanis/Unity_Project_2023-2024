Shader "Hidden/Vista/Graph/ValueCheck"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

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
			#define BELOW_ZERO float4(0, 1, 0, 1)
			#define ABOVE_ONE float4(0, 1, 1, 1)

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.localPos = v.vertex;
				return o;
			}

			float4 frag(v2f input): SV_Target
			{
				float value = tex2D(_MainTex, input.localPos).r;
				if (value >= 0 && value <= 1)
				{
					return float4(value, 0, 0, 1);
				}
				else if (value < 0)
				{
					return BELOW_ZERO;
				}
				else if (value > 1)
				{
					return ABOVE_ONE;
				}
				return 0;
			}
			ENDCG

		}
	}
}
