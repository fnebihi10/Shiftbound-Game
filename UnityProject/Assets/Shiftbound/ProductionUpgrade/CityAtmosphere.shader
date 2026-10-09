Shader "Shiftbound/Registered far city"
{
    Properties { _MainTex("Distant city panorama",2D)="white"{} _Tint("Atmosphere",Color)=(.86,.92,1,1) }
    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _Tint;
            struct A{float4 vertex:POSITION;};struct V{float4 pos:SV_POSITION;float3 direction:TEXCOORD0;};
            V vert(A v){V o;o.pos=TransformObjectToHClip(v.vertex.xyz);o.direction=v.vertex.xyz;return o;}
            half4 frag(V i):SV_Target {
                float3 d=normalize(i.direction);
                float u=frac(atan2(d.x,d.z)*.159154943+.75);
                // Compress the painted city into a physically distant horizon band.
                // Real near/middle buildings supply scale, contact and parallax.
                float v=saturate(.39+d.y*2.45);
                half3 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,float2(u,v)).rgb;
                half3 ends=.5*(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,float2(.008,v)).rgb+SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,float2(.992,v)).rgb);
                float continuity=smoothstep(0,.045,u)*(1-smoothstep(.955,1,u));
                c=lerp(ends,c,continuity);
                half3 horizon=half3(.70,.77,.80)*_Tint.rgb;
                half3 zenith=half3(.22,.43,.72)*_Tint.rgb;
                half3 sky=lerp(horizon,zenith,saturate(d.y*.9));
                c=lerp(c,sky,smoothstep(.18,.30,d.y));
                c=lerp(horizon,c,smoothstep(-.24,-.04,d.y));
                return half4(c,1);
            }
            ENDHLSL
        }
    }
}
