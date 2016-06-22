Shader "Custom/Player Shader Test 1"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
		_SecondTex ("Second Texture", 2D) = "white" {}
		_DissolveMask ("Dissolve Mask", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_Saturation("Saturation", Range(0,1)) = 1
		_DissolveFactor("Dissolve Factor", Range(0,1)) = 0
		_BlendTexturesFactor("Blend Textures Factor", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
		ZTest LEqual
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;

			float4 _MainTex_ST;

			uniform float	_DissolveTexScale;
			uniform float	_BlendTexturesFactor;


            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;

                return OUT;
            }

            sampler2D _MainTex;
			sampler2D _SecondTex;
			sampler2D _DissolveMask;

			float _Saturation;
			fixed _DissolveFactor;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c1 = tex2D(_MainTex, IN.texcoord);
				fixed4 c2 = tex2D(_SecondTex, IN.texcoord);

				fixed4 c = lerp(c1, c2, _BlendTexturesFactor) * IN.color;

				fixed4 dissolve = tex2D(_DissolveMask, IN.texcoord);

				fixed gray = dot(c.rgb, fixed3(0.299, 0.587, 0.114));

				c.rgb = lerp(fixed3(gray, gray, gray), c.rgb, _Saturation);

				c.a = step(1.0 - dissolve.r, 1.0 - _DissolveFactor) * c.a;
                
                return c;
            }
        ENDCG
        }
    }
}
