// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "FlockSimulation/PhysicallyBasedRendering"
{
    Properties
    {
        _BaseColor ("Base Color", 2D) = "white" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _Roughness("Roughness strength", Range(0.0, 1.0)) = 0.1
        _MetalicMap ("Metalic Map", 2D) = "white" {}
        _Metalic ("Metalic strength", Range(0.0, 1.0)) = 0.0
        _Reflectance("Reflectance", Range(0.0, 1.0)) = 0.5
        _NormalMap("Normal map", 2D) = "black" {}
        _NormalStrength("Normal Strength", Float) = 1.0
        _Emission("Emission Color", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        ZTest LEqual
        ZWrite On
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
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float3 tangent : TEXCOORD3;
                float3 bitangent : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            //Necessary Textures
            sampler2D _BaseColor;
            float4 _BaseColor_ST;
            sampler2D _NormalMap;
            float4 _NormalMap_ST;
            sampler2D _RoughnessMap;
            float4 _RoughnessMap_ST;
            sampler2D _MetalicMap;
            float4 _MetalicMap_ST;
            sampler2D _AOMap;

            //Properties
            float _Roughness;
            float _Metalic;
            float _NormalStrength;
            float _Reflectance;
            float4 _Emission;

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

            float4x4 inverse(float4x4 m) 
            {
                float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
                float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
                float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
                float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

                float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
                float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
                float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
                float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

                float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
                float idet = 1.0f / det;

                float4x4 ret;

                ret[0][0] = t11 * idet;
                ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
                ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
                ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

                ret[1][0] = t12 * idet;
                ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
                ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
                ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

                ret[2][0] = t13 * idet;
                ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
                ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
                ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

                ret[3][0] = t14 * idet;
                ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
                ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
                ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

                return ret;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID (v, o);
                float4 vertex = float4(0.0f, 0.0f, 0.0f, 1.0f);
                #ifdef UNITY_INSTANCING_ENABLED
                int id = UNITY_ACCESS_INSTANCED_PROP(Props, _Index);
                vertex = mul(_Boids[id].localToWorldMatrix, v.vertex);
                float4x4 worldToLocalMatrix = inverse(_Boids[id].localToWorldMatrix);
                o.vertex = mul(UNITY_MATRIX_VP, vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = normalize(mul(v.normal, worldToLocalMatrix));
                o.tangent = normalize(mul(v.tangent, worldToLocalMatrix));
                o.bitangent = normalize(mul(cross(v.normal, v.tangent.xyz) ,worldToLocalMatrix));
                #else
                vertex = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = normalize(mul(v.normal, unity_WorldToObject));
                o.tangent = normalize(mul(v.tangent, unity_WorldToObject));
                o.bitangent = normalize(mul(cross(v.normal, v.tangent.xyz) , unity_WorldToObject));
                #endif

                o.uv = v.uv;
                return o;
            }

            // F0 + (1 - F0) * pow(1.0f - dot(view, H), 5)
            float3 fresnelShlick(float vDotH, float3 f0)
            {
                return f0 + (1.0f - f0)* pow(saturate(1.0f - vDotH), 5.0f);
            }

            //https://i.ytimg.com/vi/wbBtAFpOxg8/maxresdefault.jpg
            float d_ggx(float normHDot, float roughness)
            {
                float alpha = roughness * roughness;
                float alpha_2 = alpha * alpha;
                float normHDot_2 = normHDot * normHDot;
                float div = normHDot_2*(alpha_2-1.0f) + 1.0f;
                return alpha_2 / (UNITY_PI * (div * div));
            }

            float g1_ggx_shlick(float cosTheta, float roughness)
            {
                float alpha = roughness * roughness;
                float k = alpha * 0.5f;
                return max(0.0f, cosTheta) / (cosTheta * (1.0f - k) + k);
            }

            //https://hyungjunpark.weebly.com/uploads/8/2/9/8/82980160/published/3_3.png?1493963988
            float g_smith(float viewNormDot, float lightNormDot, float roughness)
            {
                return g1_ggx_shlick(viewNormDot, roughness) * g1_ggx_shlick(lightNormDot, roughness);
            }

            float3 BRDF_Microfacet(float3 rd, float3 lightDir, float3 normal,
                                   float metalic, float roughness, float reflectance, float3 baseColor, float3 skyboxColor)
            {
                float3 H = normalize(lightDir + rd);
                float vDotN = max(0.0f, dot(normal, rd));
                float vDotH = max(0.0f, dot(H, rd));
                float nDotL = max(0.0f, dot(lightDir, normal));
                float nDotH = max(0.0f, dot(H, normal));
        
                float3 col = baseColor;
                float3 f0 = float3(1.0f, 1.0f, 1.0f) * reflectance * reflectance;
                f0 = lerp(f0, col, metalic);
    
                float3 F = fresnelShlick(vDotH, f0);
                float D = d_ggx(nDotH, roughness);
    
                float G = g_smith(vDotN, nDotL, roughness);
    
                float3 specular = (F * D * G) / (4.0f * max(0.001f, vDotN) * max(0.001f, nDotL));

                col *= (1.0f - metalic);
    
                float3 diffuse = col / UNITY_PI;
    
                return diffuse + specular;
            }


            float4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4 col = tex2D(_BaseColor, TRANSFORM_TEX(i.uv, _BaseColor));
                float atteunation = unity_4LightAtten0.y;

                float3 lightDir = _WorldSpaceLightPos0.a == 0.0f ? normalize(_WorldSpaceLightPos0.xyz ) : normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
                float3 camDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 H = normalize(camDir + lightDir);

                float3 normalMap = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)));
                float normalMapExists = normalMap.b != 0.0f;
                normalMap.b *= (1.0f / _NormalStrength);
                normalMap = normalize(normalMap);
                float3x3 TBN = transpose(float3x3(i.tangent, i.bitangent, i.normal));
                normalMap = normalize(mul(TBN, normalMap));
                float metalic = tex2D(_MetalicMap, TRANSFORM_TEX(i.uv, _MetalicMap)).r;
                float roughness = tex2D(_RoughnessMap, TRANSFORM_TEX(i.uv, _RoughnessMap)).r;
                float3 reflCamDir = normalize(reflect(-camDir, i.normal));
                float4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflCamDir);
                float3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
                float3 normal = i.normal;
                float nDotL = max(0.0f, dot(normal, lightDir));
                float3 finalColor = _Emission +  BRDF_Microfacet(camDir, lightDir, normal, _Metalic * metalic, _Roughness * roughness, _Reflectance, col.rgb, skyColor) * nDotL;
                col.rgb = finalColor;
                col.a = 1.0f;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
