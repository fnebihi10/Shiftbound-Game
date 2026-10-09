Shader "Shiftbound/Rooted foliage"
{
    Properties { _Wind("Leaf motion",Float)=.014 }
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
            struct A{float4 vertex:POSITION;float3 normal:NORMAL;half4 color:COLOR;};
            struct V{float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;half4 color:COLOR;half fog:TEXCOORD2;};
            V vert(A v){V o;float3 w=TransformObjectToWorld(v.vertex.xyz);w.x+=sin(_Time.y*1.8+w.x*.9+w.z*.7)*_Wind*v.color.a;w.z+=cos(_Time.y*1.2+w.y)*_Wind*.5*v.color.a;o.pos=TransformWorldToHClip(w);o.world=w;o.normal=TransformObjectToWorldNormal(v.normal);o.color=v.color;o.fog=ComputeFogFactor(o.pos.z);return o;}
            half4 frag(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target{
                float3 n=normalize(i.normal)*IS_FRONT_VFACE(face,1,-1);Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
                half diffuse=saturate(dot(n,light.direction));half back=saturate(dot(-n,light.direction))*.22;
                half3 c=i.color.rgb*(SampleSH(n)*.8+light.color*(.2+diffuse*.8+back)*light.shadowAttenuation);
                return half4(MixFog(c,i.fog),1);
            }
            ENDHLSL
        }
    }
}
