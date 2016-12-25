//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

#include "UnityCG.cginc"
#include "Lighting.cginc"

// constants
#define MIE_G (-0.990)
#define MIE_G2 0.9801
#define SKY_GROUND_THRESHOLD 0.02

#ifndef SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
#if defined(SHADER_API_MOBILE)
#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 1
#else
#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 0
#endif
#endif

#if defined(UNITY_COLORSPACE_GAMMA)
#define GAMMA 2
#define COLOR_2_GAMMA(color) color
#define COLOR_2_LINEAR(color) color * color
#define LINEAR_2_OUTPUT(color) sqrt(color)
#else
#define GAMMA 2.2
// HACK: to get gfx-tests in Gamma mode to agree until UNITY_ACTIVE_COLORSPACE_IS_GAMMA is working properly
#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
#define COLOR_2_LINEAR(color) color
#define LINEAR_2_LINEAR(color) color
#endif

struct procedural_sky_vertex
{
	float3 ray : NORMAL;
	fixed4 vertexColor : COLOR0;
	fixed4 sunColor : COLOR1;
};

fixed4 _TintColor;
fixed3 _EmissiveColor;
float _DirectionalLightMultiplier;
float _PointSpotLightMultiplier;
float _AmbientLightMultiplier;

sampler2D _MainTex;
float4 _MainTex_ST;

#if defined(SOFTPARTICLES_ON)

float _InvFade;
sampler2D _CameraDepthTexture;

#endif

inline fixed LerpFade(float4 lifeTime, float t)
{
	// the vertex will fade in, stay at full color, then fade out
	// x = creation time seconds
	// y = fade time in seconds
	// z = life time seconds

	// debug
	// return 1;

	float peakFadeIn = lifeTime.x + lifeTime.y;
	if (t < peakFadeIn)
	{
		return lerp(0, 1, max(0, ((t - lifeTime.x) / (peakFadeIn - lifeTime.x))));
	}
	float endLifetime = (lifeTime.x + lifeTime.z);
	float startFadeOut = endLifetime - lifeTime.y;

	// will be negative until fade out time (startFadeOut) is passed which will keep it at full color
	return lerp(1, 0, max(0, ((t - startFadeOut) / (endLifetime - startFadeOut))));
}

fixed3 ApplyLight(float4 lightPos, float4 lightDir, fixed3 lightColor, half4 lightAtten, float3 viewPos)
{
	float3 toLight = (lightPos.xyz - (viewPos * lightPos.w));
	float lengthSq = dot(toLight, toLight);
	float atten = 1.0 / (1.0 + (lengthSq * lightAtten.z));
	toLight = normalize(toLight);

#if defined(ORTHOGRAPHIC_MODE)

	float3 normal = fixed3(0, 0, 1);
	float diff = max(lightPos.w, dot(normal, toLight));
	return lightColor.rgb * (diff * atten);

#else

	// calculate modifier for non-directional light, will be 0,0,0 for directional lights
	float modifierNonDirectionalLight = lightPos.w;
	float3 normal = toLight * modifierNonDirectionalLight;

	// spot light calculation - should stay as 1 for point lights
	float rho = max(0, dot(toLight, lightDir.xyz));
	float spotAtt = max(0, (rho - lightAtten.x) * lightAtten.y);

	// add normal for directional lights of toLight - will be 0,0,0 for non-directional lights
	// we create a custom modifier for directional lights that works better than a simple normal calculation
	float4 lightPosWorld = mul(UNITY_MATRIX_T_MV, lightPos);
	float modifierDirectionalLight = 1.0 - modifierNonDirectionalLight;
	normal += (toLight * modifierDirectionalLight);

	// apply spot modifier last
	modifierNonDirectionalLight *= spotAtt;

	atten *= max(0, dot(normal, toLight));

	return lightColor.rgb *
		((atten * modifierNonDirectionalLight) +
		(clamp((lightPosWorld.y * 2) + 1.5, 0, 1) * modifierDirectionalLight));

#endif

}

inline fixed4 CalculateVertexColor(float3 viewPos)
{
	fixed3 vertexColor = UNITY_LIGHTMODEL_AMBIENT.rgb * _AmbientLightMultiplier;
	vertexColor += ApplyLight(unity_LightPosition[0], unity_SpotDirection[0], unity_LightColor[0], unity_LightAtten[0], viewPos);
	vertexColor += ApplyLight(unity_LightPosition[1], unity_SpotDirection[1], unity_LightColor[1], unity_LightAtten[1], viewPos);
	vertexColor += ApplyLight(unity_LightPosition[2], unity_SpotDirection[2], unity_LightColor[2], unity_LightAtten[2], viewPos);
	vertexColor += ApplyLight(unity_LightPosition[3], unity_SpotDirection[3], unity_LightColor[3], unity_LightAtten[3], viewPos);

	return fixed4(vertexColor, 1);
}

inline float3 RotateVertexLocalQuaternion(float3 position, float3 axis, float angle)
{
	float half_angle = angle * 0.5;
	float _sin, _cos;
	sincos(half_angle, _sin, _cos);
	float4 q = float4(axis.xyz * _sin, _cos);
	return position + (2.0 * cross(cross(position, q.xyz) + (q.w * position), q.xyz));
}

fixed GetMiePhase(fixed size, fixed eyeCos, fixed eyeCos2)
{
	fixed temp = 1.0 + MIE_G2 - 2 * MIE_G * eyeCos;
	temp = max(1.0e-4, smoothstep(0.0, 0.005, temp) * temp);
	return size * ((1.0 - MIE_G2) / (2.0 + MIE_G2)) * (1.0 + eyeCos2) / temp;
}

fixed GetRayleighPhase(fixed eyeCos2)
{
	return 0.75 + 0.75 * eyeCos2;
}

fixed GetRayleighPhase(fixed3 light, fixed3 ray)
{
	fixed eyeCos = dot(light, ray);
	return GetRayleighPhase(eyeCos * eyeCos);
}

fixed CalcSunSpot(fixed size, fixed3 vec1, fixed3 vec2)
{
	fixed3 delta = vec1 - vec2;
	fixed dist = length(delta);
	fixed spot = 1.0 - smoothstep(0.0, size * 3, dist);
	return 100 * spot * spot;
}

fixed4 GetSunColorHighQuality(float3 sunNormal, fixed4 sunColor, fixed size, float3 ray)
{
	ray = normalize(ray);
	fixed eyeCos = dot(sunNormal, ray);
	fixed eyeCos2 = eyeCos * eyeCos;
	fixed mie = GetMiePhase(size, eyeCos, eyeCos2);
	return (mie * sunColor);
}

fixed4 GetSunColorFast(float3 sunNormal, fixed4 sunColor, fixed size, float3 ray)
{
	ray = normalize(ray);
	fixed eyeCos = dot(sunNormal, ray);
	fixed eyeCos2 = eyeCos * eyeCos;
	fixed mie = CalcSunSpot(size, sunNormal, -ray);
	return (mie * sunColor);
}

float SkyScale(float inCos)
{
	float x = 1.0 - inCos;
#if defined(SHADER_API_N3DS)
	// The polynomial expansion here generates too many swizzle instructions for the 3DS vertex assembler
	// Approximate by removing x^1 and x^2
	return 0.25 * exp(-0.00287 + x * x * x * (-6.80 + x * 5.25));
#else
	return 0.25 * exp(-0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))));
#endif

}

fixed GetSunLightSkyMultiplier(float3 lightPos, float3 skyVertex)
{
	fixed3 toLight = (lightPos - skyVertex);
	fixed lengthSq = dot(toLight, toLight);
	fixed atten = 1.0 / (1.0 + (lengthSq * 1));
	return clamp(atten * 1.5, 1.0, 1.4);
}

procedural_sky_vertex CalculateSkyVertex(float3 lightPos, fixed3 lightColor, fixed3 groundColor, float3 skyVertex, fixed3 skyTintColor, float atmosphereThickness)
{
	procedural_sky_vertex o;

	static const float3 kDefaultScatteringWavelength = float3(.65, .57, .475);
	static const float3 kVariableRangeForScatteringWavelength = float3(.15, .15, .15);
	static const float OUTER_RADIUS = 1.065;
	static const float kOuterRadius = OUTER_RADIUS;
	static const float kOuterRadius2 = OUTER_RADIUS * OUTER_RADIUS;
	static const float kInnerRadius = 1.0;
	static const float kInnerRadius2 = 1.0;
	static const float kCameraHeight = 0.0001;
	static const float kMIE = 0.0010; // Mie constant
	static const float kSUN_BRIGHTNESS = 20.0; // Sun brightness
	static const float kMAX_SCATTER = 50.0; // Maximum scattering value, to prevent math overflows on Adrenos
	static const half kSunScale = 400.0 * kSUN_BRIGHTNESS;
	static const float kKmESun = kMIE * kSUN_BRIGHTNESS;
	static const float kKm4PI = kMIE * 4.0 * 3.14159265;
	static const float kScale = 1.0 / (OUTER_RADIUS - 1.0);
	static const float kScaleDepth = 0.25;
	static const float kScaleOverScaleDepth = (1.0 / (OUTER_RADIUS - 1.0)) / 0.25;
	static const float kSamples = 2.0; // THIS IS UNROLLED MANUALLY, DON'T TOUCH
	float kRAYLEIGH = (lerp(0, 0.0025, pow(atmosphereThickness, 2.5))); // Rayleigh constant
	float3 kSkyTintInGammaSpace = COLOR_2_GAMMA(skyTintColor); // convert tint from Linear back to Gamma
	float3 kScatteringWavelength = lerp(
		kDefaultScatteringWavelength - kVariableRangeForScatteringWavelength,
		kDefaultScatteringWavelength + kVariableRangeForScatteringWavelength,
		float3(1, 1, 1) - kSkyTintInGammaSpace); // using Tint in sRGB gamma allows for more visually linear interpolation and to keep (.5) at (128, gray in sRGB) point
	float3 kInvWavelength = 1.0 / pow(kScatteringWavelength, 4);
	float kKrESun = kRAYLEIGH * kSUN_BRIGHTNESS;
	float kKr4PI = kRAYLEIGH * 4.0 * 3.14159265;
	float3 cameraPos = float3(0, kInnerRadius + kCameraHeight, 0); // The camera's current position
	// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
	float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, skyVertex));
	o.ray = -eyeRay;
	float far = 0.0;
	fixed3 cIn, cOut;
	if (eyeRay.y >= 0.0)
	{
		// Sky
		// Calculate the length of the "atmosphere"
		far = sqrt(kOuterRadius2 + kInnerRadius2 * eyeRay.y * eyeRay.y - kInnerRadius2) - kInnerRadius * eyeRay.y;
		float3 pos = cameraPos + far * eyeRay;

		// Calculate the ray's starting position, then calculate its scattering offset
		float height = kInnerRadius + kCameraHeight;
		float depth = exp(kScaleOverScaleDepth * (-kCameraHeight));
		float startAngle = dot(eyeRay, cameraPos) / height;
		float startOffset = depth * SkyScale(startAngle);

		// Initialize the scattering loop variables
		float sampleLength = far / kSamples;
		float scaledLength = sampleLength * kScale;
		float3 sampleRay = eyeRay * sampleLength;
		float3 samplePoint = cameraPos + sampleRay * 0.5;

		// Now loop through the sample rays
		float3 frontColor = float3(0.0, 0.0, 0.0);
		{
			float height = length(samplePoint);
			float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
			float lightAngle = dot(lightPos, samplePoint) / height;
			float cameraAngle = dot(eyeRay, samplePoint) / height;
			float scatter = (startOffset + depth * (SkyScale(lightAngle) - SkyScale(cameraAngle)));
			float3 attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));
			frontColor += attenuate * (depth * scaledLength);
			samplePoint += sampleRay;
		}
		{
			float height = length(samplePoint);
			float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
			float lightAngle = dot(lightPos, samplePoint) / height;
			float cameraAngle = dot(eyeRay, samplePoint) / height;
			float scatter = (startOffset + depth * (SkyScale(lightAngle) - SkyScale(cameraAngle)));
			float3 attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));
			frontColor += attenuate * (depth * scaledLength);
			samplePoint += sampleRay;
		}

		// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
		cIn = frontColor * (kInvWavelength * kKrESun);
		cOut = frontColor * kKmESun;
	}
	else
	{
		// Ground
		far = (-kCameraHeight) / (min(-0.001, eyeRay.y));
		float3 pos = cameraPos + far * eyeRay;

		// Calculate the ray's starting position, then calculate its scattering offset
		float depth = exp((-kCameraHeight) * (1.0 / kScaleDepth));
		float cameraAngle = dot(o.ray, pos);
		float lightAngle = dot(lightPos, pos);
		float cameraScale = SkyScale(cameraAngle);
		float lightScale = SkyScale(lightAngle);
		float cameraOffset = depth * cameraScale;
		float temp = (lightScale + cameraScale);

		// Initialize the scattering loop variables
		float sampleLength = far / kSamples;
		float scaledLength = sampleLength * kScale;
		float3 sampleRay = eyeRay * sampleLength;
		float3 samplePoint = cameraPos + sampleRay * 0.5;

		// Now loop through the sample rays
		float3 frontColor = float3(0.0, 0.0, 0.0);
		float3 attenuate;
		{
			float height = length(samplePoint);
			float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
			float scatter = depth * temp - cameraOffset;
			attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));
			frontColor += attenuate * (depth * scaledLength);
			samplePoint += sampleRay;
		}
		cIn = frontColor * (kInvWavelength * kKrESun + kKmESun);
		cOut = clamp(attenuate, 0.0, 1.0);
	}

	fixed rayLeigh = GetRayleighPhase(lightPos, o.ray);
	fixed atten = GetSunLightSkyMultiplier(lightPos, skyVertex);
	o.vertexColor = fixed4((cIn * rayLeigh) + (cIn + COLOR_2_LINEAR(groundColor) * cOut), 1);
	o.sunColor = fixed4((1.0 - cOut) * lightColor, atten);

#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
	o.vertexColor = sqrt(o.vertexColor);
	o.sunColor = sqrt(o.sunColor);
#endif

	return o;
}