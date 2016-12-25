//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

Shader "WeatherMaker/WeatherMakerCloudDomeShader"
{
	Properties
	{
		_MainTex("Color (RGB) Alpha (A)", 2D) = "gray" {}
		_MaskTex("Mask Texture (RGBA)", 2D) = "white" {}
		_TintColor("Tint Color (RGB)", Color) = (1, 1, 1, 1)
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 2
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_EmissiveColor("Emissive Color (RGB)", Color) = (0.1, 0.1, 0.1, 1)
		_AmbientLightMultiplier("Ambient light multiplier", Range(0, 2)) = 0.1
		_BillboardOffset("Billboard Multiplier (float)", float) = -200
		_FadeOffset("Fade Offset (float)", float) = 5000
		_InvFade("Soft Particles Factor", Range(0.01, 3.0)) = 0.25
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			Cull Off
			Lighting On
			ZWrite Off

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			// #pragma multi_compile_particles
			#pragma multi_compile __ PER_PIXEL_LIGHTING
			// #pragma enable_d3d11_debug_symbols

			#include "WeatherMakerShader.cginc"

			float _BillboardOffset;
			float _FadeOffset;

			float4 _MaskTex_ST;
			sampler2D _MaskTex;

			struct appdata_t
			{
				// center position
				float4 vertex : POSITION;

				// xy: texcoord, zw = fade texcoord
				float4 texcoord : TEXCOORD0;

				// color
				fixed4 color : COLOR;

				// lifetime, 
				float4 lifeTime : TEXCOORD1;

				// velocity
				float3 velocity : NORMAL;

				// other (x = start angle, y = angular velocity, z = billboard width, w = billboard height)
				float4 other : TANGENT;
			};

			struct v2f
			{
				half2 uv_MainTex : TEXCOORD0;
				half2 uv_MaskTex : TEXCOORD1;
				fixed4 color : COLOR0;
				float4 pos : SV_POSITION;

#if defined(PER_PIXEL_LIGHTING)

				float3 viewPos : TEXCOORD2;

#endif

#if defined(SOFTPARTICLES_ON)

				float4 projPos : TEXCOORD3;

#endif

			};

			v2f vert(appdata_t v)
			{
				v2f o;

				float elapsedSeconds = _Time.y - v.lifeTime.x;
				float4 movement = float4(v.velocity * elapsedSeconds, 0);
				float4 pos = mul(unity_ObjectToWorld, v.vertex) + movement;
				float4 offset = float4(v.other.z, v.other.w, 0, 0);

				// billboard such that the quad is always facing down at the camera, but more at an angle as the clouds get further sideways away from the camera
				float3 zaxis = _WorldSpaceCameraPos - pos.xyz;
				zaxis.y += _BillboardOffset;
				zaxis = normalize(zaxis);
				float3 xaxis = normalize(cross(float3(1, 0, 0), zaxis));
				float3 yaxis = cross(zaxis, xaxis);
				float3x3 rotationMatrix =
				{
					xaxis.x, yaxis.x, zaxis.x,
					xaxis.y, yaxis.y, zaxis.y,
					xaxis.z, yaxis.z, zaxis.z,
				};
				offset.xyz = mul(rotationMatrix, offset.xyz);
				offset.xyz = RotateVertexLocalQuaternion(offset.xyz, zaxis, v.other.x + (v.other.y * elapsedSeconds));
				pos += mul(unity_ObjectToWorld, offset);
				o.pos = mul(UNITY_MATRIX_MVP, mul(unity_WorldToObject, pos));
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				o.uv_MaskTex = TRANSFORM_TEX(v.texcoord.zw, _MaskTex);

#if defined(PER_PIXEL_LIGHTING)

				o.viewPos = mul(UNITY_MATRIX_MV, v.vertex);
				o.color = v.color * _TintColor;

#else

				o.color = CalculateVertexColor(mul(UNITY_MATRIX_MV, pos).xyz) * v.color * _TintColor;

#endif

				o.color.a = ((o.color.a * (_FadeOffset / distance(_WorldSpaceCameraPos, pos.xyz)) * abs(zaxis.y + zaxis.y)));
				o.color.a *= LerpFade(v.lifeTime, _Time.y);

#if defined(SOFTPARTICLES_ON)

				o.projPos = ComputeScreenPos(pos);
				COMPUTE_EYEDEPTH(o.projPos.z);

#endif

				return o;
			}

			fixed4 frag(v2f f) : COLOR
			{

#if defined(SOFTPARTICLES_ON)

				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(f.projPos)));
				float partZ = f.projPos.z;
				float fade = saturate(_InvFade * (sceneZ - partZ));
				f.color.a *= fade;

#endif

				return tex2D(_MainTex, f.uv_MainTex) * tex2D(_MaskTex, f.uv_MaskTex) * f.color

#if defined(PER_PIXEL_LIGHTING)

				* CalculateVertexColor(f.viewPos);

#endif

				;
			}

			ENDCG
		}
	}

	Fallback "Particles/VertexLit"
}