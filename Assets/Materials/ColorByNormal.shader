Shader "Custom/ColorByNormal" { 
   SubShader { 
      Pass { 
         CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		 #pragma vertex vert  
         #pragma fragment frag 

		#include "UnityCG.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct vertexInput{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 col : TEXCOORD0;
         };

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            output.pos =  UnityObjectToClipPos(input.vertex);
            output.col = float4(input.normal, 1.0) + float4(0.5, 0.5, 0.5, 0.0);
            return output;
         }
		 float4 frag(vertexOutput input) : COLOR // fragment shader
         {
            return input.col; 
         }
		ENDCG 
      }
   }
}