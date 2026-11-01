
Shader "Custom/RadialVignette"
{
    Properties
    {
        _BaseColor ("Vignette Color", Color) = (0, 0, 0, 1)
        _Radius ("Circle Size", Range(0.05, 1)) = 0.35
        _Softness ("Edge Softness", Range(0.01, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Radius;
                float _Softness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS =
                    TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 p = IN.uv - float2(0.5, 0.5);

                // ชดเชยอัตราส่วนหน้าจอให้วงกลมไม่ยืด
                p.x *= _ScreenParams.x / _ScreenParams.y;

                float d = length(p);

                float edge = smoothstep(
                    _Radius,
                    _Radius + _Softness,
                    d
                );

                half4 color = _BaseColor;
                color.a *= edge;

                return color;
            }

            ENDHLSL
        }
    }
}

