Shader "Custom/Falling Triangles"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_Saturation("Saturation", Range(0,1)) = 1
		_FallingDirection ("Falling direction", Vector) = (0, -1, 0)
		_AnimationTime ("Animation Time", Float) = 0
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
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
				float4 normal   : NORMAL;
                float4 color    : COLOR; // x = speed
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

			float3	_FallingDirection;
			float _AnimationTime;

			float4x4 rotationMat(float3 axis, float angle)
			{
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;
    
				float4x4 mat = float4x4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
										oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
										oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
										0.0,                                0.0,                                0.0,                                1.0);
				return mat;
			}

			float4x4 rotationX(float angle)
			{
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;
    
				float4x4 mat = float4x4(oc + c,		0.0,	0.0,	0.0,
										0.0,		c,		-s,		0.0,
										0.0,		s,		c,		0.0,
										0.0,		0.0,	0.0,	1.0);
				return mat;
			}

			float4x4 rotationZ(float angle)
			{
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;
    
				float4x4 mat = float4x4(c,		-s,		0.0,		0.0,
										s,		c,      0.0,		0.0,
										0.0,	0.0,	oc + c,     0.0,
										0.0,	0.0,	0.0,        1.0);
				return mat;
			}
			
            v2f vert(appdata_t IN)
            {
                v2f OUT;

				float4 p = IN.vertex;

				p.xyz -= IN.normal;

				float spd = IN.color.x;

				float3 fall = _FallingDirection * _AnimationTime * spd;
				float angX = IN.color.g * _AnimationTime * 5.0 * spd * 0.1f;
				float angZ = IN.color.b * _AnimationTime * 5.0 * spd * 0.1f;

				float scale = min(1.0, length(fall) / 10.0);
				p.xyz *= 1.0 - scale;

				float4x4 rot = mul(rotationX(angX), rotationZ(angZ));
				p = mul(rot, p);

				p.xyz += IN.normal;

				p = mul(_Object2World, p);

				p.xyz += fall;

                OUT.vertex = mul(UNITY_MATRIX_VP, p);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = _Color;
				OUT.color.a *= 1.0 - scale;

                return OUT;
            }

            sampler2D _MainTex;

			float _Saturation;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

				fixed gray = dot(c.rgb, fixed3(0.299, 0.587, 0.114));

				c.rgb = lerp(fixed3(gray, gray, gray), c.rgb, _Saturation);
                
                return c;
            }
        ENDCG
        }
    }
}