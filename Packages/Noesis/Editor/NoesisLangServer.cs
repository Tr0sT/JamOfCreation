using System;
using System.Runtime.InteropServices;
using Noesis;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public class NoesisLangServer
{
    private delegate void Callback_LangServerRender(IntPtr contentPtr, int renderWidth, int renderHeight,
        double renderTime, string savePath);
    private delegate IntPtr Callback_LangServerTextureLoad(IntPtr filename, ref uint x, ref uint y,
        ref uint width, ref uint height, ref float dpiScale, ref uint numLevels);

    private static Callback_LangServerTextureLoad _langServerTextureLoad = LangServerTextureLoad;
    private static Callback_LangServerRender _langServerRender = LangServerRender;	
    private static bool _isInitialized = false;
    private static bool _isDisabled = false;

    static NoesisLangServer()
    {
        EditorApplication.update += () =>
        {
            if (!_isDisabled)
            {
                if (!_isInitialized)
                {
                    _isInitialized = true; 
                    NoesisUnity.Init();
                    Init();
                    return;
                }

                Noesis_LangServer_RunTick();
            }
        };
    }

    private static void Init()
    {
        try
        {
            SetLangServerName("Unity");
            Noesis_SetUnityLangServerProviders();
            Noesis_SetLangServerTextureLoadCallback(_langServerTextureLoad);
            Noesis_LangServer_SetRenderCallback(_langServerRender);
            Noesis_LangServer_Init();
        }
        catch (EntryPointNotFoundException)
        {
            _isDisabled = true;
            return;
        }

        System.AppDomain.CurrentDomain.DomainUnload += (sender, e) =>
        {
            Noesis_LangServer_Shutdown();
        };
    }

    private static bool IsGL()
    {
        var type = SystemInfo.graphicsDeviceType;

      #if UNITY_2023_1_OR_NEWER
        return type == GraphicsDeviceType.OpenGLES3 || type == GraphicsDeviceType.OpenGLCore;
      #else
        return type == GraphicsDeviceType.OpenGLES2 || type == GraphicsDeviceType.OpenGLES3
            || type == GraphicsDeviceType.OpenGLCore;
      #endif
    }

    private static void Capture(UIElement content, int width, int height, double time, string savePath)
    {
        try
        {
            // MuteLog is required to prevent XAML and binding errors showing during render
            NoesisUnity.MuteLog();

            CommandBuffer commands = new CommandBuffer();

            Border root = new Border();
            root.Child = content;
            View view = Noesis.GUI.CreateView(root);
            view.SetFlags(IsGL() ? 0 : RenderFlags.FlipY);
            view.SetTessellationMaxPixelError(Noesis.TessellationMaxPixelError.HighQuality.Error);
            view.SetSize(width, height);
            view.Update(0.0);

            NoesisRenderer.RegisterView(view, commands);
            Graphics.ExecuteCommandBuffer(commands);
            commands.Clear();

            NoesisRenderer.SetRenderSettings();

            view.Update(time);

            NoesisRenderer.UpdateRenderTree(view, commands);
            NoesisRenderer.RenderOffscreen(view, commands, false);

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default,
                RenderTextureReadWrite.Default, 8);
            commands.SetRenderTarget(rt);
            commands.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
            NoesisRenderer.RenderOnscreen(view, false, commands, true, false);

            Graphics.ExecuteCommandBuffer(commands);
            commands.Clear();

            RenderTexture.ReleaseTemporary(rt);

            if (rt != null)
            {
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;

                Texture2D tex = new Texture2D(width, height);
                tex.ReadPixels(new UnityEngine.Rect(0, 0, width, height), 0, 0);
                tex.Apply(true);

                RenderTexture.active = prev;

                System.IO.File.WriteAllBytes(savePath, tex.EncodeToPNG());
            }

            NoesisRenderer.UnregisterView(view, commands);
            Graphics.ExecuteCommandBuffer(commands);
            commands.Clear();
            view.Content?.Dispose();
            view.Dispose();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            NoesisUnity.UnmuteLog();
        }
    }

    private static void SetLangServerName(string serverName)
    {
        IntPtr namePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(serverName);
        Noesis_LangServer_SetName(namePtr);
        System.Runtime.InteropServices.Marshal.FreeHGlobal(namePtr);
    }

    #region StringFromNativeUtf8 Implementation
    private static int NextPowerOf2(int x)
    {
        if (x < 0) return 0;
        --x;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }

    [ThreadStatic]
    private static byte[] _byteBuffer = null;

    public static string StringFromNativeUtf8(IntPtr nativeUtf8)
    {
        if (nativeUtf8 == IntPtr.Zero)
        {
            return string.Empty;
        }

#if __MonoCS__
            // Mono on all platforms currently uses UTF-8 encoding 
            return Marshal.PtrToStringAnsi(nativeUtf8);
#else
        // Waiting for PtrToStringUtf8 implementation in C#
        // https://github.com/dotnet/corefx/issues/9605
        int len = 0;
        while (Noesis.Marshal.ReadByte(nativeUtf8, len) != 0) len++;
        if (_byteBuffer == null || len > _byteBuffer.Length) _byteBuffer = new byte[NextPowerOf2(Math.Max(2048, len))];
        Noesis.Marshal.Copy(nativeUtf8, _byteBuffer, 0, len);
        return System.Text.Encoding.UTF8.GetString(_byteBuffer, 0, len);
#endif
    }
    #endregion

    [MonoPInvokeCallback(typeof(Callback_LangServerTextureLoad))]
    private static IntPtr LangServerTextureLoad(IntPtr filenamePtr,
        ref uint x, ref uint y, ref uint width, ref uint height, ref float dpiScale,
        ref uint numLevels)
    {
        try
        {
            string filename = StringFromNativeUtf8(filenamePtr);

            Texture2D texture = new Texture2D(1, 1);

            byte[] bytes = System.IO.File.ReadAllBytes(filename);

            if (texture.LoadImage(bytes))
            {
                // NoesisGUI needs premultipled alpha
                UnityEngine.Color[] c = texture.GetPixels(0);

                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                {
                    for (int i = 0; i < c.Length; i++)
                    {
                        c[i].r = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].r) * c[i].a);
                        c[i].g = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].g) * c[i].a);
                        c[i].b = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].b) * c[i].a);
                    }
                }
                else
                {
                    for (int i = 0; i < c.Length; i++)
                    {
                        c[i].r = c[i].r * c[i].a;
                        c[i].g = c[i].g * c[i].a;
                        c[i].b = c[i].b * c[i].a;
                    }
                }

                // Set new content
                texture.SetPixels(c, 0);
                texture.Apply(true, true);

                // Set texture info
                x = 0;
                y = 0;
                width = (uint)texture.width;
                height = (uint)texture.height;
                dpiScale = 1;
                numLevels = (uint)texture.mipmapCount;

                return texture.GetNativeTexturePtr();
            }

            x = 0;
            y = 0;
            width = 0;
            height = 0;
            dpiScale = 1;
        }
        catch (Exception e)
        {
            Error.UnhandledException(e);
        }

        return IntPtr.Zero;
    }

    [MonoPInvokeCallback(typeof(Callback_LangServerRender))]
    private static void LangServerRender(IntPtr contentPtr, int renderWidth, int renderHeight, double renderTime, string savePath)
    {
        if (!NoesisUnity.Initialized || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        try
        {
            UIElement content = (UIElement)BaseComponent.GetProxy(contentPtr);
            if (content != null)
            {
                Capture(content, renderWidth, renderHeight, renderTime, savePath);
            }
        }
        catch (Exception e)
        {
            Error.UnhandledException(e);
        }
    }

    #region Imports
    [DllImport(Library.Name)]
    static extern void Noesis_LangServer_Init();

    [DllImport(Library.Name)]
    static extern void Noesis_LangServer_RunTick();

    [DllImport(Library.Name)]
    static extern void Noesis_LangServer_Shutdown();

    [DllImport(Library.Name)]
    static extern void Noesis_LangServer_SetName(IntPtr serverName);

    [DllImport(Library.Name)]
    static extern void Noesis_SetUnityLangServerProviders();

    [DllImport(Library.Name)]
    static extern void Noesis_SetLangServerTextureLoadCallback(Callback_LangServerTextureLoad langServerTextureLoad);

    [DllImport(Library.Name)]
    static extern void Noesis_LangServer_SetRenderCallback(Callback_LangServerRender langServerRender);
    #endregion
}
