using UnityEngine;

public class NoesisShader: ScriptableObject
{
    public int type;
    public string label;
    public byte[] code;

    public System.IntPtr effect;
    public System.IntPtr brush_path;
    public System.IntPtr brush_path_aa;
    public System.IntPtr brush_sdf;
    public System.IntPtr brush_opacity;
}
