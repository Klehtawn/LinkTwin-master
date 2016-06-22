Shader "Custom/Blend Images"
{
    Properties
    {
        _MainTex ("First Image", 2D) = "white" {}
		_SecondTex ("Second Image", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_BlendFactor("Blend Factor", float) = 0


		[PerRendererData] _StencilComp ("Stencil Comparison", Float) = 8
		[PerRendererData] _Stencil ("Stencil ID", Float) = 0
		[PerRendererData] _StencilOp ("Stencil Operation", Float) = 0
		[PerRendererData] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[PerRendererData] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[PerRendererData] _ColorMask ("Color Mask", Float) = 15
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

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

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

			float4 _MainTex_ST; 

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;

                return OUT;
            }

            sampler2D _MainTex, _SecondTex;

			float _BlendFactor;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c1 = tex2D(_MainTex, IN.texcoord);
				fixed4 c2 = tex2D(_SecondTex, IN.texcoord);
                
                return lerp(c1, c2, _BlendFactor) * IN.color;
            }
        ENDCG
        }
    }
}