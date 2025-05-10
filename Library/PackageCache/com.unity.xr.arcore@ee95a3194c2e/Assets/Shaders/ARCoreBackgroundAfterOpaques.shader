Shader "Unlit/ARCoreBackground/AfterOpaques"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _EnvironmentDepth("Texture", 2D) = "black" {}
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////
    // GLSL shader for OpenGLES 3. GLES3 cannot use HLSL shader like Vulkan because of dependency on
    // GL_OES_EGL_image_external_essl3 extension. Thus they are kept in different subshaders.
    /////////////////////////////////////////////////////////////////////////////////////////////////
    SubShader
    {
        Name "ARCore Background (After Opaques) for GLES3"
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "ForceNoShadowCasting" = "True"
        }

        Pass
        {
            Name "ARCore Background Occlusion Handling"
            Cull Off
            ZTest GEqual
            ZWrite On
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }

            GLSLPROGRAM

            #pragma only_renderers gles3

            #pragma multi_compile_local __ ARCORE_ENVIRONMENT_DEPTH_ENABLED
            #pragma multi_compile_local __ ARCORE_IMAGE_STABILIZATION_ENABLED

            #include "UnityCG.glslinc"

#ifdef SHADER_API_GLES3
#extension GL_OES_EGL_image_external_essl3 : require
#endif // SHADER_API_GLES3

#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE vec2
#else // ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE vec3
#endif // !ARCORE_IMAGE_STABILIZATION_ENABLED

            // Device display transform is provided by the AR Foundation camera background renderer.
            uniform mat4 _UnityDisplayTransform;

#ifdef VERTEX
            varying ARCORE_TEXCOORD_TYPE textureCoord;
            varying vec2 textureCoordQuad;

            void main()
            {
#ifdef SHADER_API_GLES3
#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
                // Transform the position from object space to clip space.
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;

#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
                // Get quad uvs for sampling camera texture
                textureCoordQuad = gl_MultiTexCoord0;

                // Remap the texture coordinates based on the device rotation.
                // _UnityDisplayTransform is provided as "Row Major" for all mobile platforms, so use the
                // 'Row Vector * Matrix' operator. The GLSL '*' operator is overloaded to use a row vector
                // when a matrix is left-multiplied by a vector. Refer to this doc for more information:
                // https://en.wikibooks.org/wiki/GLSL_Programming/Vector_and_Matrix_Operations#Operators
                textureCoord = (vec4(gl_MultiTexCoord0.x, gl_MultiTexCoord0.y, 1.0f, 0.0f) * _UnityDisplayTransform).xy;

#else // ARCORE_IMAGE_STABILIZATION_ENABLED
                textureCoordQuad = ComputeScreenPos(gl_Position);

                textureCoord = gl_MultiTexCoord0.xyz;
#endif // !ARCORE_IMAGE_STABILIZATION_ENABLED

#else // !ARCORE_ENVIRONMENT_DEPTH_ENABLED
                // Don't Subtract depth when ARCore Occlusion is not enabled.
                // Accomplish this by clipping all four vertices of the mesh/
                gl_Position = vec4(0.0f, 0.0f, -1.0f, 0.0f);
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED
#endif // SHADER_API_GLES3
            }
#endif // VERTEX

#ifdef FRAGMENT
            varying ARCORE_TEXCOORD_TYPE textureCoord;

            varying vec2 textureCoordQuad;

            uniform samplerExternalOES _MainTex;
            uniform float _UnityCameraForwardScale;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
            uniform sampler2D _EnvironmentDepth;
            uniform sampler2D _CameraDepthTexture;
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED

            float ConvertDistanceToDepth(float d)
            {
                d = _UnityCameraForwardScale > 0.0 ? _UnityCameraForwardScale * d : d;

                float zBufferParamsW = 1.0 / _ProjectionParams.y;
                float zBufferParamsY = _ProjectionParams.z * zBufferParamsW;
                float zBufferParamsX = 1.0 - zBufferParamsY;
                float zBufferParamsZ = zBufferParamsX * _ProjectionParams.w;

                // Clip any distances smaller than the near clip plane, and compute the depth value from the distance.
                return (d < _ProjectionParams.y) ? 1.0f : ((1.0 / zBufferParamsZ) * ((1.0 / d) - zBufferParamsW));
            }

            void main()
            {
#ifdef SHADER_API_GLES3
#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
                vec2 uvs = textureCoord.xy;
#else // ARCORE_IMAGE_STABILIZATION_ENABLED
                // apply perspective correction
                vec2 uvs = textureCoord.xy / textureCoord.z;
#endif

                vec3 result = texture(_MainTex, uvs).xyz;

                float depth = texture(_CameraDepthTexture, textureCoordQuad).x;

                float distance = texture(_EnvironmentDepth, uvs).x;
                float environmentDepth = ConvertDistanceToDepth(distance);

#ifndef UNITY_COLORSPACE_GAMMA
                result = GammaToLinearSpace(result);
#endif // !UNITY_COLORSPACE_GAMMA

                if (depth >= environmentDepth)
                {
                    discard;
                }

                gl_FragColor = vec4(result, 1.0);
                gl_FragDepth = depth;
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED
#endif // SHADER_API_GLES3
            }

#endif // FRAGMENT
            ENDGLSL
        }

        Pass
        {
            Name "AR Camera Background (ARCore)"
            Cull Off
            ZTest LEqual
            ZWrite On
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }

            GLSLPROGRAM

            #pragma multi_compile_local __ ARCORE_ENVIRONMENT_DEPTH_ENABLED
            #pragma multi_compile_local __ ARCORE_IMAGE_STABILIZATION_ENABLED

            #pragma only_renderers gles3

            #include "UnityCG.glslinc"

#ifdef SHADER_API_GLES3
#extension GL_OES_EGL_image_external_essl3 : require
#endif // SHADER_API_GLES3

#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE vec2
#else // ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE vec3
#endif // !ARCORE_IMAGE_STABILIZATION_ENABLED

            // Device display transform is provided by the AR Foundation camera background renderer.
            uniform mat4 _UnityDisplayTransform;

#ifdef VERTEX
            varying ARCORE_TEXCOORD_TYPE textureCoord;

            void main()
            {
#ifdef SHADER_API_GLES3
                // Transform the position from object space to clip space.
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;

#ifdef ARCORE_IMAGE_STABILIZATION_ENABLED
                textureCoord = gl_MultiTexCoord0.xyz;
#else
                // Remap the texture coordinates based on the device rotation.
                // _UnityDisplayTransform is provided as "Row Major" for all mobile platforms, so use the
                // 'Row Vector * Matrix' operator. The GLSL '*' operator is overloaded to use a row vector
                // when a matrix is left-multiplied by a vector. Refer to this doc for more information:
                // https://en.wikibooks.org/wiki/GLSL_Programming/Vector_and_Matrix_Operations#Operators
                textureCoord.xy = (vec4(gl_MultiTexCoord0.x, gl_MultiTexCoord0.y, 1.0f, 0.0f) * _UnityDisplayTransform).xy;
#endif
#endif // SHADER_API_GLES3
            }
#endif // VERTEX

#ifdef FRAGMENT
            varying ARCORE_TEXCOORD_TYPE textureCoord;
            uniform samplerExternalOES _MainTex;
            uniform float _UnityCameraForwardScale;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
            uniform sampler2D _EnvironmentDepth;
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED

#if defined(SHADER_API_GLES3) && !defined(UNITY_COLORSPACE_GAMMA)
            float GammaToLinearSpaceExact (float value)
            {
                if (value <= 0.04045F)
                    return value / 12.92F;
                else if (value < 1.0F)
                    return pow((value + 0.055F)/1.055F, 2.4F);
                else
                    return pow(value, 2.2F);
            }

            vec3 GammaToLinearSpace (vec3 sRGB)
            {
                // Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
                return sRGB * (sRGB * (sRGB * 0.305306011F + 0.682171111F) + 0.012522878F);

                // Precise version, useful for debugging, but the pow() function is too slow.
                // return vec3(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b));
            }

#endif // SHADER_API_GLES3 && !UNITY_COLORSPACE_GAMMA

            float ConvertDistanceToDepth(float d)
            {
                d = _UnityCameraForwardScale > 0.0 ? _UnityCameraForwardScale * d : d;

                float zBufferParamsW = 1.0 / _ProjectionParams.y;
                float zBufferParamsY = _ProjectionParams.z * zBufferParamsW;
                float zBufferParamsX = 1.0 - zBufferParamsY;
                float zBufferParamsZ = zBufferParamsX * _ProjectionParams.w;

                // Clip any distances smaller than the near clip plane, and compute the depth value from the distance.
                return (d < _ProjectionParams.y) ? 1.0f : ((1.0 / zBufferParamsZ) * ((1.0 / d) - zBufferParamsW));
            }

            void main()
            {
#ifdef SHADER_API_GLES3
#ifdef ARCORE_IMAGE_STABILIZATION_ENABLED
                vec2 tc = textureCoord.xy / textureCoord.z;
#else
                vec2 tc = textureCoord;
#endif
                vec3 result = texture(_MainTex, tc).xyz;
                float depth = 1.0;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
                float distance = texture(_EnvironmentDepth, tc).x;
                depth = ConvertDistanceToDepth(distance);
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED

#ifndef UNITY_COLORSPACE_GAMMA
                result = GammaToLinearSpace(result);
#endif // !UNITY_COLORSPACE_GAMMA

                gl_FragColor = vec4(result, 1.0);
                gl_FragDepth = depth;
#endif // SHADER_API_GLES3
            }

#endif // FRAGMENT
            ENDGLSL
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    // HLSL shader for Vulkan. It should be kept the same with GLES3 except for the syntax diff.
    /////////////////////////////////////////////////////////////////////////////////////////////
    SubShader
    {
        Name "ARCore Background (After Opaques) for Vulkan"
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "ForceNoShadowCasting" = "True"
        }

        Pass
        {
            Name "ARCore Background Occlusion Handling"
            Cull Off
            ZTest GEqual
            ZWrite On
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }

            HLSLPROGRAM

            #pragma only_renderers vulkan

            #pragma multi_compile_local __ ARCORE_ENVIRONMENT_DEPTH_ENABLED
            #pragma multi_compile_local __ ARCORE_IMAGE_STABILIZATION_ENABLED

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE float2
#else // ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE float3
#endif // !ARCORE_IMAGE_STABILIZATION_ENABLED

            // Device display transform is provided by the AR Foundation camera background renderer.
            float4x4 _UnityDisplayTransform;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                ARCORE_TEXCOORD_TYPE textureCoord : TEXCOORD0;
                float2 textureCoordQuad : TEXCOORD1;
            };

            v2f vert(vertexInput i)
            {
                v2f o;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
                // Transform the position from object space to clip space.
                o.position = UnityObjectToClipPos(i.vertex.xyz);

#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
                // Get quad uvs for sampling camera texture
                o.textureCoordQuad = i.uv.xy;

                // Remap the texture coordinates based on the device rotation.
                // _UnityDisplayTransform is provided as "Row Major" for all mobile platforms, so use the
                // 'Row Vector * Matrix' operator. In HLSL, with mul(x, y) if x is a vector, it treated
                // as a row vector. For more information:
                // https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-mul
                o.textureCoord = mul(float4(i.uv.x, i.uv.y, 1.0f, 0.0f), _UnityDisplayTransform).xy;

#else // ARCORE_IMAGE_STABILIZATION_ENABLED
                o.textureCoordQuad = ComputeScreenPos(o.position);

                o.textureCoord = i.uv.xyz;
#endif // !ARCORE_IMAGE_STABILIZATION_ENABLED

#else // !ARCORE_ENVIRONMENT_DEPTH_ENABLED
                // Don't Subtract depth when ARCore Occlusion is not enabled.
                // Accomplish this by clipping all four vertices of the mesh/
                o.position = float4(0.0f, 0.0f, -1.0f, 0.0f);

                // Ensure output is initialized and mute the warning.
                o.textureCoordQuad = float2(0.0, 0.0);
#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
                o.textureCoord = float2(0.0, 0.0);
#else
                o.textureCoord = float3(0.0, 0.0, 0.0);
#endif

#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED
                return o;
            }

            sampler2D _MainTex;
            float _UnityCameraForwardScale;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
            sampler2D _EnvironmentDepth;
            sampler2D _CameraDepthTexture;
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED

            float ConvertDistanceToDepth(float d)
            {
                d = _UnityCameraForwardScale > 0.0 ? _UnityCameraForwardScale * d : d;

                float zBufferParamsW = 1.0 / _ProjectionParams.y;
                float zBufferParamsY = _ProjectionParams.z * zBufferParamsW;
                float zBufferParamsX = 1.0 - zBufferParamsY;
                float zBufferParamsZ = zBufferParamsX * _ProjectionParams.w;

                // Clip any distances smaller than the near clip plane, and compute the depth value from the distance.
                return (d < _ProjectionParams.y) ? 1.0f : ((1.0 / zBufferParamsZ) * ((1.0 / d) - zBufferParamsW));
            }

            struct fragOutput
            {
                float4 color : SV_Target;
                float depth : SV_Depth;
            };

            fragOutput frag(v2f i)
            {
                fragOutput o;
                // Ensure output is initialized and mute the warning.
                o.color = float4(0.0, 0.0, 0.0, 0.0);
                o.depth = 0.0;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
                float2 uvs = i.textureCoord.xy;
#else // ARCORE_IMAGE_STABILIZATION_ENABLED
                // apply perspective correction
                float2 uvs = i.textureCoord.xy / i.textureCoord.z;
#endif

                float depth = tex2D(_CameraDepthTexture, i.textureCoordQuad).x;

                float distance = tex2D(_EnvironmentDepth, uvs).x;
                float environmentDepth = ConvertDistanceToDepth(distance);

                if (depth >= environmentDepth)
                {
                    discard;
                    return o;
                }

                float3 result = tex2D(_MainTex, uvs).xyz;
#ifndef UNITY_COLORSPACE_GAMMA
                result = GammaToLinearSpace(result);
#endif // !UNITY_COLORSPACE_GAMMA

                o.color = float4(result, 1.0);
                o.depth = 1.0 - depth; // Unity Vulkan uses reverse Z.

#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED
                return o;
            }

            ENDHLSL
        }

        Pass
        {
            Name "AR Camera Background (ARCore)"
            Cull Off
            ZTest LEqual
            ZWrite On
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }

            HLSLPROGRAM

            #pragma multi_compile_local __ ARCORE_ENVIRONMENT_DEPTH_ENABLED
            #pragma multi_compile_local __ ARCORE_IMAGE_STABILIZATION_ENABLED

            #pragma only_renderers vulkan

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

#ifndef ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE float2
#else // ARCORE_IMAGE_STABILIZATION_ENABLED
#define ARCORE_TEXCOORD_TYPE float3
#endif // !ARCORE_IMAGE_STABILIZATION_ENABLED

            // Device display transform is provided by the AR Foundation camera background renderer.
            float4x4 _UnityDisplayTransform;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                ARCORE_TEXCOORD_TYPE textureCoord : TEXCOORD0;
            };

            v2f vert(vertexInput i)
            {
                v2f o;

                // Transform the position from object space to clip space.
                o.position = UnityObjectToClipPos(i.vertex.xyz);

#ifdef ARCORE_IMAGE_STABILIZATION_ENABLED
                o.textureCoord = i.uv.xyz;
#else
                // Remap the texture coordinates based on the device rotation.
                // _UnityDisplayTransform is provided as "Row Major" for all mobile platforms, so use the
                // 'Row Vector * Matrix' operator. In HLSL, with mul(x, y) if x is a vector, it treated
                // as a row vector. For more information:
                // https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-mul
                o.textureCoord = mul(float4(i.uv.x, i.uv.y, 1.0f, 0.0f), _UnityDisplayTransform).xy;
#endif
                return o;
            }

            sampler2D _MainTex;
            float _UnityCameraForwardScale;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
            sampler2D _EnvironmentDepth;
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED

#ifndef UNITY_COLORSPACE_GAMMA
            float3 GammaToLinearSpace(float3 sRGB)
            {
                // Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
                return sRGB * (sRGB * (sRGB * 0.305306011F + 0.682171111F) + 0.012522878F);
            }
#endif // !UNITY_COLORSPACE_GAMMA

            float ConvertDistanceToDepth(float d)
            {
                d = _UnityCameraForwardScale > 0.0 ? _UnityCameraForwardScale * d : d;

                float zBufferParamsW = 1.0 / _ProjectionParams.y;
                float zBufferParamsY = _ProjectionParams.z * zBufferParamsW;
                float zBufferParamsX = 1.0 - zBufferParamsY;
                float zBufferParamsZ = zBufferParamsX * _ProjectionParams.w;

                // Clip any distances smaller than the near clip plane, and compute the depth value from the distance.
                return (d < _ProjectionParams.y) ? 1.0f : ((1.0 / zBufferParamsZ) * ((1.0 / d) - zBufferParamsW));
            }

            struct fragOutput
            {
                float4 color : SV_Target;
                float depth : SV_Depth;
            };

            fragOutput frag(v2f i)
            {
#ifdef ARCORE_IMAGE_STABILIZATION_ENABLED
                float2 tc = i.textureCoord.xy / i.textureCoord.z;
#else
                float2 tc = i.textureCoord;
#endif
                float3 result = tex2D(_MainTex, tc).xyz;
                float depth = 1.0;

#ifdef ARCORE_ENVIRONMENT_DEPTH_ENABLED
                float distance = tex2D(_EnvironmentDepth, tc).x;
                depth = ConvertDistanceToDepth(distance);
#endif // ARCORE_ENVIRONMENT_DEPTH_ENABLED

#ifndef UNITY_COLORSPACE_GAMMA
                result = GammaToLinearSpace(result);
#endif // !UNITY_COLORSPACE_GAMMA

                fragOutput o;

                o.color = float4(result, 1.0);
                o.depth = 1.0 - depth; // Unity Vulkan uses reverse Z.

                return o;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
