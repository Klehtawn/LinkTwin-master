Shader "Custom/Invisible" {
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_ColorMask ("Color ref", Color) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
	}
	SubShader
	{
		Pass
		{
			ZWrite Off
			ColorMask 0  

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
   
			struct v2f {
				float4 pos : SV_POSITION;
			};
     
			v2f vert ()
			{
				v2f o;
				o.pos = fixed4(0,0,0,0);
				return o;
			}
 
			fixed4 frag (v2f i) : COLOR0 { return fixed4(0,0,0,0); }
			ENDCG
		}
	}
}