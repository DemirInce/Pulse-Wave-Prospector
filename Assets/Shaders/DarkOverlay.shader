Shader "Custom/DarkOverlay"
{
Properties
{
_Color ("Color", Color) = (0,0,0,0.8)
}

SubShader
{
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }

    Pass
    {
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 5
            Comp NotEqual
        }

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        struct appdata {
            float4 vertex : POSITION;
        };

        struct v2f {
            float4 pos : SV_POSITION;
        };

        float4 _Color;

        v2f vert (appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            return _Color;
        }
        ENDCG
    }
}

}
