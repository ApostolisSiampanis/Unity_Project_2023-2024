Shader "Hidden/Vista/Graph/SlopeMask"
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
			float _MinAngle;
			float _MaxAngle;
			sampler2D _Transition;
			float3 _TerrainSize;

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
				float value = tex2D(_MainTex, input.localPos).r;
				float blendSlopeValue = 1;
				float3 normal = normalFromHeightMap(_MainTex, _MainTex_TexelSize, _TerrainSize, input.localPos);
				float cosine = abs(normal.y);
				float slopeAngle = acos(cosine);
				float slopeTransitionFactor = (slopeAngle - _MinAngle) / (_MaxAngle - _MinAngle);
				float slopeTransition = tex2D(_Transition, float2(slopeTransitionFactor, 0.5f)).r;
				blendSlopeValue = (slopeAngle >= _MinAngle) * (slopeAngle <= _MaxAngle) * slopeTransition;

				return blendSlopeValue;
			}
			ENDCG

		}
	}
}
