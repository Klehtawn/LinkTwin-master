Shader "Custom/Image Controls Opaque"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_Brightness("Brightness", Range(-1,1)) = 0
		_Contrast("Contrast", Range(-1,1)) = 0
		_Saturation("Saturation", Range(0,2)) = 1
		_RedGreenBalance("Red Green balance", Range(-1,1)) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Geometry" 
            "IgnoreProjector"="True" 
            "RenderType"="Geometry" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite On
		ZTest LEqual
        Fog { Mode Off }

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
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
			fixed _Brightness, _Contrast, _Saturation, _RedGreenBalance;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
				fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
				col.rgb = pow(col.rgb, lerp(1.0, 8.0, _Contrast));
				col.rgb += _Brightness;
				fixed gray = dot(col.rgb, fixed3(0.299, 0.587, 0.114));
				col.rgb = lerp(gray, col.rgb, _Saturation);
				col.r = lerp(col.r, 0.0, _RedGreenBalance * 0.2);
				col.g = lerp(col.g, 0.0, -_RedGreenBalance * 0.2);

                return col;
            }
        ENDCG
        }
    }
}