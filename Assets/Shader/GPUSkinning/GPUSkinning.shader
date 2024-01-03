Shader "Unlit/GPUSkinning"
{
    Properties
    {
        [Header(Albedo)]
        _MainTex ("Albedo", 2D) = "white" {}
        _AlbedoIntensity("Albedo Intensity",Range(0,10))=1
        _Color("Color",Color)=(1,1,1,1)
        [Toggle]_Fresnel("Fresnel",float)=0
        _FresnelColor("FresnelColor",Color)=(1,1,1,1)
        _FresnelIntensity("FresnelInstensity",Range(1,20))=5
        [Toggle][ToggleOn]_AlphaClip("Clip",Float)=0
        [Space]
        [Header(Animation)]
        [HideInInspector]_AnimationTex("AnimationTex",2D) = "white"{}
        [HideInInspector]_NumPixelsPerFrame("NumPixelsPerFrame",float)=1
    }
    SubShader
    {
        Tags { "RenderType"="GPUSkinnedOpaque" "PerformaceChecks"="False" "DisableBatching"="true"}
        LOD 300

        Pass
        {
            Tags{"LightMode"="ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ALPHACLIIP_ON
            #pragma multi_compile _ _FRESNEL_ON
            #pragma target 3.0
            #pragma skip_variants SPOT POINT POINT_COOKIE DIRECTIONAL_COOKIE VERTEXLIGHT_ON FOG_EXP FOG_EXP2 DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING LIGHTPROBE_SH DIRLIGHTMAP_COMBINED LIGHTMAP_ON
            
            #include "GPUSKinningInclude.cginc"
            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster" "DisableBatching" = "false"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ALPHACLIIP_ON
            #pragma target 3.0
            
            #include "GPUSKinningInclude.cginc"

            float4 fragShadow(v2f i):SV_Target
            {
                fixed4 mainTex=tex2D(_MainTex,i.uv);
                #ifdef _ALPHACLIP_ON
                    clip(mainTex.a - 0.5);
                #endif
                return 0;
            }
            ENDCG
        }
    }
    SubShader
    {
        Tags { "RenderType" = "GPUSkinnedOpaque" "PerformaceChecks" = "False" "DisableBatching" = "true"}
        LOD 200

        Pass
        {
            Tags{"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ALPHACLIIP_ON
            #define NO_SHADOW
            #pragma target 3.0
            #pragma skip_variants SPOT POINT POINT_COOKIE DIRECTIONAL_COOKIE VERTEXLIGHT_ON FOG_EXP FOG_EXP2 DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING LIGHTPROBE_SH DIRLIGHTMAP_COMBINED LIGHTMAP_ON

            #include "GPUSKinningInclude.cginc"
            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster" "DisableBatching" = "false"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ALPHACLIIP_ON
            #pragma target 3.0

            #include "GPUSKinningInclude.cginc"

            float4 fragShadow(v2f i) :SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex,i.uv);
                #ifdef _ALPHACLIP_ON
                    clip(mainTex.a - 0.5);
                #endif
                return 0;
            }
            ENDCG
        }
    }
}
