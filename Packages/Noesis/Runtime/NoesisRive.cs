using UnityEngine;

public class NoesisRive: ScriptableObject
{
    public string uri;
    public byte[] content;

    [System.Serializable]
    public struct Texture
    {
        public string uri;
        public UnityEngine.Texture texture;
    }

    [System.Serializable]
    public struct Font
    {
        public string uri;
        public NoesisFont font;
    }

    public Texture[] textures;
    public Font[] fonts;
}
