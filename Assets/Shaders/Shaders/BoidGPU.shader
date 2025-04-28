// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Unlit/BoidGPU"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PROCEDURAL_INSTANCING_ON
            #pragma multi_compile_instancing 
            #pragma target 5.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            
            struct Boid
            {
                float4 worldPosition; // pos x, y, z | radius w
                float4 size;
                float4 velocity; // velocity x y z | debug
                float4 acceleration;
                float4x4 localToWorldMatrix;
            };

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(int, _Index)
            UNITY_INSTANCING_BUFFER_END(Props)

            StructuredBuffer<Boid> _Boids;
            StructuredBuffer<float> _BoidsBufferTest;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                float4 vertex = float4(0.0f, 0.0f, 0.0f, 1.0f);
                #ifdef UNITY_INSTANCING_ENABLED
                int id = UNITY_ACCESS_INSTANCED_PROP(Props, _Index);
                vertex = mul(_Boids[id].localToWorldMatrix, v.vertex);
                #else
                vertex = mul(UNITY_MATRIX_M, v.vertex);
                #endif

                o.vertex = mul(UNITY_MATRIX_VP, vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
