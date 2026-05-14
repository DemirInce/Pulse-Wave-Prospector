Shader "Custom/FOVMask"
{
SubShader
{
Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }

    Pass
    {
        ZWrite Off
        ColorMask 0   // Don't draw anything visible

        Stencil
        {
            Ref 5
            Comp Always
            Pass Replace
        }
    }
}


}
