Shader "Shiftbound/Benchmark Route Preview"
{
    Properties { _BaseColor("Preview", Color) = (0.2,0.75,0.85,0.16) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float3 local:TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.world = TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.world);
                o.normal = TransformObjectToWorldNormal(input.normalOS);
                o.local = input.positionOS.xyz;
                return o;
            }
            half4 Frag(Varyings input):SV_Target
            {
                // The footprint is legible as a proposal, with hatching and no opaque fill.
                float hatch = step(0.86, frac((input.world.x + input.world.z) * 2.0));
                float edge = 1.0 - step(0.035, min(0.5-abs(input.local.x), 0.5-abs(input.local.z)));
                float top = step(0.65, input.normal.y);
                return half4(_BaseColor.rgb, lerp(0.045, _BaseColor.a + hatch * 0.12 + edge * 0.25, top));
            }
            ENDHLSL
        }
    }
}
