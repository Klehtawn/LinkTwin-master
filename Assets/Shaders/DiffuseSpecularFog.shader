Shader "Custom/DiffuseSpecularFog" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_FogStart ("Fog start", Range(0,1)) = 0.0
		_FogEnd ("Fog end", Range(0,1)) = 1.
		_FogColor("Fog color", Color) = (0,0,0,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		//#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _FogStart, _FogEnd;
		fixed4 _FogColor;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			#if (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)) && defined(SHADER_API_MOBILE)
				float depth = (IN.screenPos.z + 1.0f) * 0.5f;                             
			#else
				float depth = IN.screenPos.z;             
			#endif

			half fogFactor = clamp((depth - _FogStart) / (_FogEnd - _FogStart), 0.0, 1.0);
			o.Albedo = lerp(c.rgb, _FogColor.rgb, fogFactor);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
