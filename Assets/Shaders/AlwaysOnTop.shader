Shader "Zee/AlwaysOnTop"
{
	Properties{
		_Color("Color", Color) = (0,0,0,0)
	}

		SubShader
	{
		Tags { "Queue" = "Overlay" "RenderType" = "Overlay" }
		ZTest Always

		CGPROGRAM
		#pragma surface surf NoLighting noambient

		fixed4 _Color;

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
			return fixed4(s.Albedo, s.Alpha);
		}

		struct Input
		{
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _Color;
		}
		ENDCG
	}
		Fallback "Diffuse"
}