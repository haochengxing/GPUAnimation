#ifndef GPUSKINNED_INCLUDE
#define GPUSKINNED_INCLUDE

#include "../DiffuseInclude.cginc"

struct appdata
{
	float4 vertex	: POSITION;
	float3 normal	: NORMAL;
	float2 uv		: TEXCOORD0;
	float2 uv2		: TEXCOORD1;
	float4 uv3		: TEXCOORD2;
	float4 uv4		: TEXCOORD3;

#ifdef INSTANCING_ON
	UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
};

struct v2f
{
	float4 pos		: SV_POSITION;
	half3 normal	: NORMAL;
	float2 uv		: TEXCOORD0;
	half3 lightDir	: TEXCOORD1;
	float4 worldPos : TEXCOORD2;

#ifdef INSTANCING_ON
	UNITY_VERTEX_INPUT_INSTANCE_ID
#endif

		UNITY_FOG_COORDS(6)

#ifndef NO_SHADOW
		SHADOW_COORDS(7)
#endif

};

#ifdef INSTANCING_ON
UNITY_INSTANCING_BUFFER_START(VertProps)
	UNITY_DEFINE_INSTANCED_PROP(float,_FrameIndex)
UNITY_INSTANCING_BUFFER_END(VertProps)
#else
uniform float _FrameIndex;
#endif

uniform sampler2D_half _AnimationTex;
uniform float4 _AnimationTex_TexelSize;
uniform float _NumPixelsPerFrame;

float4 _FresnelColor;
float _FresnelIntensity;

inline float4 indexToUV(float index)
{
	float row = floor(index/_AnimationTex_TexelSize.z);
	float col = index - row * _AnimationTex_TexelSize.z;
	return float4(col/_AnimationTex_TexelSize.z,row/_AnimationTex_TexelSize.w,0,0);
}

inline float4x4 getMatrix(float frameStartIndex,float boneIndex)
{
	float matStartIndex = frameStartIndex + boneIndex * 3;
	float4 row0 = tex2Dlod(_AnimationTex, indexToUV(matStartIndex));
	float4 row1 = tex2Dlod(_AnimationTex, indexToUV(matStartIndex + 1));
	float4 row2 = tex2Dlod(_AnimationTex, indexToUV(matStartIndex + 2));
	float4 row3 = float4(0, 0, 0, 1);
	float4x4 mat = float4x4(row0, row1, row2, row3);
	return mat;
}

inline float4x4 getSkinnedMatrix(float4 boneIndexes,float4 boneWeights,float frameIndex)
{
	float frameStartIndex = frameIndex * _NumPixelsPerFrame;
	boneIndexes = round(boneIndexes);
	float4x4 mat = getMatrix(frameStartIndex, boneIndexes.x) * boneWeights.x;
	mat += getMatrix(frameStartIndex, boneIndexes.y) * boneWeights.y;
	mat += getMatrix(frameStartIndex, boneIndexes.z) * boneWeights.z;
	mat += getMatrix(frameStartIndex, boneIndexes.w) * boneWeights.w;
	return mat;
}

v2f vert(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);

#ifdef INSTANCING_ON
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	float frameIndex = UNITY_ACCESS_INSTANCED_PROP(VertProps, _FrameIndex);
#else
	float frameIndex = _FrameIndex;
#endif

	float4x4 skinnedMatrix = getSkinnedMatrix(v.uv3, v.uv4, frameIndex);
	float4 pos = mul(skinnedMatrix, v.vertex);
	half3 mormal = mul((float3x3)skinnedMatrix, v.normal);

#ifndef NO_SHADOW
	v.vertex = pos;
#endif

	o.normal = UnityObjectToWorldNormal(mormal);
	o.worldPos = mul(unity_ObjectToWorld, pos);
	o.lightDir = normalize(_WorldSpaceLightPos0.xyz);
	o.pos = UnityObjectToClipPos(pos);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);

#ifndef NO_SHADOW
	TRANSFER_SHADOW(o);
#endif

	UNITY_TRANSFER_FOG(o, o.pos);

	return o;
}

fixed4 frag(v2f i) :SV_Target
{
	half3 normal = i.normal;
	half3 lightDir = i.lightDir;
	half ndotL = max(0.0, dot(normal, lightDir));
	fixed4 mainTex = tex2D(_MainTex, i.uv);

#ifdef _ALPHACLIP_ON
	clip(mainTex.a - 0.5);
#endif

	fixed3 albedo = mainTex.rgb;
	fixed3 col = albedo * _AlbedoIntensity * _Color.rgb;
	fixed3 lightColor = _LightColor0;

#ifndef NO_SHADOW
	lightColor *= SHADOW_ATTENUATION(i);
#endif

	const fixed3 ambient = fixed3(.65, .65, .7);
	fixed3 color = col.rgb * ambient + col.rgb * lightColor * ndotL;

#ifdef _FRESNEL_ON
	float3 worldViewDir = normalize(_WorldSpaceCamera.rgb - i.worldPos.rgb);
	color = color + _FresnelColor * pow((1 - max(dot(normal, worldViewDir), 0)), _FresnelIntensity);
#endif

	UNITY_APPLY_FOG(i.fogCoord, color);

	return fixed4(color, 1.0);
}

#endif