Shader "Hidden/Vista/Graph/WeightBlend"
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
			sampler2D _Foreground;
			sampler2D _ForegroundMask;
			float _TargetValue;

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
				float bg = saturate(tex2D(_Background, input.localPos).r);
				float fg = saturate(tex2D(_Foreground, input.localPos).r) * saturate(tex2D(_ForegroundMask, input.localPos).r);
				float value = lerp(bg, _TargetValue, fg);
				return value;
			}
			ENDCG

		}
	}
}
