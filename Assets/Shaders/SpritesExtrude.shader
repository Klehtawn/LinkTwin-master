Shader "Sprites/Default Extruded"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_Extrusion ("Extrusion", float) = 10.0
		_ExtrusionDir ("Extrusion direction", Vector) = (0, 0, 1)
		_MapCenter ("Map Center", Vector) = (0, 0, 0)
    }

    SubShader
    {
        Tags
        { 
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
            "IgnoreProjector"="True" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Back
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend Off //SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		AlphaTest Greater 0.5

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"

			float _Extrusion;
			float4 _ExtrusionDir;

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

			float3 _MapCenter;

			float4 _MainTex_ST;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

				float4 worldPos = mul(_Object2World, float4(IN.vertex.xyz, 1.0));

				float3 newVert = worldPos + _ExtrusionDir * IN.vertex.y * _Extrusion;
				newVert.y *= 0.0;

                OUT.vertex = mul(UNITY_MATRIX_VP, float4(newVert, 1.0));

                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;

                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
				fixed alpha = tex2D(_MainTex, IN.texcoord).a;
				if(alpha < 0.5)
					discard;
                fixed4 c = IN.color;
				c.a = alpha;
                return c;
            }
        ENDCG
        }
    }
}