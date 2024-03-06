Shader "Hidden/Vista/Graph/AngularWarp"
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

			sampler2D _MainTex;
			sampler2D _DirectionMap;
			sampler2D _IntensityMap;
			float _MinAngle;
			float _MaxAngle;
			float _DirectionMultiplier;
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
				float fAngle = _DirectionMultiplier * tex2D(_DirectionMap, input.localPos).r;
				float angle = lerp(_MinAngle, _MaxAngle, fAngle);
				float fIntensity = _IntensityMultiplier * tex2D(_IntensityMap, input.localPos).r;
				float2 offset = float2(cos(angle), sin(angle)) * fIntensity;
				float2 uv = input.localPos + offset;
				float value = tex2D(_MainTex, uv).r;
				return value;
			}
			ENDCG

		}
	}
}
