Shader "Custom/HexTexture"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _PointyHexTexture ("Pointy Hex Texture", 2D) = "white" {}
        _FlatHexTexture ("Flat Hex Texture", 2D) = "white" {}
        _HexTextureLerp ("Lerp t value for pointy/flat hex texture", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _PointyHexTexture;
        sampler2D _FlatHexTexture;

        struct Input
        {
            float2 uv_PointyHexTexture;
            float2 uv_FlatHexTexture;
            float4 color : COLOR;
        };

        half _HexTextureLerp;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = lerp(
                tex2D (_PointyHexTexture, IN.uv_PointyHexTexture),
                tex2D (_FlatHexTexture, IN.uv_FlatHexTexture),
                _HexTextureLerp
            ) * _Color;
            o.Albedo = lerp(c.rgb, IN.color, 0.5);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
