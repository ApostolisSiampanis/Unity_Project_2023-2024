Shader "Hidden/Vista/Graph/RgbaMerge"
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

			sampler2D _RTex;
			sampler2D _GTex;
			sampler2D _BTex;
			sampler2D _ATex;
			float4 _Multiplier;

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
				float r = tex2D(_RTex, input.localPos).r;
				float g = tex2D(_GTex, input.localPos).r;
				float b = tex2D(_BTex, input.localPos).r;
				float a = tex2D(_ATex, input.localPos).r;

				float4 color = float4(r,g,b,a)*_Multiplier;
				return color;
			}
			ENDCG
		}
	}
}
