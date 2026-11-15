Shader "Custom/NebulaBackground"
{
    Properties
    {
        _ColorA ("Inner Color", Color) = (0.15, 0.35, 1.0, 1)
        _ColorB ("Outer Color", Color) = (1.0, 0.25, 0.6, 1)
        _Scale ("Noise Scale", Float) = 1.6
        _Density ("Density", Range(0.5, 6)) = 2.5
        _Intensity ("Glow Intensity", Range(0.1, 6)) = 2.0
        _Speed ("Drift Speed", Float) = 0.03
        _Seed ("Seed", Float) = 0
        _StarDensity ("Star Threshold", Range(0.98, 0.9999)) = 0.9985
        _Steps ("March Steps", Range(8, 64)) = 32
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorA, _ColorB;
                float _Scale, _Density, _Intensity, _Speed, _Seed, _StarDensity, _Steps;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float noise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
                         lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                         lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x), f.y),
                    f.z);
            }

            float fbm(float3 p)
            {
                float a = 0.5, s = 0.0;
                [unroll] for (int i = 0; i < 4; i++)
                {
                    s += a * noise(p);
                    p = p * 2.03 + 11.7;
                    a *= 0.5;
                }
                return s;
            }

            float DensityAt(float3 p)
            {
                float3 q = p * _Scale + _Seed + float3(_Time.y * _Speed, 0, _Time.y * _Speed * 0.6);
                float d = fbm(q);
                d = smoothstep(0.42, 0.85, d);
                d *= saturate(1.0 - length(p) / 2.2);   // fade toward the edge of the volume
                return d * _Density;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = (i.positionCS.xy / _ScreenParams.xy) * 2.0 - 1.0;
                uv.x *= _ScreenParams.x / _ScreenParams.y;

                float3 ro = float3(0, 0, -2.5);
                float3 rd = normalize(float3(uv, 1.4));

                int steps = (int)_Steps;
                float stepSize = 4.5 / steps;
                float t = 0.5;
                float trans = 1.0;
                float3 col = 0;

                [loop] for (int s = 0; s < 64; s++)
                {
                    if (s >= steps || trans < 0.02) break;
                    float3 p = ro + rd * t;
                    float d = DensityAt(p);
                    if (d > 0.001)
                    {
                        float3 c = lerp(_ColorA.rgb, _ColorB.rgb, saturate(length(p) / 2.0));
                        col += c * d * trans * stepSize * _Intensity;
                        trans *= exp(-d * stepSize * 2.0);
                    }
                    t += stepSize;
                }

                // Background stars, dimmed by nebula in front of them
                float h = hash(floor(rd * 160.0));
                float star = step(_StarDensity, h) * (0.5 + 0.5 * hash(floor(rd * 160.0) + 3.1));
                col += star * trans;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}