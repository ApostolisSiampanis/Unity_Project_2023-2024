Shader "Hidden/Vista/Graph/DirectionalWarp"
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
			sampler2D _DirectionMap;
			float4 _DirectionMap_TexelSize;
			sampler2D _IntensityMap;
			float _IntensityMultiplier;

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
				float3 normal = normalFromHeightMap(_DirectionMap, _DirectionMap_TexelSize, float3(1, 1, 1), input.localPos);
				float fIntensity = _IntensityMultiplier * tex2D(_IntensityMap, input.localPos).r;
				float2 uv = input.localPos - normal.xz * fIntensity;
				float value = tex2D(_MainTex, uv).r;
				return value;
			}
			ENDCG

		}
	}
}
