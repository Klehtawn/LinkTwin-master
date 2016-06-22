Shader "Unlit/Overlay"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
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
                fixed4 diffuse = tex2D(_MainTex, IN.texcoord);
                fixed luminance =  dot(diffuse, fixed4(0.2126, 0.7152, 0.0722, 0));
                fixed oldAlpha = diffuse.a;

				fixed4 diffuse1 = diffuse * 2.0 * _Color;
				fixed4 diffuse2 = 1.0-2.0*(1.0-diffuse)*(1.0-_Color);

				diffuse = lerp(diffuse1, diffuse2, step(luminance, 0.5));
  
                diffuse.a  = oldAlpha * _Color.a;


                return diffuse;
            }
        ENDCG
        }
    }
}