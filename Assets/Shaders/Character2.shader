Shader "Zee/Character2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("_NormalMap", 2D) = "black" {}
		_EmissionMap("Emission", 2D) = "black" {}
		_ORM("Occlusion Roughness Metalic", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderQueue" = "AlphaTest" "RenderType" = "TransparentCutout" }
        LOD 200
		//AlphaToMask On
		Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _EmissionMap;
		sampler2D _ORM;

        struct Input
        {
            float2 uv_MainTex;
        };

        float4 _Color;
		//float4 _MainTex_TexelSize;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		float CalcMipLevel(float2 texture_coord)
		{
			float2 dx = ddx(texture_coord);
			float2 dy = ddy(texture_coord);
			float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));

			return max(0.0, 0.5 * log2(delta_max_sqr));
		}


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 e = tex2D(_EmissionMap, IN.uv_MainTex);
			fixed4 orm = tex2D(_ORM, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Metallic = orm.b;
			o.Smoothness = 1-orm.g;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

			// rescale alpha by mip level (if not using preserved coverage mip maps)
			//c.a *= 1 + max(0, CalcMipLevel(IN.uv_MainTex * _MainTex_TexelSize.zw));// *_MipScale;
			// rescale alpha by partial derivative
			//c.a = (c.a - 0.5) / max(fwidth(c.a), 0.0001) + 0.5;


			if (c.a < 0.5)
				// alpha value less than user-specified threshold?
			{
				discard; // yes: discard this fragment
			}



            o.Alpha = c.a;
			o.Occlusion = orm.r;
			o.Emission = e;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
