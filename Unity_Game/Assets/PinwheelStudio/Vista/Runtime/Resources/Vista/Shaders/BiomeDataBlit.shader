Shader "Hidden/Vista/BiomeDataBlit"
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
			#include "Includes/Math.hlsl"
			#include "Includes/Sampling.hlsl"

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

			sampler2D _MainTex; //texture to copy
			float4 _MainTex_TexelSize;

			float2 _RenderTargetSize; //size of the canvas texture
			float4 _SrcBounds;
			float4 _DestBounds;

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
				float2 destUV = correctUV(input.uv, _RenderTargetSize);
				float2 destMin = float2(_DestBounds.x, _DestBounds.y);
				float2 destMax = float2(_DestBounds.x + _DestBounds.z, _DestBounds.y + _DestBounds.w);
				float2 destPos = float2
				(
					lerp(destMin.x, destMax.x, destUV.x),
					lerp(destMin.y, destMax.y, destUV.y)
				);

				float2 srcMin = float2(_SrcBounds.x, _SrcBounds.y);
				float2 srcMax = float2(_SrcBounds.x + _SrcBounds.z, _SrcBounds.y + _SrcBounds.w);
				float2 srcUV = float2
				(
					(destPos.x - srcMin.x) / (srcMax.x - srcMin.x),
					(destPos.y - srcMin.y) / (srcMax.y - srcMin.y)
				);

				if (srcUV.x < 0 || srcUV.x > 1 || srcUV.y < 0 || srcUV.y > 1)
				{
					discard;
				}
				float4 data = tex2D(_MainTex, srcUV);
				return data;
			}
			ENDCG

		}
	}
}
