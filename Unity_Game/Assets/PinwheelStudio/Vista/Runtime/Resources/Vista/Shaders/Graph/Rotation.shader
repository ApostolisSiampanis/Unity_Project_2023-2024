Shader "Hidden/Vista/Graph/Rotation"
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
			#pragma shader_feature_local REMAP_01

			#include "UnityCG.cginc"
			#include "../Includes/Math.hlsl"
			#include "../Includes/Geometry.hlsl"

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
			float4 _MainTex_TexelSize;

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
				float3 normal = normalFromHeightMap(_MainTex, _MainTex_TexelSize, float3(1,1,1), input.localPos);
				if (normal.x == 0 && normal.z == 0)
					return 0;
				float2 dir = normalize(normal.xz);
				float rad = atan2(dir.y, dir.x);				
				rad = (rad >= 0) * (rad) + (rad < 0) * (UNITY_TWO_PI + rad);
				#if REMAP_01
					rad = rad/UNITY_TWO_PI;
				#endif
				return rad;
			}
			ENDCG
		}
	}
}
