Shader "Hidden/Vista/Graph/FalloffDetail"
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

			sampler2D _BaseMask;
			sampler2D _DetailMask;
			float _DetailMultiplier;

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
				float baseMask = saturate(tex2D(_BaseMask, input.localPos).r);
				float detailMask = saturate(tex2D(_DetailMask, input.localPos).r * _DetailMultiplier);

				float v0 = saturate(baseMask * detailMask);
				float v1 = saturate(baseMask + detailMask);
				float result = lerp(v0, v1, baseMask);

				return result;
			}
			ENDCG

		}
	}
}
