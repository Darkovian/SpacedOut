//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// Resources:
// http://library.nd.edu/documents/arch-lib/Unity/SB/Assets/SampleScenes/Shaders/Skybox-Procedural.shader
//

Shader "WeatherMaker/WeatherMakerSkySphereShader"
{
	Properties
	{
		_MainTex ("Day Texture", 2D) = "blue" {}
		_DawnDuskTex ("Dawn/Dusk Texture", 2D) = "orange" {}
		_NightTex ("Night Texture", 2D) = "black" {}
		_DayMultiplier ("Day Multiplier", Range(0, 3)) = 1
		_DawnDuskMultiplier ("Dawn/Dusk Multiplier", Range(0, 1)) = 0
		_NightMultiplier ("Night Multiplier", Range(0, 3)) = 0
		_NightVisibilityThreshold ("Night Visibility Threshold", Range(0, 1)) = 0
		_SunNormal ("Sun Normal pointing to 0,0,0", Vector) = (0, 0, 0, 0)
		_SunColor ("Sun Color", Color) = (1, 1, 1, 1)
		_SunSize ("Sun Size", Range(0.01, 10)) = 0.02
		_SkyTintColor ("Sky tint color, procedural only", Color) = (0.5, 0.5, 0.5, 1)
		_SkyAtmosphereThickness ("Sky atmosphere thickness, procedural only", Range(0, 5)) = 1
		_GroundTintColor("Ground tint color, procedural only", Color) = (0.4, 0.4, 0.4)
	}
	SubShader
	{
		Tags{ "RenderType" = "Background" "IgnoreProjector" = "True" "Queue" = "Background" }
		Cull Off Lighting Off ZWrite Off

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
			#pragma multi_compile __ ENABLE_SUN_HIGH_QUALITY ENABLE_SUN_FAST
			#pragma multi_compile __ ENABLE_PROCEDURAL_TEXTURED_SKY
			#pragma multi_compile __ ENABLE_PROCEDURAL_SKY
			
			#include "WeatherMakerShader.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

#if defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST) || defined(ENABLE_PROCEDURAL_TEXTURED_SKY) || defined(ENABLE_PROCEDURAL_SKY)

				float3 ray : NORMAL;

#endif

#if defined(ENABLE_PROCEDURAL_TEXTURED_SKY) || defined(ENABLE_PROCEDURAL_SKY)

				fixed4 vertexColor : COLOR0;

#endif

#if defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

				fixed4 sunColor : TEXCOORD1;

#endif

			};

			sampler2D _DawnDuskTex;
			float4 _DawnDuskTex_ST;
			sampler2D _NightTex;
			float4 _NightTex_ST;
			fixed _DayMultiplier;
			fixed _DawnDuskMultiplier;
			fixed _NightMultiplier;
			fixed _NightVisibilityThreshold;
			float3 _SunNormal;
			fixed3 _SunColor;
			fixed _SunSize;
			fixed3 _SkyTintColor;
			float _SkyAtmosphereThickness;
			fixed3 _GroundTintColor;

			fixed4 GetSunColor(v2f i)
			{

#if defined(ENABLE_SUN_HIGH_QUALITY)

				return GetSunColorHighQuality(_SunNormal, i.sunColor, _SunSize, i.ray);

#elif defined(ENABLE_SUN_FAST)

				return GetSunColorFast(_SunNormal, i.sunColor, _SunSize, i.ray);

#else

				return fixed4(0, 0, 0, 0);

#endif

			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 0));
				//o.vertex.z = o.vertex.w;
				o.uv = v.uv; // TRANSFORM_TEX not supported

#if defined(ENABLE_PROCEDURAL_TEXTURED_SKY) || defined(ENABLE_PROCEDURAL_SKY)

				procedural_sky_vertex psv = CalculateSkyVertex(_SunNormal, _SunColor, _GroundTintColor, v.vertex.xyz, _SkyTintColor, _SkyAtmosphereThickness);
				o.ray = psv.ray;
				o.vertexColor = psv.vertexColor;

#if defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

				o.sunColor = psv.sunColor;

#endif

#elif defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

				o.ray = v.normal;
				o.sunColor = fixed4(_SunColor, GetSunLightSkyMultiplier(-_SunNormal, v.vertex.xyz));

#endif

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 result;

#if defined(ENABLE_SUN_HIGH_QUALITY) || defined(ENABLE_SUN_FAST)

				fixed sunMultiplier = i.sunColor.w;

#else

				fixed sunMultiplier = 1;

#endif

#if defined(ENABLE_PROCEDURAL_TEXTURED_SKY)

				fixed4 skyColor = i.vertexColor + GetSunColor(i);
				skyColor.a = 1.0 - _NightMultiplier;
				fixed4 dayColor = tex2D(_MainTex, i.uv) * _DayMultiplier;
				dayColor.rgb *= sunMultiplier;
				fixed4 dawnDuskColor = tex2D(_DawnDuskTex, i.uv);
				fixed4 dawnDuskColor2 = dawnDuskColor * _DawnDuskMultiplier;
				dawnDuskColor2.rgb *= sunMultiplier;
				dayColor += dawnDuskColor2;
				fixed4 nightColor = (tex2D(_NightTex, i.uv) * _NightMultiplier);
				nightColor *= ceil(max(nightColor.r, max(nightColor.g, nightColor.b)) - _NightVisibilityThreshold);

				// hide night texture wherever dawn/dusk is opaque
				nightColor.rgb *= (1.0 - dawnDuskColor.a);

				// blend texture on top of sky
				result = ((dayColor * dayColor.a) + (skyColor * (1.0 - dayColor.a)));

				// blend previous result on top of night
				result = ((result * result.a) + (nightColor * (1.0 - result.a)));

#elif defined(ENABLE_PROCEDURAL_SKY)

				result = (i.vertexColor * sunMultiplier) + GetSunColor(i) + (tex2D(_NightTex, i.uv) * _NightMultiplier);

#else

				fixed4 dayColor = tex2D(_MainTex, i.uv) * sunMultiplier * _DayMultiplier;
				fixed4 dawnDuskColor = (tex2D(_DawnDuskTex, i.uv) * _DawnDuskMultiplier);
				fixed4 nightColor = (tex2D(_NightTex, i.uv) * _NightMultiplier);
				nightColor *= ceil(max(nightColor.r, max(nightColor.g, nightColor.b)) - _NightVisibilityThreshold);
				result = (dayColor + dawnDuskColor + nightColor) + GetSunColor(i);

#endif

				return result;
			}

			ENDCG
		}
	}
}
