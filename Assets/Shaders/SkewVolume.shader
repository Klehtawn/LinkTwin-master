Shader "Custom/Skew Volume"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_DetailTex ("Detail Texture", 2D) = "white" {}

		_SkewDirection("Skew Direction", Vector) = (0, 0, 1)
		_SkewBase("Skew Base", float) = 0
		_LightDir("Light Direction", Vector) = (0, -1, 0)
		_AmbientColor("Ambient Color", float) = 0.65

        _Color ("Tint", Color) = (1,1,1,1)
		_DetailPower("Detail power", Range(0,1)) = 1
		_Saturation("Saturation", Range(0,1)) = 1
		_Brightness("Brightness", Range(-1,1)) = 0
		_WriteStencil("Write stencil", Int) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Geometry" 
            "IgnoreProjector"="True" 
            "RenderType"="Opaque" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite On
		ZTest LEqual
        Fog { Mode Off }
        Blend Off

        Pass
        {
		Stencil {
                Ref [_WriteStencil]
                Comp Always
                Pass Replace
            }
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
				float3 normal   : NORMAL;
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
			float3 _SkewDirection;
			float3 _LightDir;
			float _SkewBase;
			float _AmbientColor;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

				float4 worldPos = mul(_Object2World, float4(IN.vertex.xyz, 1.0));
				worldPos.xyz += _SkewDirection * (worldPos.y - _SkewBase);

				float3 newVert = worldPos;
                OUT.vertex = mul(UNITY_MATRIX_VP, float4(newVert, 1.0));

                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);

				float3 nrm =  mul(_Object2World, float4(IN.normal, 0.0)); 
                OUT.color = IN.color * _Color * saturate(saturate(dot(normalize(_LightDir), normalize(nrm))) + _AmbientColor);
                return OUT;
            }

            sampler2D _MainTex;
			sampler2D _DetailTex;

			float _Saturation, _DetailPower, _Brightness;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

				fixed4 detail = tex2D(_DetailTex, IN.texcoord);
				c.rgb *= lerp(fixed3(1.0, 1.0, 1.0), detail.rgb, _DetailPower);

				fixed gray = dot(c.rgb, fixed3(0.299, 0.587, 0.114));

				c.rgb = lerp(fixed3(gray, gray, gray), c.rgb, _Saturation) + _Brightness;
                
                return c;
            }
        ENDCG
        }
    }
}