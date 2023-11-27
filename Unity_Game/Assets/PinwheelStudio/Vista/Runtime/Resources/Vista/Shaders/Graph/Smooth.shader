Shader "Hidden/Vista/Graph/Smooth"
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
			sampler2D _MaskMap;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.localPos = v.vertex;
				return o;
			}

			float getWeight(float x)
			{
				float standardDeviation = 0.3;
				float sqrSD = standardDeviation * standardDeviation;
				return exp(-x * x / (2 * sqrSD));
			}

			float blur(float2 uvPos, int radius)
			{
				float2 texelSize = _MainTex_TexelSize.xy;
				float2 textureRes = _MainTex_TexelSize.zw;
				float avgColor = 0;
				float sampleCount = 0;
				float2 uv = float2(0, 0);
				float2 centerPixel = uvPos * textureRes;
				float2 pixelPos = float2(0, 0);
				float d = 0;
				float f = 0;
				float weight = 0;
				for (int x0 = -radius; x0 <= radius; ++x0)
				{
					for (int y0 = -radius; y0 <= radius; ++y0)
					{
						uv = uvPos + float2(x0 * texelSize.x, y0 * texelSize.y);
						pixelPos = uv * textureRes;
						d = distance(centerPixel, pixelPos);
						f = saturate(d / radius);
						weight = getWeight(f);
						avgColor += tex2D(_MainTex, uv).r * weight;
						sampleCount += weight;
					}
				}
				avgColor = avgColor / sampleCount;
				return avgColor;
			}

			float frag(v2f input): SV_Target
			{
				float currentValue = tex2D(_MainTex, input.localPos).r;
				float blurValue = blur(input.localPos, 2);
				float maskValue = saturate(tex2D(_MaskMap, input.localPos).r);
				float value = lerp(currentValue, blurValue, maskValue);
				return value;
			}
			ENDCG

		}
	}
}
