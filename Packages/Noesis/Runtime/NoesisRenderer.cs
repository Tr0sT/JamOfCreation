using System.Runtime.InteropServices;
using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

#if UNITY_2022_2_OR_NEWER
using static UnityEngine.Rendering.CustomMarkerCallbackFlags;
#endif

#if ENABLE_URP_PACKAGE_RENDER_GRAPH
using UnityEngine.Rendering.RenderGraphModule;
#endif

/// <summary>
/// In Unity, the render thread is only accesible in C++ using IssuePluginEvent(). This is a helper
/// class to communicate a C# view with its C++ renderer.
/// </summary>
public class NoesisRenderer
{
    /// <summary>
    /// Registers a view in the render thread
    /// </summary>
    public static void RegisterView(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands)
    {
        commands.IssuePluginEventAndData(_renderRegisterCallback, (int)EventId.NoRender, view.Renderer.CPtr.Handle);
    }

    /// <summary>
    /// Sends render tree update commands to native code
    /// </summary>
    public static void UpdateRenderTree(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands)
    {
        // Send information about requested textures to C++ texture provider
        NoesisTextureProvider.instance.UpdateTextures();

        commands.IssuePluginEventAndData(_updateRenderTreeCallback, (int)EventId.NoRender, view.Renderer.CPtr.Handle);
    }

    /// <summary>
    /// Sends offscreen render commands to native code
    /// </summary>
    public static void RenderOffscreen(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands, bool invalidate)
    {
        // This a way to force Unity to close the current MTL command encoder
        // We need to activate a new encoder in the current command buffer for our Offscreen phase
        if (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
        {
            UnityEngine.RenderTexture surface = UnityEngine.RenderTexture.GetTemporary(1,1);
            commands.SetRenderTarget(surface, LoadAction.DontCare, StoreAction.DontCare, LoadAction.DontCare, StoreAction.DontCare);
            commands.ClearRenderTarget(false, false, UnityEngine.Color.clear);
            UnityEngine.RenderTexture.ReleaseTemporary(surface);
        }

      #if UNITY_EDITOR
        // When a texture is modified and reimported its native pointer changes, so we need to
        // send the new texture native pointer to C++ to update texture provider cache
        NoesisTextureProvider.instance.UpdateTextures();
      #endif

      #if UNITY_2022_2_OR_NEWER
        commands.IssuePluginEventAndDataWithFlags(_renderOffscreenCallback, (int)EventId.RenderOffscreen,
            invalidate ? CustomMarkerCallbackForceInvalidateStateTracking : CustomMarkerCallbackDefault,
            view.Renderer.CPtr.Handle);
      #else
        commands.IssuePluginEventAndData(_renderOffscreenCallback, (int)EventId.RenderOffscreen, view.Renderer.CPtr.Handle);
        if (invalidate) InvalidateState(commands);
      #endif
    }

    private static void FixCommandBuffer(UnityEngine.Rendering.CommandBuffer commands)
    {
        // This is a workaround for a bug in Unity. When rendering nothing Unity sends us an empty command buffer
        if (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal ||
            UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.PlayStation4 ||
            UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.PlayStation5)
        {
            commands.DrawMesh(GetDummyMesh(), new UnityEngine.Matrix4x4(), GetDummyMaterial());
        }
    }

    /// <summary>
    /// Sends render commands to native code
    /// </summary>
    public static void RenderOnscreen(Noesis.View view, bool flipY, UnityEngine.Rendering.CommandBuffer commands,
        bool invalidate, bool clearStencil)
    {
        FixCommandBuffer(commands);

        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;

      #if UNITY_2022_2_OR_NEWER
        commands.IssuePluginEventAndDataWithFlags(_renderOnscreenCallback, (int)eventId,
            invalidate ? CustomMarkerCallbackForceInvalidateStateTracking : CustomMarkerCallbackDefault,
            view.Renderer.CPtr.Handle);
      #else
        commands.IssuePluginEventAndData(_renderOnscreenCallback, (int)eventId, view.Renderer.CPtr.Handle);
        if (invalidate) InvalidateState(commands);
      #endif
    }

  #if ENABLE_URP_PACKAGE_RENDER_GRAPH
    public static void RenderOnscreen_(Noesis.View view, bool flipY, UnityEngine.Rendering.RasterCommandBuffer commands,
        bool invalidate, bool clearStencil)
    {
        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;
        commands.IssuePluginEventAndData(_renderOnscreenCallback, (int)eventId, view.Renderer.CPtr.Handle);
        if (invalidate) InvalidateState(commands);
    }

    public static void RenderOnscreen_(Noesis.View view, bool flipY, UnityEngine.Rendering.UnsafeCommandBuffer commands,
        bool invalidate, bool clearStencil)
    {
        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;
        commands.IssuePluginEventAndData(_renderOnscreenCallback, (int)eventId, view.Renderer.CPtr.Handle);
        if (invalidate) InvalidateState(commands);
    }
  #endif

    [StructLayoutAttribute(LayoutKind.Sequential)]
    private struct ViewProj
    {
        public System.IntPtr renderer;
        public Noesis.Matrix4 projection;
    };

    public static void RenderOnscreen(Noesis.View view_, Noesis.Matrix4 projection_, bool flipY,
        UnityEngine.Rendering.CommandBuffer commands, bool invalidate, bool clearStencil)
    {
        FixCommandBuffer(commands);

        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var data = new ViewProj() { renderer = view_.Renderer.CPtr.Handle, projection = projection_ };
        System.IntPtr ptr = Noesis_AllocateProj();
        Marshal.StructureToPtr(data, ptr, false);

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;

      #if UNITY_2022_2_OR_NEWER
        commands.IssuePluginEventAndDataWithFlags(_renderOnscreenMtxCallback, (int)eventId,
            invalidate ? CustomMarkerCallbackForceInvalidateStateTracking : CustomMarkerCallbackDefault,
            ptr);
      #else
        commands.IssuePluginEventAndData(_renderOnscreenMtxCallback, (int)eventId, ptr);
        if (invalidate) InvalidateState(commands);
      #endif
    }

  #if ENABLE_URP_PACKAGE_RENDER_GRAPH
    public static void RenderOnscreen_(Noesis.View view_, Noesis.Matrix4 projection_, bool flipY,
        UnityEngine.Rendering.RasterCommandBuffer commands, bool invalidate, bool clearStencil)
    {
        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var data = new ViewProj() { renderer = view_.Renderer.CPtr.Handle, projection = projection_ };
        System.IntPtr ptr = Noesis_AllocateProj();
        Marshal.StructureToPtr(data, ptr, false);

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;
        commands.IssuePluginEventAndData(_renderOnscreenMtxCallback, (int)eventId, ptr);
        if (invalidate) InvalidateState(commands);
    }

    public static void RenderOnscreen_(Noesis.View view_, Noesis.Matrix4 projection_, bool flipY,
        UnityEngine.Rendering.UnsafeCommandBuffer commands, bool invalidate, bool clearStencil)
    {
        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var data = new ViewProj() { renderer = view_.Renderer.CPtr.Handle, projection = projection_ };
        System.IntPtr ptr = Noesis_AllocateProj();
        Marshal.StructureToPtr(data, ptr, false);

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;
        commands.IssuePluginEventAndData(_renderOnscreenMtxCallback, (int)eventId, ptr);
        if (invalidate) InvalidateState(commands);
    }
  #endif

    [StructLayoutAttribute(LayoutKind.Sequential)]
    private struct ViewStereoProj
    {
        public System.IntPtr renderer;
        public Noesis.Matrix4 projection;
        public Noesis.Matrix4 leftEyeProjection;
        public Noesis.Matrix4 rightEyeProjection;
    };

    public static void RenderOnscreen(Noesis.View view_, Noesis.Matrix4 projection_, Noesis.Matrix4 leftEyeProjection_,
        Noesis.Matrix4 rightEyeProjection_, bool flipY, UnityEngine.Rendering.CommandBuffer commands, bool invalidate,
        bool clearStencil)
    {
        FixCommandBuffer(commands);

        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var data = new ViewStereoProj() { renderer = view_.Renderer.CPtr.Handle, projection = projection_, leftEyeProjection = leftEyeProjection_, rightEyeProjection = rightEyeProjection_ };
        System.IntPtr ptr = Noesis_AllocateStereoProj();
        Marshal.StructureToPtr(data, ptr, false);

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;

      #if UNITY_2022_2_OR_NEWER
        commands.IssuePluginEventAndDataWithFlags(_renderOnscreenMtxStereoCallback, (int)eventId,
            invalidate ? CustomMarkerCallbackForceInvalidateStateTracking : CustomMarkerCallbackDefault,
            ptr);
      #else
        commands.IssuePluginEventAndData(_renderOnscreenMtxStereoCallback, (int)eventId, ptr);
        if (invalidate) InvalidateState(commands);
      #endif
    }

  #if ENABLE_URP_PACKAGE_RENDER_GRAPH
    public static void RenderOnscreen_(Noesis.View view_, Noesis.Matrix4 projection_, Noesis.Matrix4 leftEyeProjection_,
        Noesis.Matrix4 rightEyeProjection_, bool flipY, UnityEngine.Rendering.RasterCommandBuffer commands, bool invalidate,
        bool clearStencil)
    {
        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var data = new ViewStereoProj() { renderer = view_.Renderer.CPtr.Handle, projection = projection_, leftEyeProjection = leftEyeProjection_, rightEyeProjection = rightEyeProjection_ };
        System.IntPtr ptr = Noesis_AllocateStereoProj();
        Marshal.StructureToPtr(data, ptr, false);

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;
        commands.IssuePluginEventAndData(_renderOnscreenMtxStereoCallback, (int)eventId, ptr);
        if (invalidate) InvalidateState(commands);
    }

    public static void RenderOnscreen_(Noesis.View view_, Noesis.Matrix4 projection_, Noesis.Matrix4 leftEyeProjection_,
        Noesis.Matrix4 rightEyeProjection_, bool flipY, UnityEngine.Rendering.UnsafeCommandBuffer commands, bool invalidate,
        bool clearStencil)
    {
        if (clearStencil)
        {
            commands.ClearRenderTarget(UnityEngine.Rendering.RTClearFlags.Stencil, UnityEngine.Color.black, 1.0f, 0);
        }

        var data = new ViewStereoProj() { renderer = view_.Renderer.CPtr.Handle, projection = projection_, leftEyeProjection = leftEyeProjection_, rightEyeProjection = rightEyeProjection_ };
        System.IntPtr ptr = Noesis_AllocateStereoProj();
        Marshal.StructureToPtr(data, ptr, false);

        var eventId = flipY ? EventId.RenderOnscreenFlipY : EventId.RenderOnscreen;
        commands.IssuePluginEventAndData(_renderOnscreenMtxStereoCallback, (int)eventId, ptr);
        if (invalidate) InvalidateState(commands);
    }
  #endif

    /// <summary>
    /// CommandBuffer equivalent to GL.InvalidateState
    /// </summary>
    private static void InvalidateState(UnityEngine.Rendering.CommandBuffer commands)
    {
        // There is nothing equivalent to GL.InvalidateState() for the command buffer
        // But as IssuePluginEvent() invalidates the state, invoking an empty C++ callback gives the same effect
        // Note that IssuePluginEventAndData() does not invalidate the state
        // D3D12 and Vulkan use ConfigureEvent via IUnityGraphics and this is not needed
        if (UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Direct3D12 &&
            UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
        {
            commands.IssuePluginEvent(_invalidateStateCallback, (int)EventId.NoRender);
        }
    }

  #if ENABLE_URP_PACKAGE_RENDER_GRAPH
    private static void InvalidateState(UnityEngine.Rendering.RasterCommandBuffer commands)
    {
        // There is nothing equivalent to GL.InvalidateState() for the command buffer
        // But as IssuePluginEvent() invalidates the state, invoking an empty C++ callback gives the same effect
        // Note that IssuePluginEventAndData() does not invalidate the state
        // D3D12 and Vulkan use ConfigureEvent via IUnityGraphics and this is not needed
        if (UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Direct3D12 &&
            UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
        {
            commands.IssuePluginEvent(_invalidateStateCallback, (int)EventId.NoRender);
        }
    }

    private static void InvalidateState(UnityEngine.Rendering.UnsafeCommandBuffer commands)
    {
        // There is nothing equivalent to GL.InvalidateState() for the command buffer
        // But as IssuePluginEvent() invalidates the state, invoking an empty C++ callback gives the same effect
        // Note that IssuePluginEventAndData() does not invalidate the state
        // D3D12 and Vulkan use ConfigureEvent via IUnityGraphics and this is not needed
        if (UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Direct3D12 &&
            UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
        {
            commands.IssuePluginEvent(_invalidateStateCallback, (int)EventId.NoRender);
        }
    }
  #endif

    private static string ActiveShaderLang()
    {
        var type = UnityEngine.SystemInfo.graphicsDeviceType;

        if (type == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11) return "hlsl";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12) return "hlsl";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore) return "glsl";
      #if !UNITY_2023_1_OR_NEWER
        if (type == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2) return "essl";
      #endif
        if (type == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3) return "essl";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.Vulkan) return "spirv";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.Metal) return "mtl";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.PlayStation4) return "pssl_orbis";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.PlayStation5) return "pssl_prospero";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.Switch) return "nvn";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.GameCoreXboxOne) return "hlsl";
        if (type == UnityEngine.Rendering.GraphicsDeviceType.GameCoreXboxSeries) return "hlsl";

        return "";
    }

    private static uint StrHash(string str)
    {
        uint result = 2166136261;

        foreach (char c in str)
        {
            result = (result * 16777619) ^ c;
        }

        return result;
    }

    /// <summary>
    /// Creates a custom pixel shader
    /// </summary>
    public static void CreatePixelShader(NoesisShader shader)
    {
        if (shader.code != null && shader.code.Length > 0)
        {
            var stream = new System.IO.MemoryStream(shader.code);
            var reader = new System.IO.BinaryReader(stream);

            while (stream.Position < stream.Length)
            {
                uint id = (uint)reader.ReadInt32();
                int size = reader.ReadInt32();

                if (id == StrHash(ActiveShaderLang()))
                {
                    byte[] label = System.Text.Encoding.ASCII.GetBytes(shader.label);

                    System.IntPtr data = Noesis_AllocateNative(4 * 4 + size + label.Length);
                    int[] args = new int[] { shader.type, size, label.Length, 0 };
                    Marshal.Copy(args, 0, data, 4);
                    Marshal.Copy(shader.code, (int)stream.Position, data + 16, size);
                    Marshal.Copy(label, 0, data + 16 + size, label.Length);

                    if (_nextShaderId == 0)
                    {
                        // At domain reload _nextShaderId is reset, let's keep in sync with the render thread
                        _commands.IssuePluginEventAndData(_renderClearShadersCallback, (int)EventId.NoRender, System.IntPtr.Zero);
                    }

                    _commands.IssuePluginEventAndData(_renderCreateShaderCallback, (int)EventId.NoRender, data);
                    UnityEngine.Graphics.ExecuteCommandBuffer(_commands);
                    _commands.Clear();

                    if (shader.type == 1)
                    {
                        shader.brush_path = (System.IntPtr)(++_nextShaderId);
                        shader.brush_path_aa = (System.IntPtr)(++_nextShaderId);
                        shader.brush_sdf = (System.IntPtr)(++_nextShaderId);
                        shader.brush_opacity = (System.IntPtr)(++_nextShaderId);
                    }
                    else
                    {
                        shader.effect = (System.IntPtr)(++_nextShaderId);
                    }

                    break;
                }

                stream.Seek(size, System.IO.SeekOrigin.Current);
            }
        }
    }

    /// <summary>
    /// Unregisters given renderer
    /// </summary>
    public static void UnregisterView(Noesis.View view, UnityEngine.Rendering.CommandBuffer commands)
    {
        commands.IssuePluginEventAndData(_renderUnregisterCallback, (int)EventId.NoRender, view.Renderer.CPtr.Handle);
    }

    /// <summary>
    /// Updates render settings
    /// </summary>
    public static void SetRenderSettings()
    {
        NoesisSettings settings = NoesisSettings.Get();

        bool linearRendering = false;

        switch (settings.linearRendering)
        {
            case NoesisSettings.LinearRendering._SamesAsUnity:
            {
                linearRendering = UnityEngine.QualitySettings.activeColorSpace == UnityEngine.ColorSpace.Linear;
                break;
            }
            case NoesisSettings.LinearRendering._Enabled:
            {
                linearRendering = true;
                break;
            }
            case NoesisSettings.LinearRendering._Disabled:
            {
                linearRendering = false;
                break;
            }
        }

        int sampleCount = 1;

        switch (settings.offscreenSampleCount)
        {
            case NoesisSettings.OffscreenSampleCount._SameAsUnity:
            {
                sampleCount = UnityEngine.QualitySettings.antiAliasing;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._1x:
            {
                sampleCount = 1;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._2x:
            {
                sampleCount = 2;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._4x:
            {
                sampleCount = 4;
                break;
            }
            case NoesisSettings.OffscreenSampleCount._8x:
            {
                sampleCount = 8;
                break;
            }
        }

        uint offscreenDefaultNumSurfaces = settings.offscreenInitSurfaces;
        uint offscreenMaxNumSurfaces = settings.offscreenMaxSurfaces;

        int glyphCacheTextureWidth = 1024;
        int glyphCacheTextureHeight = 1024;

        switch (settings.glyphTextureSize)
        {
            case NoesisSettings.TextureSize._256x256:
            {
                glyphCacheTextureWidth = 256;
                glyphCacheTextureHeight = 256;
                break;
            }
            case NoesisSettings.TextureSize._512x512:
            {
                glyphCacheTextureWidth = 512;
                glyphCacheTextureHeight = 512;
                break;
            }
            case NoesisSettings.TextureSize._1024x1024:
            {
                glyphCacheTextureWidth = 1024;
                glyphCacheTextureHeight = 1024;
                break;
            }
            case NoesisSettings.TextureSize._2048x2048:
            {
                glyphCacheTextureWidth = 2048;
                glyphCacheTextureHeight = 2048;
                break;
            }
            case NoesisSettings.TextureSize._4096x4096:
            {
                glyphCacheTextureWidth = 4096;
                glyphCacheTextureHeight = 4096;
                break;
            }
        }

        Noesis_RendererSettings(linearRendering, sampleCount, offscreenDefaultNumSurfaces,
            offscreenMaxNumSurfaces, glyphCacheTextureWidth, glyphCacheTextureHeight);
    }

    #region Private
    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderRegisterCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetUpdateRenderTreeCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderOffscreenCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderOnscreenCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderOnscreenMtxCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderOnscreenMtxStereoCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetInvalidateStateCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_AllocateNative(int size);

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_AllocateProj();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_AllocateStereoProj();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderClearShadersCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderCreateShaderCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern System.IntPtr Noesis_GetRenderUnregisterCallback();

    [DllImport(Noesis.Library.Name)]
    private static extern void Noesis_RendererSettings(bool linearSpaceRendering, int offscreenSampleCount,
        uint offscreenDefaultNumSurfaces, uint offscreenMaxNumSurfaces, int glyphCacheTextureWidth, int glyphCacheTextureHeight);

    // Keep this in sync with UnityDevice in C++
    private enum EventId
    {
        NoRender = 0x6446,
        RenderOffscreen = 0x6447,
        RenderOnscreen = 0x6448,
        RenderOnscreenFlipY = 0x6449
    }

    private static uint _nextShaderId;
    private static UnityEngine.Rendering.CommandBuffer _commands = new UnityEngine.Rendering.CommandBuffer();
    private static System.IntPtr _renderRegisterCallback = Noesis_GetRenderRegisterCallback();
    private static System.IntPtr _updateRenderTreeCallback = Noesis_GetUpdateRenderTreeCallback();
    private static System.IntPtr _renderOffscreenCallback = Noesis_GetRenderOffscreenCallback();
    private static System.IntPtr _renderOnscreenCallback = Noesis_GetRenderOnscreenCallback();
    private static System.IntPtr _renderOnscreenMtxCallback = Noesis_GetRenderOnscreenMtxCallback();
    private static System.IntPtr _renderOnscreenMtxStereoCallback = Noesis_GetRenderOnscreenMtxStereoCallback();
    private static System.IntPtr _invalidateStateCallback = Noesis_GetInvalidateStateCallback();
    private static System.IntPtr _renderClearShadersCallback = Noesis_GetRenderClearShadersCallback();
    private static System.IntPtr _renderCreateShaderCallback = Noesis_GetRenderCreateShaderCallback();
    private static System.IntPtr _renderUnregisterCallback = Noesis_GetRenderUnregisterCallback();

    private static UnityEngine.Material _dummyMaterial;
    private static UnityEngine.Mesh _dummyMesh;

    private static UnityEngine.Material GetDummyMaterial()
    {
        if (_dummyMaterial == null)
        {
            _dummyMaterial = new UnityEngine.Material(UnityEngine.Shader.Find("UI/Default"));
        }
        return _dummyMaterial;
    }

    private static UnityEngine.Mesh GetDummyMesh()
    {
        if (_dummyMesh == null)
        {
            _dummyMesh = new UnityEngine.Mesh();
            _dummyMesh.vertices = new UnityEngine.Vector3[3];
            _dummyMesh.vertices[0] = new UnityEngine.Vector3(0, 0, 0);
            _dummyMesh.vertices[1] = new UnityEngine.Vector3(0, 0, 0);
            _dummyMesh.vertices[2] = new UnityEngine.Vector3(0, 0, 0);
            _dummyMesh.triangles = new int[3] { 0, 2, 1 };
        }
        return _dummyMesh;
    }

    #endregion
}