// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UI_TexturedImageFlipbook"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _CausticTexture("CausticTexture", 2D) = "white" {}
        _Power("Power", Float) = 0.1
        _Columns("Columns", Int) = 4
        _FlipbookSpeed("FlipbookSpeed", Float) = 1
        _Rows("Rows", Int) = 4
        _FlipbookColor("FlipbookColor", Color) = (0.4858491,0.4880465,1,0)
        _Transition("Transition", Range( 0 , 1)) = 0
        _TransitionTexture("TransitionTexture", 2D) = "white" {}
        _Flip("Flip", Int) = 1
        [HideInInspector] _texcoord( "", 2D ) = "white" {}

    }

    SubShader
    {
		LOD 0

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
        	Ref [_Stencil]
        	ReadMask [_StencilReadMask]
        	WriteMask [_StencilWriteMask]
        	Comp [_StencilComp]
        	Pass [_StencilOp]
        }


        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        
        Pass
        {
            Name "Default"
        CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityShaderVariables.cginc"


            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
                
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            uniform float _Transition;
            uniform int _Flip;
            uniform sampler2D _TransitionTexture;
            uniform sampler2D _CausticTexture;
            uniform int _Columns;
            uniform int _Rows;
            uniform float _FlipbookSpeed;
            uniform float4 _FlipbookColor;
            uniform float _Power;

            
            v2f vert(appdata_t v )
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                

                v.vertex.xyz +=  float3( 0, 0, 0 ) ;

                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = v.texcoord;
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN ) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                float2 appendResult96 = (float2((float)_Flip , 1.0));
                float2 texCoord78 = IN.texcoord.xy * appendResult96 + float2( -2,0 );
                float2 appendResult76 = (float2(( _Flip * ( _Transition * -1.0 ) ) , 0.0));
                float2 texCoord77 = IN.texcoord.xy * float2( 1,1 ) + appendResult76;
                float clampResult83 = clamp( ( ( ( _Transition * 4.0 ) + texCoord78.x ) + tex2D( _TransitionTexture, texCoord77 ).r ) , 0.0 , 1.0 );
                float2 texCoord46 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
                // *** BEGIN Flipbook UV Animation vars ***
                // Total tiles of Flipbook Texture
                float fbtotaltiles43 = (float)_Columns * (float)_Rows;
                // Offsets for cols and rows of Flipbook Texture
                float fbcolsoffset43 = 1.0f / (float)_Columns;
                float fbrowsoffset43 = 1.0f / (float)_Rows;
                // Speed of animation
                float fbspeed43 = _Time.y * _FlipbookSpeed;
                // UV Tiling (col and row offset)
                float2 fbtiling43 = float2(fbcolsoffset43, fbrowsoffset43);
                // UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
                // Calculate current tile linear index
                float fbcurrenttileindex43 = round( fmod( fbspeed43 + 0.0, fbtotaltiles43) );
                fbcurrenttileindex43 += ( fbcurrenttileindex43 < 0) ? fbtotaltiles43 : 0;
                // Obtain Offset X coordinate from current tile linear index
                float fblinearindextox43 = round ( fmod ( fbcurrenttileindex43, (float)_Columns ) );
                // Multiply Offset X by coloffset
                float fboffsetx43 = fblinearindextox43 * fbcolsoffset43;
                // Obtain Offset Y coordinate from current tile linear index
                float fblinearindextoy43 = round( fmod( ( fbcurrenttileindex43 - fblinearindextox43 ) / (float)_Columns, (float)_Rows ) );
                // Reverse Y to get tiles from Top to Bottom
                fblinearindextoy43 = (int)((float)_Rows-1) - fblinearindextoy43;
                // Multiply Offset Y by rowoffset
                float fboffsety43 = fblinearindextoy43 * fbrowsoffset43;
                // UV Offset
                float2 fboffset43 = float2(fboffsetx43, fboffsety43);
                // Flipbook UV
                half2 fbuv43 = texCoord46 * fbtiling43 + fboffset43;
                // *** END Flipbook UV Animation vars ***
                float2 uv_MainTex = IN.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                float4 blendOpSrc28 = ( tex2D( _CausticTexture, fbuv43 ) * _FlipbookColor );
                float4 blendOpDest28 = tex2D( _MainTex, uv_MainTex );
                float4 lerpBlendMode28 = lerp(blendOpDest28,max( blendOpSrc28, blendOpDest28 ),_Power);
                

                half4 color = ( clampResult83 * lerpBlendMode28 );

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
        ENDCG
        }
    }
    CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.RangedFloatNode;75;-1760,-592;Inherit;False;Property;_Transition;Transition;6;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-1360,-288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;98;-1344,-448;Inherit;False;Property;_Flip;Flip;8;0;Create;True;0;0;0;False;0;False;1;1;False;0;1;INT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-1152,-320;Inherit;False;2;2;0;INT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;46;-992,160;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;19;-960,576;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;48;-928,272;Inherit;False;Property;_Columns;Columns;2;0;Create;True;0;0;0;False;0;False;4;2;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;49;-928,352;Inherit;False;Property;_Rows;Rows;4;0;Create;True;0;0;0;False;0;False;4;2;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-960,480;Inherit;False;Property;_FlipbookSpeed;FlipbookSpeed;3;0;Create;True;0;0;0;False;0;False;1;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;76;-1008,-288;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;96;-1024,-464;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;43;-640,224;Inherit;False;0;0;6;0;FLOAT2;1,0;False;1;FLOAT;3;False;2;FLOAT;3;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;77;-848,-304;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-1024,-592;Inherit;False;2;2;0;FLOAT;2;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;78;-832,-464;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;-1,1;False;1;FLOAT2;-2,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;3;-608,16;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-384,192;Inherit;True;Property;_CausticTexture;CausticTexture;0;0;Create;True;0;0;0;False;0;False;-1;58e268609009d964b90ccc3241ea7df1;efd3df4a7af65ed4c8b79b99cad3741f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;50;-336,384;Inherit;False;Property;_FlipbookColor;FlipbookColor;5;0;Create;True;0;0;0;False;0;False;0.4858491,0.4880465,1,0;0.4150937,0.359034,0.346564,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;81;-592,-304;Inherit;True;Property;_TransitionTexture;TransitionTexture;7;0;Create;True;0;0;0;False;0;False;-1;None;4bd6618be485426998392fc8a5e9bc18;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;80;-512,-528;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-384,0;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-80,208;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;41;0,368;Inherit;False;Property;_Power;Power;1;0;Create;True;0;0;0;False;0;False;0.1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;-224,-432;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;28;128,0;Inherit;False;Lighten;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;83;16,-432;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;624,-16;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;848,-16;Float;False;True;-1;2;ASEMaterialInspector;0;3;UI_TexturedImageFlipbook;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;3;1;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;True;0;True;_ColorMask;False;False;False;False;False;False;False;True;True;0;True;_Stencil;255;True;_StencilReadMask;255;True;_StencilWriteMask;0;True;_StencilComp;0;True;_StencilOp;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;0;True;unity_GUIZTestMode;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;84;0;75;0
WireConnection;97;0;98;0
WireConnection;97;1;84;0
WireConnection;76;0;97;0
WireConnection;96;0;98;0
WireConnection;43;0;46;0
WireConnection;43;1;48;0
WireConnection;43;2;49;0
WireConnection;43;3;44;0
WireConnection;43;5;19;0
WireConnection;77;1;76;0
WireConnection;79;0;75;0
WireConnection;78;0;96;0
WireConnection;5;1;43;0
WireConnection;81;1;77;0
WireConnection;80;0;79;0
WireConnection;80;1;78;1
WireConnection;4;0;3;0
WireConnection;51;0;5;0
WireConnection;51;1;50;0
WireConnection;82;0;80;0
WireConnection;82;1;81;1
WireConnection;28;0;51;0
WireConnection;28;1;4;0
WireConnection;28;2;41;0
WireConnection;83;0;82;0
WireConnection;66;0;83;0
WireConnection;66;1;28;0
WireConnection;0;0;66;0
ASEEND*/
//CHKSM=F307E89CC42B4F431BFAE9D580B571FCB6F68C2E