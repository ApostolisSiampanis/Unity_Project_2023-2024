Shader "Hidden/Vista/Graph/RadialGradient"
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

			float2 _TextureSize;
			float2 _Center;
			float _Radius;

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
				float d = distance(uv, _Center);
				float f = 0;
				#if REPEAT
					f = frac(1 - d / _Radius);
				#else
					f = saturate(1 - d / _Radius);
				#endif
				return f;
			}
			ENDCG

		}
	}
}
