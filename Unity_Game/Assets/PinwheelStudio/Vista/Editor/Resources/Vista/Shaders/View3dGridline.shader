Shader "Hidden/Vista/TerrainGraphEditor/View3dGridline"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100
		ZWrite Off
		ZTest LEqual
		Blend Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				float4 color: COLOR;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				float4 color: COLOR;
			};

			float4 _Origin;
			float4 _Offset; //(origin offset, endpoint offset)

			v2f vert(uint id: SV_VERTEXID)
			{
				int i = id / 2;
				float2 xz = _Origin + i * _Offset.xy;
				if (id % 2 != 0)
				{
					xz += _Offset.zw;
				}
				float4 vertex = float4(xz.x, 0, xz.y, 1);
				float4 color = float4(0.1, 0.1, 0.1, 1);

				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.color = color;
				return o;
			}

			fixed4 frag(v2f i): SV_Target
			{
				return i.color;
			}
			ENDCG

		}
	}
}
