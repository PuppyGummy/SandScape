Shader "Custom/WaterVertexAnimSurf"
{
    Properties
    {
        _WaterColor ("Albedo", Color) = (1,1,1,1)
        _FoamColor("Foam Color", Color) = (0,0,0,0)
        _FoamSize("Foam Size", Range(0,10)) = 2
        _EdgeWidth("Edge Width", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 1
        _WaveStrength ("Wave Strength", Float) = 2
        _WaveFrequency ("Wave Frequency", Float) = 1.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        AlphaToMask Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade keepalpha nolightmap nodirlightmap vertex:vert
        #pragma target 3.0
        #pragma shader_feature _ALPHAPREMULTIPLY_ON

        struct Input
        {
            float3 worldPos;
            float4 screenPos;
			half3 worldNormal;
			INTERNAL_DATA
        };

        half _Metallic;
        half _Smoothness;
        fixed4 _WaterColor;
        fixed4 _FoamColor;
        fixed _FoamSize;
        fixed _EdgeWidth;
        UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
        float _WaveStrength;
        float _WaveFrequency;
        float _WaveSpeed;

        //noise

        float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}

        void vert(inout appdata_full v)
        {
            float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
            
            // f(t) = Asin(ω*t+φ)
            // waveOffset = sin( _WaveSpeed * t + φ) * _WaveStrength, φ is based on world position
            
            // float wavePhase = _Time.y * _WaveSpeed + worldPosition.x * _WaveFrequency + worldPosition.z * _WaveFrequency;
            
            float wavePhase1 = _Time.y * _WaveSpeed + (worldPosition.x + worldPosition.z) * _WaveFrequency;
            float wavePhase2 = _Time.y * (_WaveSpeed * 0.75) + (worldPosition.x - worldPosition.z) * (_WaveFrequency * 1.5);

            // float waveOffset = sin(wavePhase) * _WaveStrength;
            float waveOffset1 = sin(wavePhase1) * _WaveStrength * 0.1;
            float waveOffset2 = sin(wavePhase2) * _WaveStrength * 0.05;

            float waveOffset = waveOffset1 + waveOffset2;
            
            worldPosition.y += waveOffset;
            worldPosition.x += 0.1 * waveOffset;
            worldPosition.z += 0.1 * waveOffset;
            
            // Transform the modified world position back to local space
            v.vertex = mul(unity_WorldToObject, float4(worldPosition, 1.0));
        }

        void surf (Input i, inout SurfaceOutputStandard o)
        {
            half3 worldPos = i.worldPos;
			half2 panner1 = ( 0.1 * _Time.y * float2( 1,0 ) + (worldPos).xz);
			half simplePerlin1 = snoise( ( panner1 * 1 * _FoamSize) );
			half2 panner2 = ( 0.1 * _Time.y * float2( -1,0 ) + (worldPos).xz);
			half simplePerlin2 = snoise( ( panner2 * 2 * _FoamSize) );
			half largeNoisePattern = clamp( ( simplePerlin1 + simplePerlin2 ) , 0.0 , 1.0 );
			float4 screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 screenPosNorm = screenPos / screenPos.w;
			screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;
			half eyeDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, screenPosNorm.xy ));

            // the depth difference between the water and the scene in the camera space
			float edgeEffect = abs( ( eyeDepth - screenPos.w ) );

			// calculate hard edge foam
			float foamWater = ( 1.0 - step( largeNoisePattern , edgeEffect - _EdgeWidth) );
			float foamEdge = ( 1.0 - step( (0.1 + (_SinTime.w - -1.0) * (0.3 - 0.1) / (1.0 - -1.0)) , edgeEffect - _EdgeWidth) );

            o.Albedo = saturate( ( saturate( ( ( _FoamColor * foamWater ) + ( _FoamColor * foamEdge ) ) ) + _WaterColor ) ).rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = _WaterColor.a;
        }

        ENDCG
    }
    FallBack "Legacy Shaders/Transparent/Cutout/VertexLit"
}