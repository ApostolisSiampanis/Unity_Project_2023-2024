Shader "Hidden/Vista/Graph/DirectionMask"
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
			float _Angle;
			float _Tolerance;
			float3 _TerrainSize;
			sampler2D _FalloffTex;

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
				float3 normal = normalFromHeightMap(_MainTex, _MainTex_TexelSize, _TerrainSize, input.localPos);
				float2 dir = normalize(normal.xz);
				float rad = atan2(dir.y, dir.x);
				float deg = (rad >= 0) * (rad * 57.2958) + (rad < 0) * (359 + rad * 57.2958);
				float minAngle = (_Angle - _Tolerance * 0.5);
				float maxAngle = (_Angle + _Tolerance * 0.5);

				float deg0 = (deg + 180) % 360;
				float minAngle0 = (minAngle + 180) % 360;
				float maxAngle0 = (maxAngle + 180) % 360;
				float v0 = deg0 > minAngle0 && deg0 <= maxAngle0;
				float f0 = (1 - abs(inverseLerp(deg0, minAngle0, maxAngle0) * 2 - 1)) * v0;

				float deg1 = (deg + 360) % 360;
				float minAngle1 = (minAngle + 360) % 360;
				float maxAngle1 = (maxAngle + 360) % 360;
				float v1 = deg1 > minAngle1 && deg1 <= maxAngle1;
				float f1 = (1 - abs(inverseLerp(deg1, minAngle1, maxAngle1) * 2 - 1)) * v1;

				float value = max(f0, f1);
				value = tex2D(_FalloffTex, value.xx).r * ((dir.x + dir.y) != 0);

				return value;
			}
			ENDCG

		}
	}
}
