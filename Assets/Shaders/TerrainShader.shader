Shader "Custom/Terrain" {
	Properties{

		_TexArr("Textures", 2DArray) = "" {}

		_WallTex("Wall Texture", 2D) = "white" {}
		_TexScale("Texture Scale", Float) = 1

	}

	SubShader{

		Tags { "RenderType" = "Opaque" } 
		LOD 200 

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows // Use Unity's standard lighting model
		#pragma target 3.5 
		#pragma require 2darray

		sampler2D _WallTex;
		UNITY_DECLARE_TEX2DARRAY(_TexArr);
		float _TexScale;

		struct Input {

			float3 worldPos;
			float3 worldNormal;
			float2 uv_TexArr;

		};

		void surf(Input IN, inout SurfaceOutputStandard o) {

			float3 scaledWorldPos = IN.worldPos / _TexScale; 
			float3 pWeight = abs(IN.worldNormal); 
			pWeight /= (pWeight.x + pWeight.y + pWeight.z); 
      //pWeight.x = floor(pWeight.x);
      pWeight.y = floor(pWeight.y);
      //pWeight.z = floor(pWeight.z);

      /*
      float3 pWeight;
      pWeight.x = 0; 
      pWeight.y = 0;
      pWeight.z = 0;
      */

			int texIndex = floor(IN.uv_TexArr.x + 0.1);
			//int texIndex = 1;
			float3 projected;

			//projected = float3(scaledWorldPos.y, scaledWorldPos.z, texIndex);
			//float3 xP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;
			float3 xP = tex2D(_WallTex, scaledWorldPos.yz) * pWeight.x;


			projected = float3(scaledWorldPos.x, scaledWorldPos.z, texIndex);
			float3 yP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

			//projected = float3(scaledWorldPos.x, scaledWorldPos.y, texIndex);
			//float3 zP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;
			float3 zP = tex2D(_WallTex, scaledWorldPos.xy) * pWeight.z;


			o.Albedo = xP + yP + zP;
		}
		ENDCG
	}
	FallBack "Diffuse"
}