Shader "Custom/RadialVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Vignette Color", Color) = (0, 0, 0, 0.91)
        _Radius ("Circle Size", Range(0.05, 1)) = 0.2
        _Softness ("Edge Softness", Range(0.01, 1)) = 0.247
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Radius;
                float _Softness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centeredUv = input.uv - float2(0.5, 0.5);
                centeredUv.x *= _ScreenParams.x / _ScreenParams.y;
                float distanceFromCenter = length(centeredUv);
                float darkness = smoothstep(_Radius, _Radius + _Softness, distanceFromCenter);
                half4 color = _BaseColor * input.color;
                color *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                color.a *= darkness;
                return color;
            }
            ENDHLSL
        }
    }
}
