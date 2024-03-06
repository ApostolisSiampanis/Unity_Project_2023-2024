Shader "Hidden/Vista/Graph/MetallicSmoothnessOutput"
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

			sampler2D _Metallic;
			float _MetallicMultiplier;
			sampler2D _Smoothness;
			float _SmoothnessMultiplier;

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
				float metallic = tex2D(_Metallic, input.localPos).r;
				float smoothness = tex2D(_Smoothness, input.localPos).r;
				float4 color = float4(metallic * _MetallicMultiplier, 0, 0, smoothness * _SmoothnessMultiplier);

				return color;
			}
			ENDCG

		}
	}
}
