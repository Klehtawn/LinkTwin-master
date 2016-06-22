Shader "Custom/Shattered Mesh"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_DetailTex ("Detail Texture", 2D) = "white" {}
		_DissolveTex ("Dissolve Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)

		_DetailPower("Detail power", Range(0,1)) = 1
		_Brightness("Brightness", Range(-1,1)) = 0

		_LightDirAndAmbient("Light Direction And Ambient", Vector) = (0, -1, 0, 0.65)

		_AnimationTime ("Animation Time", Range(0,1)) = 0
		_ExplodeFactor("Explode Factor", float) = 1

		_Wind("Wind", Vector) = (0, 0, 0)

		_SkewDirectionAndBase("Skew Direction And Base", Vector) = (0, 0, 1, 0)
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
        ZWrite On
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
				float3 normal   : NORMAL;
				float4 tangent	: TANGENT;
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

			float _AnimationTime;
			float _ExplodeFactor;

			float4 _LightDirAndAmbient;
			float3 _Wind;

			float4 _SkewDirectionAndBase;

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

				p.xyz -=  IN.tangent.xyz;

				float spd = IN.color.x;

				float3 _FallingDirection = normalize(IN.tangent.xyz) + _Wind;

				float3 fall = _FallingDirection * _AnimationTime * spd * _ExplodeFactor;
				float angX = IN.color.g * _AnimationTime;
				float angZ = IN.color.b * _AnimationTime;

				float4x4 rot = mul(rotationX(angX), rotationZ(angZ));
				p.xyz = mul(rot, p).xyz;

				float scale = lerp(1.0, 0.0001, _AnimationTime * _AnimationTime);
				p.xyz *= scale;

				p.xyz += IN.tangent;
				p.xyz += fall;

				p = mul(_Object2World, p);

				p.xyz += _SkewDirectionAndBase.xyz * (p.y - _SkewDirectionAndBase.w);

                OUT.vertex = mul(UNITY_MATRIX_VP, p);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				
				float3 nrm =  normalize(mul(rot, float4(IN.normal, 0.0))); 
				nrm =  mul(_Object2World, nrm); 
                OUT.color = _Color * saturate(saturate(dot(normalize(_LightDirAndAmbient.xyz), normalize(nrm))) + _LightDirAndAmbient.w);

                return OUT;
            }

            sampler2D _MainTex;
			sampler2D _DetailTex;
			sampler2D _DissolveTex;

			float _DetailPower, _Brightness;

            fixed4 frag(v2f IN) : SV_Target
            {
				fixed4 dissolve = tex2D(_DissolveTex, IN.texcoord);
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				fixed4 detail = tex2D(_DetailTex, IN.texcoord);
				c.rgb *= lerp(fixed3(1.0, 1.0, 1.0), detail.rgb, _DetailPower);
				c.rgb += _Brightness;

				c.a = step(1.0 - dissolve.r, 1.0 - _AnimationTime);// * c.a;
				//c.a = saturate(dissolve.r + _AnimationTime);
				c.a = 1.0;

                return c;
            }
        ENDCG
        }
    }
}