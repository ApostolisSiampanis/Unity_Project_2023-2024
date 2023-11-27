Shader "Hidden/Vista/Graph/ColorBlend"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Blend Off

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
			sampler2D _Weight;
			float4 _Color;

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
				float4 bg = tex2D(_Background, input.localPos);
				float f = tex2D(_Weight, input.localPos).r;
				float4 color = lerp(bg, _Color, f);
				return color;
			}
			ENDCG
		}
	}
}
