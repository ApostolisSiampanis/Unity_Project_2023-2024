Shader "Hidden/Vista/Graph/AngularGradient"
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

			float2 _TextureSize;
			float4x4 _TransformMatrix;

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
				float2 uv = correctUV(input.localPos, _TextureSize);
				float2 gradientPoint = mul(_TransformMatrix, float4(uv.xy, 0, 1));
				if (gradientPoint.x == 0 && gradientPoint.y== 0)
					return 0;
				float2 normalizedPoint = normalize(gradientPoint);
				float rad = atan2(normalizedPoint.y, normalizedPoint.x);
				rad = (rad >= 0) * (rad) + (rad < 0) * (UNITY_TWO_PI + rad);
				float f = 1 - rad / UNITY_TWO_PI;

				return f;
			}
			ENDCG

		}
	}
}
