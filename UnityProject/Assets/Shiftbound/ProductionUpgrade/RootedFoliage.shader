Shader "Shiftbound/Rooted foliage"
{
    Properties { _Wind("Leaf motion",Float)=.014 _LeafMap("Veined ivy",2D)="white"{} }
    SubShader {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
        Cull Off
        Pass {
            Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            float _Wind;
            TEXTURE2D(_LeafMap);SAMPLER(sampler_LeafMap);
            struct A{float4 vertex:POSITION;float3 normal:NORMAL;half4 color:COLOR;float2 uv:TEXCOORD0;};
            struct V{float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;half4 color:COLOR;half fog:TEXCOORD2;float2 uv:TEXCOORD3;};
            V vert(A v){V o;float3 w=TransformObjectToWorld(v.vertex.xyz);w.x+=sin(_Time.y*1.8+w.x*.9+w.z*.7)*_Wind*v.color.a;w.z+=cos(_Time.y*1.2+w.y)*_Wind*.5*v.color.a;o.pos=TransformWorldToHClip(w);o.world=w;o.normal=TransformObjectToWorldNormal(v.normal);o.color=v.color;o.fog=ComputeFogFactor(o.pos.z);o.uv=v.uv;return o;}
            half4 frag(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target{
                float3 n=normalize(i.normal)*IS_FRONT_VFACE(face,1,-1);Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
                half diffuse=saturate(dot(n,light.direction));half back=saturate(dot(-n,light.direction))*.22;
                half4 surface=half4(1,1,1,1);
                if(i.color.a>.5){surface=SAMPLE_TEXTURE2D(_LeafMap,sampler_LeafMap,i.uv);clip(surface.a-.45);}
                half3 c=i.color.rgb*surface.rgb*(SampleSH(n)*.8+light.color*(.2+diffuse*.8+back)*light.shadowAttenuation);
                return half4(MixFog(c,i.fog),1);
            }
            ENDHLSL
        }
        Pass {
            Name "ShadowCaster"
            Tags{"LightMode"="ShadowCaster"}
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float _Wind;float3 _LightDirection;TEXTURE2D(_LeafMap);SAMPLER(sampler_LeafMap);
            struct A{float4 vertex:POSITION;float3 normal:NORMAL;half4 color:COLOR;float2 uv:TEXCOORD0;};
            struct V{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;half leaf:TEXCOORD1;};
            V shadowVert(A v) {
                float3 w=TransformObjectToWorld(v.vertex.xyz);
                w.x+=sin(_Time.y*1.8+w.x*.9+w.z*.7)*_Wind*v.color.a;
                w.z+=cos(_Time.y*1.2+w.y)*_Wind*.5*v.color.a;
                float3 n=TransformObjectToWorldNormal(v.normal);
                float4 p=TransformWorldToHClip(ApplyShadowBias(w,n,_LightDirection));
                #if UNITY_REVERSED_Z
                p.z=min(p.z,p.w*UNITY_NEAR_CLIP_VALUE);
                #else
                p.z=max(p.z,p.w*UNITY_NEAR_CLIP_VALUE);
                #endif
                V o;o.pos=p;o.uv=v.uv;o.leaf=v.color.a;return o;
            }
            half4 shadowFrag(V i):SV_Target{if(i.leaf>.5)clip(SAMPLE_TEXTURE2D(_LeafMap,sampler_LeafMap,i.uv).a-.45);return 0;}
            ENDHLSL
        }
    }
}
