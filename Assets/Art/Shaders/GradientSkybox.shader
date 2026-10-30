Shader "WeatherApp/GradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.16, 0.34, 0.62, 1)
        _HorizonColor ("Horizon Color", Color) = (0.62, 0.76, 0.88, 1)
        _GroundColor ("Ground Color", Color) = (0.30, 0.30, 0.28, 1)
        _HorizonSharpness ("Horizon Sharpness", Range(0.5, 8)) = 3.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _TopColor;
            fixed4 _HorizonColor;
            fixed4 _GroundColor;
            float _HorizonSharpness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float height = normalize(i.direction).y;

                // Blend from horizon to zenith, then fade into the ground colour below the horizon.
                float up = saturate(pow(saturate(height), 1.0 / _HorizonSharpness));
                fixed3 sky = lerp(_HorizonColor.rgb, _TopColor.rgb, up);

                float down = saturate(-height * 4.0);
                fixed3 colour = lerp(sky, _GroundColor.rgb, down);

                return fixed4(colour, 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}