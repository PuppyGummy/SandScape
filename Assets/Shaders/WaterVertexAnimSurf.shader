Shader "Custom/WaterVertexAnimSurf"
{
    Properties
    {
        _Color ("Albedo", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _WaveStrength ("Wave Strength", Float) = 0.5
        _WaveFrequency ("Wave Frequency", Float) = 1.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
        [Toggle] _YAxis("Y Axis?", int) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        AlphaToMask Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha
        #pragma target 3.0
        #pragma shader_feature _ALPHAPREMULTIPLY_ON

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Metallic;
        half _Smoothness;
        fixed4 _Color;
        float _WaveStrength;
        float _WaveFrequency;
        float _WaveSpeed;
        int _YAxis;

        #pragma vertex vert

        void vert(inout appdata_full v)
        {
            float wave = sin(_WaveFrequency * (v.vertex.y * _YAxis + v.vertex.z * (1-_YAxis) + _Time.y * _WaveSpeed)) * _WaveStrength;
            v.vertex.y = v.vertex.y + wave * _YAxis;
            v.vertex.z = v.vertex.z + wave * (1-_YAxis);

            float slightHorizontalMovementScale = 0.1;
            v.vertex.x += slightHorizontalMovementScale * wave;
            v.vertex.y += slightHorizontalMovementScale * wave * (1 - _YAxis);
            v.vertex.z += slightHorizontalMovementScale * wave * _YAxis;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = _Color.a;
        }

        ENDCG
    }
    FallBack "Legacy Shaders/Transparent/Cutout/VertexLit"
}