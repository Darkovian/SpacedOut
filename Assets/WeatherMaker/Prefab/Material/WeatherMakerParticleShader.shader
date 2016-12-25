//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

Shader "WeatherMaker/WeatherMakerParticleShader"
{
    Properties
	{
		_MainTex ("Color (RGB) Alpha (A)", 2D) = "gray" {}
		_TintColor ("Tint Color (RGB)", Color) = (1, 1, 1, 1)
		_PointSpotLightMultiplier ("Point/Spot Light Multiplier", Range (0, 10)) = 2
		_DirectionalLightMultiplier ("Directional Light Multiplier", Range (0, 10)) = 1
		_InvFade ("Soft Particles Factor", Range(0.01, 3.0)) = 1.0
		_AmbientLightMultiplier ("Ambient light multiplier", Range(0, 4)) = 1
    }

    SubShader
	{
        Tags { "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		LOD 100

        Pass
		{
			ZWrite Off
			Cull Back
            Lighting On     
			AlphaTest Greater 0.01
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
						 
            CGPROGRAM

			#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_particles
			#pragma multi_compile __ ORTHOGRAPHIC_MODE
			#pragma multi_compile __ PER_PIXEL_LIGHTING

            #include "UnityCG.cginc"
			#include "WeatherMakerShader.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

		    struct v2f
            {
                half2 uv_MainTex : TEXCOORD0;
                fixed4 color : COLOR0;
                float4 pos : SV_POSITION;

#if defined(PER_PIXEL_LIGHTING)

				float3 viewPos : TEXCOORD1;

#endif

#if defined(SOFTPARTICLES_ON)

                float4 projPos : TEXCOORD2;

#endif

            };
 
            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);


#if defined(PER_PIXEL_LIGHTING)

				o.viewPos = mul(UNITY_MATRIX_MV, v.vertex);
				o.color = v.color * _TintColor;

#else

				o.color = CalculateVertexColor(mul(UNITY_MATRIX_MV, v.vertex).xyz) * v.color * _TintColor;

#endif

				// o.color = v.color * _TintColor; // temp if you want to disable lighting

#if defined(SOFTPARTICLES_ON)

                o.projPos = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);

#endif

                return o; 
            }
			
            fixed4 frag (v2f i) : COLOR
			{       

#if defined(PER_PIXEL_LIGHTING)

				fixed4 color = tex2D(_MainTex, i.uv_MainTex) * i.color * CalculateVertexColor(i.viewPos);

#else

				fixed4 color = tex2D(_MainTex, i.uv_MainTex) * i.color;

#endif

#if defined(SOFTPARTICLES_ON)

				float sceneZ = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				color.a *= saturate(_InvFade * (sceneZ - partZ));

#endif

				return color;
            }

            ENDCG
        }
    }
 
    Fallback "Particles/Alpha Blended"
}