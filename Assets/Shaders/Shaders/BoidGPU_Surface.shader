Shader "FlockSimulation/BoidGPU_Surface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma multi_compile _ PROCEDURAL_INSTANCING_ON

        #pragma target 5.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(int, _Index)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Boid
        {
            float4 worldPosition; // pos x, y, z | radius w
            float4 size;
            float4 velocity; // velocity x y z | debug
            float4 acceleration;
            float4x4 localToWorldMatrix;
        };

        #ifdef SHADER_API_D3D11
        StructuredBuffer<Boid> _Boids;
        #endif

        void vert (inout appdata_full v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                float4 vertex = float4(0.0f, 0.0f, 0.0f, 1.0f);
                #ifdef UNITY_INSTANCING_ENABLED
                int id = UNITY_ACCESS_INSTANCED_PROP(Props, _Index);
                vertex = mul(_Boids[id].localToWorldMatrix, v.vertex);
               // unity_ObjectToWorld = _Boids[id].localToWorldMatrix;
                #endif
                v.vertex = vertex;
            }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
