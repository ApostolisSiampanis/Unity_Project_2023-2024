Shader "Hidden/Vista/Graph/AxialGradient"
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
			#pragma shader_feature_local REPEAT

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

			float4 _TextureSize;
			float4x4 _UvToLineMatrix;

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
				float2 lineUV = mul(_UvToLineMatrix, float4(uv.xy, 0, 1));
				float f = lineUV.x;
				#if REPEAT
					f = frac(f);
				#else
					f = saturate(f);
				#endif

				return f;
			}
			ENDCG
		}
	}
}
