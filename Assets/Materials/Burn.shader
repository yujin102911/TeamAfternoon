Shader "UI/Burn"
{
    Properties
    {
        // 기본 UI Image 텍스처
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        // UI 색상 틴트
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Dissolve 효과 제어 (0 = 정상, 1 = 완전히 불탐)
        _DissolveAmount ("Dissolve Amount", Range(0, 1.2)) = 0
        
        // 노이즈 텍스처 (불타는 패턴)
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        
        // 불타는 가장자리 두께
        _EdgeWidth ("Edge Width", Range(0.01, 0.3)) = 0.1
        
        // UI Stencil 설정 (Canvas 마스킹용)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        // UI용 Stencil 설정
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            // 프로퍼티 선언
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            
            // Dissolve 프로퍼티
            float _DissolveAmount;
            float _EdgeWidth;
            
            // Vertex 입력 구조체
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            // Fragment 입력 구조체
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // Vertex Shader
            // UI 오브젝트의 정점을 화면 공간으로 변환합니다
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                
                return OUT;
            }
            
            // Fragment Shader
            // 각 픽셀의 색상을 계산하여 불타는 효과를 만듭니다
            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 원본 텍스처 샘플링
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // 2. 노이즈 텍스처 샘플링 (불타는 패턴)
                float noise = tex2D(_NoiseTex, IN.texcoord * _NoiseTex_ST.xy).r;
                
                // 3. Dissolve 임계값 계산
                // noise 값이 dissolveThreshold보다 낮으면 해당 픽셀은 사라짐
                float dissolveThreshold = _DissolveAmount;
                
                // 4. Dissolve 적용
                // noise가 threshold보다 작으면 discard (투명하게)
                float dissolve = step(dissolveThreshold, noise);
                
                // 5. 알파값 적용
                // dissolve가 0이면 완전 투명, 1이면 원본 알파값 유지
                color.a *= dissolve;
                
                // 6. UI Rect Mask 적용 (Canvas의 Mask 컴포넌트 지원)
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                // 7. 알파가 0이면 픽셀 버림 (조기 종료로 성능 향상)
                #ifdef UNITY_UI_CLIP_RECT
                clip(color.a - 0.001);
                #endif
                
                return color;
            }
            ENDCG
        }
    }
}