using Noesis;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif

/// <summary>
/// Xaml provider
/// </summary>
public class NoesisXamlProvider: XamlProvider
{
    public static NoesisXamlProvider instance = new NoesisXamlProvider();

    NoesisXamlProvider()
    {
        _xamls = new Dictionary<string, XamlValue>(new UriPathComparer());
        _raws = new Dictionary<string, RawValue>(new UriPathComparer());
    }

    public void Register(string uri, NoesisXaml xaml)
    {
        if (_xamls.TryGetValue(uri, out XamlValue v))
        {
            v.refs++;
            v.xaml = xaml;
            _xamls[uri] = v;
        }
        else
        {
            _xamls[uri] = new XamlValue() { refs = 1, xaml = xaml };
        }
    }

    public void Register(string uri, byte[] data)
    {
        if (_raws.TryGetValue(uri, out RawValue v))
        {
            v.refs++;
            v.data = data;
            _raws[uri] = v;
        }
        else
        {
            _raws[uri] = new RawValue() { refs = 1, data = data };
        }
    }

    public void Unregister(string uri)
    {
        if (_xamls.TryGetValue(uri, out XamlValue xv))
        {
            if (xv.refs == 1)
            {
                _xamls.Remove(uri);
            }
            else
            {
                xv.refs--;
                _xamls[uri] = xv;
            }
        }
        else if (_raws.TryGetValue(uri, out RawValue rv))
        {
            if (rv.refs == 1)
            {
                _raws.Remove(uri);
            }
            else
            {
                rv.refs--;
                _raws[uri] = rv;
            }
        }
    }

    public override Stream LoadXaml(Uri uri)
    {
        string path = uri.OriginalString;

        if (_xamls.TryGetValue(path, out XamlValue xv))
        {
            return new MemoryStream(xv.xaml.content);
        }
        if (_raws.TryGetValue(path, out RawValue rv))
        {
            return new MemoryStream(rv.data);
        }

        return null;
    }

    public void ReloadXaml(string uri)
    {
        if (_xamls.TryGetValue(uri, out _))
        {
            RaiseXamlChanged(new Uri(uri, UriKind.Relative));
        }
    }

    public struct XamlValue
    {
        public int refs;
        public NoesisXaml xaml;
    }
    private Dictionary<string, XamlValue> _xamls;

    public struct RawValue
    {
        public int refs;
        public byte[] data;
    }
    private Dictionary<string, RawValue> _raws;
    
#if UNITY_EDITOR
    internal static string LangServerGetXaml(Uri uri)
    {
        IntPtr uriPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(uri.OriginalString);
        IntPtr strPtr = Noesis_LangServer_GetXaml(uriPtr);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(uriPtr);

        if (strPtr == IntPtr.Zero) return null;

        string str = Noesis.Extend.StringFromNativeUtf8(strPtr);
        NoesisGUI_PINVOKE.FreeString(strPtr);
        return str;
    }

    [DllImport(Library.Name)]
    static extern IntPtr Noesis_LangServer_GetXaml(IntPtr uri);
#endif
}

/// <summary>
/// Audio provider
/// </summary>
public class AudioProvider
{
    public static AudioProvider instance = new AudioProvider();

    AudioProvider()
    {
        _audios = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
    }

    public void Register(string uri, UnityEngine.AudioClip audio)
    {
        Value v;
        if (_audios.TryGetValue(uri, out v))
        {
            v.refs++;
            v.audio = audio;
            _audios[uri] = v;
        }
        else
        {
            _audios[uri] = new Value() { refs = 1, audio = audio };
        }
    }

    public void Unregister(string uri)
    {
        Value v;
        if (_audios.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _audios.Remove(uri);
            }
            else
            {
                v.refs--;
                _audios[uri] = v;
            }
        }
    }

    public void PlayAudio(string uri, float volume)
    {
        Value v;
        if (_audios.TryGetValue(uri, out v) && v.audio != null)
        {
            UnityEngine.AudioSource.PlayClipAtPoint(v.audio, UnityEngine.Vector3.zero, volume);
        }
        else
        {
            UnityEngine.Debug.LogError("AudioClip not found '" + uri + "'");
        }
    }

    public struct Value
    {
        public int refs;
        public UnityEngine.AudioClip audio;
    }

    private Dictionary<string, Value> _audios;
}

/// <summary>
/// Video provider
/// </summary>
public class VideoProvider
{
    public static VideoProvider instance = new VideoProvider();

    VideoProvider()
    {
        _videos = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
    }

    public void Register(string uri, UnityEngine.Video.VideoClip video)
    {
        Value v;
        if (_videos.TryGetValue(uri, out v))
        {
            v.refs++;
            v.video = video;
            _videos[uri] = v;
        }
        else
        {
            _videos[uri] = new Value() { refs = 1, video = video };
        }
    }

    public void Unregister(string uri)
    {
        Value v;
        if (_videos.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _videos.Remove(uri);
            }
            else
            {
                v.refs--;
                _videos[uri] = v;
            }
        }
    }

    public UnityEngine.Video.VideoClip GetVideoClip(string uri)
    {
        Value v;
        if (_videos.TryGetValue(uri, out v) && v.video != null)
        {
            return v.video;
        }
        else
        {
            return null;
        }
    }

    public struct Value
    {
        public int refs;
        public UnityEngine.Video.VideoClip video;
    }

    private Dictionary<string, Value> _videos;
}

/// <summary>
/// Shader provider
/// </summary>
public class NoesisShaderProvider
{
    public static NoesisShaderProvider instance = new NoesisShaderProvider();

    NoesisShaderProvider()
    {
        _shaders = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
    }

    public void Register(string uri, NoesisShader shader)
    {
        Value v;
        if (_shaders.TryGetValue(uri, out v))
        {
            v.refs++;
            v.shader = shader;
            _shaders[uri] = v;
        }
        else
        {
            _shaders[uri] = new Value() { refs = 1, shader = shader };
        }
    }

    public void Unregister(string uri)
    {
        Value v;
        if (_shaders.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _shaders.Remove(uri);
            }
            else
            {
                v.refs--;
                _shaders[uri] = v;
            }
        }
    }

    public NoesisShader GetShader(string uri)
    {
        Value v;
        if (_shaders.TryGetValue(uri, out v) && v.shader != null)
        {
            return v.shader;
        }
        else
        {
            return null;
        }
    }

    public struct Value
    {
        public int refs;
        public NoesisShader shader;
    }

    private Dictionary<string, Value> _shaders;
}

/// <summary>
/// Texture provider
/// </summary>
public class NoesisTextureProvider: TextureProvider
{
    public static NoesisTextureProvider instance = new NoesisTextureProvider();

    NoesisTextureProvider()
    {
        Texture.RegisterCallbacks();
        _textures = new Dictionary<string, Value>(new UriPathComparer());
        _texturesRequested = new List<Value>();
    }

    public void Register(string uri, UnityEngine.Texture texture)
    {
        Int32Rect rect = new Int32Rect(0, 0, texture.width, texture.height);

        Value v;
        if (_textures.TryGetValue(uri, out v))
        {
            v.refs++;
            v.texture = texture;
            v.rect = rect;
            _textures[uri] = v;
        }
        else
        {
            _textures[uri] = new Value() { path = uri, refs = 1, texture = texture, rect = rect };
        }
    }

    public void Register(string uri, UnityEngine.Sprite sprite)
    {
        UnityEngine.Rect r = sprite.packed && sprite.packingMode == UnityEngine.SpritePackingMode.Rectangle ?
            sprite.textureRect : sprite.rect;

        Int32Rect rect = new Int32Rect(
            (int)(r.x * sprite.spriteAtlasTextureScale),
            (int)(r.y * sprite.spriteAtlasTextureScale),
            (int)(r.width * sprite.spriteAtlasTextureScale),
            (int)(r.height * sprite.spriteAtlasTextureScale));

        Value v;
        if (_textures.TryGetValue(uri, out v))
        {
            v.refs++;
            v.texture = sprite.texture;
            v.rect = rect;
            _textures[uri] = v;
        }
        else
        {
            _textures[uri] = new Value() { path = uri, refs = 1, texture = sprite.texture, rect = rect };
        }
    }

    public void Unregister(string uri)
    {
        Value v;
        if (_textures.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _textures.Remove(uri);
            }
            else
            {
                v.refs--;
                _textures[uri] = v;
            }
        }
    }

    public override TextureInfo GetTextureInfo(Uri uri)
    {
        Int32Rect rect = new Int32Rect();

        string path = uri.OriginalString;

        if (_textures.TryGetValue(path, out Value v))
        {
            rect = v.rect;
            _texturesRequested.Add(v);
        }

        return new TextureInfo(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void UpdateTextures()
    {
        // Send information about textures being requested to C++
        if (_texturesRequested.Count > 0)
        {
            foreach (var tex in _texturesRequested.Distinct())
            {
                string path = tex.path;
                Value v = tex;
                if (v.texture != null)
                {
                    IntPtr nativePtr = v.texture.GetNativeTexturePtr();

#if UNITY_EDITOR
                    if (v.nativePtr == IntPtr.Zero)
                    {
                        v.nativePtr = nativePtr;
                        _textures[path] = v;
                    }
                    else if (v.nativePtr != nativePtr)
                    {
                        dirtyTextures = true;
                    }
#endif

                    int width = v.texture.width;
                    int height = v.texture.height;
                    int numLevels = v.texture is UnityEngine.Texture2D t ? t.mipmapCount : 1;

                    Noesis_TextureProviderStoreTextureInfo(swigCPtr.Handle, path,
                        width, height, numLevels, nativePtr);
                }
            }
        }

#if UNITY_EDITOR
        // In the Editor when a texture is modified and reimported its native pointer changes,
        // so we need to send the new texture native pointer to C++ to update texture provider
        if (dirtyTextures)
        {
            dirtyTextures = false;

            List<Value> textures = new List<Value>();

            foreach (var kv in _textures)
            {
                if (kv.Value.texture != null)
                {
                    Value v = kv.Value;
                    v.nativePtr = v.texture.GetNativeTexturePtr();

                    if (v.nativePtr != kv.Value.nativePtr)
                    {
                        textures.Add(v);
                    }
                }
            }

            // Update native pointer in C++ texture provider and raise the TextureChanged event
            foreach (var v in textures)
            {
                string path = v.path;
                int width = v.texture.width;
                int height = v.texture.height;
                int numLevels = v.texture is UnityEngine.Texture2D t ? t.mipmapCount : 1;

                _textures[path] = v;

                Noesis_TextureProviderStoreTextureInfo(swigCPtr.Handle, path,
                    width, height, numLevels, v.nativePtr);

                RaiseTextureChanged(new Uri(path, UriKind.Relative));
            }
        }
#endif

        _texturesRequested.Clear();
    }

    public struct Value
    {
        public string path;
        public int refs;
        public UnityEngine.Texture texture;
        public Int32Rect rect;
#if UNITY_EDITOR
        public IntPtr nativePtr;
#endif
    }

    private Dictionary<string, Value> _textures;
    private List<Value> _texturesRequested;
#if UNITY_EDITOR
    public bool dirtyTextures = false;
#endif

    internal new static IntPtr Extend(string typeName)
    {
        return Noesis_TextureProviderExtend(System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(typeName));
    }

    #region Imports
    [DllImport(Library.Name)]
    static extern IntPtr Noesis_TextureProviderExtend(IntPtr typeName);

    [DllImport(Library.Name)]
    static extern void Noesis_TextureProviderStoreTextureInfo(IntPtr cPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename, int width, int height, int numLevels,
        IntPtr nativePtr);
    #endregion
}

/// <summary>
/// Font provider
/// </summary>
public class NoesisFontProvider: FontProvider
{
    private static LockFontCallback _lockFont = LockFont;
    private static UnlockFontCallback _unlockFont = UnlockFont;
    public static NoesisFontProvider instance = new NoesisFontProvider();

    NoesisFontProvider()
    {
        Noesis_FontProviderSetLockUnlockCallbacks(_lockFont, _unlockFont);
        _fonts = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
    }

    public void Register(string uri, NoesisFont font)
    {
        bool register = false;
        Value v;

        if (_fonts.TryGetValue(uri, out v))
        {
            register = v.font != font;

            v.refs++;
            v.font = font;
            _fonts[uri] = v;
        }
        else
        {
            register = true;
            _fonts[uri] = new Value() { refs = 1, font = font };
        }

        if (register)
        {
            string folder = System.IO.Path.GetDirectoryName(uri);
            string filename = System.IO.Path.GetFileName(uri);
            RegisterFont(new Uri(folder, UriKind.Relative), filename);
        }
    }

    public void Unregister(string uri)
    {
        Value v;

        if (_fonts.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _fonts.Remove(uri);
            }
            else
            {
                v.refs--;
                _fonts[uri] = v;
            }
        }
    }

    public void ReloadFont(string uri)
    {
        Value v;
        if (_fonts.TryGetValue(uri, out v))
        {
            // TODO: Review this API, it should be enough to notify with the uri
            List<Face> faces = new List<Face>();

            using (MemoryStream stream = new MemoryStream(v.font.content))
            {
                GUI.EnumFontFaces(stream, (index_, family_, weight_, style_, stretch_) =>
                {
                    faces.Add(new Face()
                    {
                        index = index_, family = family_,
                        weight = weight_, style = style_, stretch = stretch_
                    });
                });
            }

            string folder = System.IO.Path.GetDirectoryName(uri).Replace('\\', '/') + "/";
            foreach (Face face in faces)
            {
                RaiseFontChanged(new Uri(folder, UriKind.Relative), face.family, face.weight, face.stretch, face.style);
            }
        }
    }

    struct Face
    {
        public int index;
        public string family;
        public FontWeight weight;
        public FontStyle style;
        public FontStretch stretch;
    }

    private delegate void LockFontCallback(string folder, string filename, out IntPtr handle, out IntPtr addr, out int length);
    [MonoPInvokeCallback(typeof(LockFontCallback))]
    private static void LockFont(string folder, string filename, out IntPtr handle, out IntPtr addr, out int length)
    {
        try
        {
            NoesisFontProvider provider = NoesisFontProvider.instance;

            Value v;
            provider._fonts.TryGetValue(folder + "/" + filename, out v);

            if (v.font != null && v.font.content != null)
            {
                GCHandle h = GCHandle.Alloc(v.font.content, GCHandleType.Pinned);
                handle = GCHandle.ToIntPtr(h);
                addr = h.AddrOfPinnedObject();
                length = v.font.content.Length;
                return;
            }
        }
        catch (Exception exception)
        {
            Error.UnhandledException(exception);
        }

        handle = IntPtr.Zero;
        addr = IntPtr.Zero;
        length = 0;
    }

    private delegate void UnlockFontCallback(IntPtr handle);
    [MonoPInvokeCallback(typeof(UnlockFontCallback))]
    private static void UnlockFont(IntPtr handle)
    {
        // In rare cases, the passed handle belongs to a domain already unloaded. That memory has
        // been already deallocated so we can safely ignore the exception
        try
        {
            GCHandle h = GCHandle.FromIntPtr(handle);
            h.Free();
        }
        catch (Exception) {}
    }

    public struct Value
    {
        public int refs;
        public NoesisFont font;
    }

    private Dictionary<string, Value> _fonts;

    internal new static IntPtr Extend(string typeName)
    {
        return Noesis_FontProviderExtend(System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(typeName));
    }

    #region Imports
    [DllImport(Library.Name)]
    static extern IntPtr Noesis_FontProviderExtend(IntPtr typeName);

    [DllImport(Library.Name)]
    static extern void Noesis_FontProviderSetLockUnlockCallbacks(LockFontCallback lockFont, UnlockFontCallback unlockFont);
    #endregion
}

/// <summary>
/// Compares Uri paths without allocating memory
/// </summary>
public class UriPathComparer : IEqualityComparer<string>
{
    #region IEqualityComparer
    public bool Equals(string x, string y)
    {
        return y.EndsWith(x, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string uri)
    {
        int offset = Path(uri);
        return Hash(uri, offset);
    }
    #endregion

    #region Path
    private int Path(string uri)
    {
        // remove '/Assembly;component/'
        int pos = uri.IndexOf(COMPONENT_STR, StringComparison.OrdinalIgnoreCase);
        if (pos != -1)
        {
            return pos + 11;
        }

        // remove 'pack://application:,,,'
        if (uri.StartsWith(PACK_STR, StringComparison.OrdinalIgnoreCase))
        {
            return 23;
        }

        // remove starting '/'
        if (uri[0] == '/')
        {
            return 1;
        }

        return 0;
    }

    private const string PACK_STR = "pack:";
    private const string COMPONENT_STR = ";component/";
    #endregion

    #region Hash
    private int Hash(string uri, int offset)
    {
        // Taken from .NET String.GetHashCode() implementation
        int hash1 = 5381;
        int hash2 = hash1;

        int idx = offset;
        while (idx < uri.Length)
        {
            int c1 = char.ToUpperInvariant(uri[idx]);
            hash1 = ((hash1 << 5) + hash1) ^ c1;

            if (idx + 1 >= uri.Length) break;

            int c2 = char.ToUpperInvariant(uri[idx + 1]);
            hash2 = ((hash2 << 5) + hash2) ^ c2;

            idx += 2;
        }

        return hash1 + (hash2 * 1566083941);
    }
    #endregion
}
