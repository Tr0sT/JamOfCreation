#define IGNORE_ISHITTESTVISIBLE_FALSE_ELEMENTS

using UnityEngine;
using Noesis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using UnityEngine.XR;

#if ENABLE_URP_PACKAGE
using UnityEngine.Rendering.Universal;
#endif

#if ENABLE_URP_PACKAGE_RENDER_GRAPH
using UnityEngine.Rendering.RenderGraphModule;
#endif

#if ENABLE_HDRP_PACKAGE
using UnityEngine.Rendering.HighDefinition;
#endif

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

//[ExecuteInEditMode]
[AddComponentMenu("NoesisGUI/Noesis View")]
[HelpURL("https://www.noesisengine.com/docs")]
[DisallowMultipleComponent]
public class NoesisView: MonoBehaviour, ISerializationCallbackReceiver
{
    #region Public properties

    /// <summary>
    /// User interface definition XAML
    /// </summary>
    public NoesisXaml Xaml
    {
        set { this._xaml = value; }
        get { return this._xaml; }
    }

    /// <summary>
    /// The texture to render this View into
    /// </summary>
    public RenderTexture Texture
    {
        set { this._texture = value; }
        get { return this._texture; }
    }

    /// <summary>
    /// Tessellation curve tolerance in screen space. 'Medium Quality' is usually fine for PPAA (non-multisampled)
    /// while 'High Quality' is the recommended pixel error if you are rendering to a 8x multisampled surface
    /// </summary>
    public float TessellationMaxPixelError
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetTessellationMaxPixelError(value);
            }

            this._tessellationMaxPixelError = value;
        }

        get
        {
            if (_uiView != null)
            {
                return _uiView.GetTessellationMaxPixelError().Error;
            }

            return this._tessellationMaxPixelError; 
        }
    }

    /// <summary>
    /// Bit flags used for debug rendering purposes.
    /// </summary>
    public RenderFlags RenderFlags
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetFlags(value);
            }

            this._renderFlags = value;
        }
        get
        {
            if (_uiView != null)
            {
                return _uiView.GetFlags();
            }

            return this._renderFlags;
        }
    }

    /// <summary>
    /// When enabled, the UI is positioned in the world among other objects in the Scene
    /// </summary>
    public bool WorldSpace
    {
        get { return (RenderFlags & RenderFlags.DepthTesting) > 0; }
    }

    /// <summary>
    /// The projection matrix set to the view is used for determining the visual impact of nodes
    /// in the offscreen phase. The stereo matrices used for rendering in VR are slightly different.
    /// To account for this difference, it is recommended to apply a scale using this property.
    /// For non-VR this must be always 1. For VR, we recommend a value between 2 and 3.
    /// </summary>
    public float StereoOffscreenScaleFactor
    {
        set { this._stereoScale = value; }
        get { return this._stereoScale; }
    }

    /// <summary>
    /// When enabled, the view is scaled by the actual DPI of the screen or physical device running the application
    /// </summary>
    public bool DPIScale
    {
        set { this._dpiScale = value; }
        get { return this._dpiScale; }
    }

    /// <summary>
    /// When continuous rendering is disabled, rendering only happens when UI changes. For performance
    /// purposes and to save battery this is the default mode when rendering to texture. If not rendering
    /// to texture, this property is ignored. Use the property 'NeedsRendering' instead.
    /// </summary>
    public bool ContinuousRendering
    {
        set { this._continuousRendering = value; }
        get { return this._continuousRendering; }
    }

    /// <summary>
    /// When enabled, the view must be explicitly updated by calling 'ExternalUpdate()'.
    /// By default, the view is automatically updated during LateUpdate.
    /// </summary>
    public bool EnableExternalUpdate
    {
        set { this._enableExternalUpdate = value; }
        get { return this._enableExternalUpdate; }
    }

    /// <summary>
    /// After updating the view, this flag indicates if the GUI needs to be repainted.
    /// This flag can be used on manually painted cameras to optimize performance and save battery.
    /// </summary>
    public bool NeedsRendering
    {
        set { this._needsRendering = value; }
        get { return this._needsRendering; }
    }

    /// <summary>
    /// Enables keyboard input management.
    /// </summary>
    public bool EnableKeyboard
    {
        set { this._enableKeyboard = value; }
        get { return this._enableKeyboard; }
    }

    /// <summary>
    /// Enables mouse input management.
    /// </summary>
    public bool EnableMouse
    {
        set { this._enableMouse = value; }
        get { return this._enableMouse; }
    }

    /// <summary>
    /// Enables touch input management.
    /// </summary>
    public bool EnableTouch
    {
        set { this._enableTouch = value; }
        get { return this._enableTouch; }
    }

    /// <summary>
    /// Enables actions input management.
    /// </summary>
    public bool EnableActions
    {
        get { return _enableActions; }
        set
        {
            if (_enableActions != value)
            {
                _enableActions = value;
                ReloadActions(_actions, _actionMap, _actionsBound);
            }
        }
    }

    /// <summary>
    /// Input System actions.
    /// </summary>
    public UnityEngine.InputSystem.InputActionAsset Actions
    {
        get { return _actions; }
        set
        {
            if (_actions != value)
            {
                ReloadActions(value, _actionMap, _actionsBound);
            }
        }
    }

    /// <summary>
    /// Set of actions being used by this view and enabled by default.
    /// </summary>
    public string ActionMap
    {
        get { return _actionMap; }
        set
        {
            if (_actionMap != value)
            {
                ReloadActions(_actions, value, _actionsBound);
            }
        }
    }

    /// <summary>
    /// The initial delay (in seconds) between an initial button action and a repeated action.
    /// </summary>
    public float ActionsRepeatDelay
    {
        set { this._actionsRepeatDelay = value; }
        get { return this._actionsRepeatDelay; }
    }

    /// <summary>
    /// The speed (in seconds) that the button action repeats itself once repeating.
    /// </summary>
    public float ActionsRepeatRate
    {
        set { this._actionsRepeatRate = value; }
        get { return this._actionsRepeatRate; }
    }

    /// <summary>
    /// Transform representing the real world origin for tracking devices
    /// </summary>
    public UnityEngine.Transform XRTrackingOrigin
    {
        set { this._xrTrackingOrigin = value; }
        get { return this._xrTrackingOrigin; }
    }

  #if ENABLE_URP_PACKAGE
    /// <summary>
    /// Controls when the UI render pass executes
    /// </summary>
    public RenderPassEvent RenderPassEvent
    {
        set { this._renderPassEvent = value; }
        get { return this._renderPassEvent; }
    }
  #endif

  #if ENABLE_HDRP_PACKAGE
    /// <summary>
    /// Controls when the UI render pass executes
    /// </summary>
    public CustomPassInjectionPoint InjectionPoint
    {
        set { this._injectionPoint = value; }
        get { return this._injectionPoint; }
    }
  #endif

    /// <summary>
    /// Emulate touch input with mouse.
    /// </summary>
    public bool EmulateTouch
    {
        set
        {
            if (_uiView != null)
            {
                _uiView.SetEmulateTouch(value);
            }

            this._emulateTouch = value;
        }
        get { return this._emulateTouch; }
    }

    /// <summary>
    /// When enabled, UI is updated using Time.realtimeSinceStartup.
    /// </summary>
    public bool UseRealTimeClock
    {
        set { this._useRealTimeClock = value; }
        get { return this._useRealTimeClock; }
    }

    /// <summary>
    /// When enabled, then stencil buffer is cleared before rendering this view
    /// </summary>
    public bool ClearStencil
    {
        set { this._clearStencil = value; }
        get { return this._clearStencil; }
    }

    /// <summary>
    /// Gets the root of the loaded Xaml.
    /// </summary>
    /// <returns>Root element.</returns>
    public FrameworkElement Content
    {
        get { return _uiView != null ? _uiView.Content : null; }
    }

    /// <summary>
    /// Indicates if this component is rendering UI to a RenderTexture.
    /// </summary>
    /// <returns></returns>
    public bool IsRenderToTexture()
    {
        return !gameObject.TryGetComponent(out Camera _);
    }

    #endregion

    #region Public events

    #region Render
    public event RenderingEventHandler Rendering
    {
        add
        {
            if (_uiView != null)
            {
                _uiView.Rendering += value;
            }
        }
        remove
        {
            if (_uiView != null)
            {
                _uiView.Rendering -= value;
            }
        }
    }

    public ViewStats GetStats()
    {
        if (_uiView != null)
        {
            return _uiView.GetStats();
        }

        return new ViewStats();
    }
    #endregion

    #region Keyboard input events
    /// <summary>
    /// Notifies Renderer that a key was pressed.
    /// </summary>
    /// <param name="key">Key identifier.</param>
    public bool KeyDown(Noesis.Key key)
    {
        if (_uiView != null)
        {
            return _uiView.KeyDown(key);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a key was released.
    /// </summary>
    /// <param name="key">Key identifier.</param>
    public bool KeyUp(Noesis.Key key)
    {
        if (_uiView != null)
        {
            return _uiView.KeyUp(key);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a key was translated to the corresponding character.
    /// </summary>
    /// <param name="ch">Unicode character value.</param>
    public bool Char(uint ch)
    {
        if (_uiView != null)
        {
            return _uiView.Char(ch);
        }

        return false;
    }
    #endregion

    #region Mouse input events
    /// <summary>
    /// Notifies Renderer that mouse was moved. The mouse position is specified in renderer
    /// surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    public bool MouseMove(int x, int y)
    {
        if (_uiView != null)
        {
            return _uiView.MouseMove(x, y);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a mouse button was pressed. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was pressed.</param>
    public bool MouseButtonDown(int x, int y, Noesis.MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseButtonDown(x, y, button);
        }

        return false;
    }

    /// Notifies Renderer that a mouse button was released. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was released.</param>
    public bool MouseButtonUp(int x, int y, Noesis.MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseButtonUp(x, y, button);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer of a mouse button double click. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="button">Indicates which button was pressed.</param>
    public bool MouseDoubleClick(int x, int y, Noesis.MouseButton button)
    {
        if (_uiView != null)
        {
            return _uiView.MouseDoubleClick(x, y, button);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that mouse wheel was rotated. The mouse position is specified in
    /// renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Mouse x-coordinate.</param>
    /// <param name="y">Mouse y-coordinate.</param>
    /// <param name="wheelRotation">Indicates the amount mouse wheel has changed.</param>
    public bool MouseWheel(int x, int y, int wheelRotation)
    {
        if (_uiView != null)
        {
            return _uiView.MouseWheel(x, y, wheelRotation);
        }

        return false;
    }
    #endregion

    #region Touch input events
    /// <summary>
    /// Notifies Renderer that a finger is moving on the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchMove(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchMove(x, y, touchId);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a finger touches the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchDown(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchDown(x, y, touchId);
        }

        return false;
    }

    /// <summary>
    /// Notifies Renderer that a finger is raised off the screen. The finger position is
    /// specified in renderer surface pixel coordinates.
    /// </summary>
    /// <param name="x">Finger x-coordinate.</param>
    /// <param name="y">Finger y-coordinate.</param>
    /// <param name="touchId">Finger identifier.</param>
    public bool TouchUp(int x, int y, uint touchId)
    {
        if (_uiView != null)
        {
            return _uiView.TouchUp(x, y, touchId);
        }

        return false;
    }
    #endregion

    #endregion

    #region Public methods

    /// <summary>
    /// Loads the user interface specified in the XAML property
    /// </summary>
    public void LoadXaml(bool force)
    {
        if (force)
        {
            DestroyView();
        }

        if (_xaml != null && _uiView == null)
        {
            object obj = _xaml.Load();

            if (obj != null)
            {
                FrameworkElement content = obj as FrameworkElement;

                if (content != null)
                {
                    CreateView(content);
                }
                else
                {
                    Debug.LogError($"{_xaml.uri}: Root type '{obj.GetType().Name}' does not inherit from 'FrameworkElement'");
                }
            }
        }
    }

    #endregion

    #region Private members

    #region MonoBehavior component messages

    /// <summary>
    /// Called once when component is attached to GameObject for the first time
    /// </summary>
    void Reset()
    {
        _isPPAAEnabled = true;
        _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
        _renderFlags = 0;
        _dpiScale = true;
        _continuousRendering = gameObject.TryGetComponent(out Camera _);
        _enableExternalUpdate = false;
        _enableKeyboard = true;
        _enableMouse = true;
        _enableTouch = true;
        _enableActions = false;
        _emulateTouch = false;
        _useRealTimeClock = false;
        _clearStencil = false;
        _actionMap = "Gamepad";
        _actionsRepeatDelay = 0.5f;
        _actionsRepeatRate = 0.1f;
      #if ENABLE_URP_PACKAGE
        _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
      #endif
      #if ENABLE_HDRP_PACKAGE
        _injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
      #endif
    }

    void Start()
    {
        // https://forum.unity.com/threads/gc-collect-in-guiutility-begingui.642229/
        // Avoid OnGUI GC Allocations
        useGUILayout = false;

      #if !ENABLE_INPUT_SYSTEM
        if (_enableActions)
        {
            Debug.LogWarning("Actions enabled requires 'Active Input Handling' set to 'New' or 'Both' in Player Settings");
            _enableActions = false;
        }
      #endif
    }

    private void ReloadActions(UnityEngine.InputSystem.InputActionAsset actions, string actionMap,
        bool actionsBound)
    {
        if (actionsBound)
        {
            UnbindActions();
        }

        _actions = actions;
        _actionMap = actionMap;

        if (actionsBound)
        {
            BindActions();
        }
    }

    private bool _actionMapEnabled = false;
    private bool _actionsBound = false;

    private void BindActions()
    {
        var actionMap = _actions?.FindActionMap(_actionMap ?? "");

        if (actionMap != null)
        {
            if (_enableActions && !actionMap.enabled)
            {
                actionMap.Enable();
                _actionMapEnabled = true;
            }

            _upAction = actionMap.FindAction("Up");
            _downAction = actionMap.FindAction("Down");
            _leftAction = actionMap.FindAction("Left");
            _rightAction = actionMap.FindAction("Right");

            _acceptAction = actionMap.FindAction("Accept");
            _cancelAction = actionMap.FindAction("Cancel");

            _menuAction = actionMap.FindAction("Menu");
            _viewAction = actionMap.FindAction("View");

            _pageLeftAction = actionMap.FindAction("PageLeft");
            _pageRightAction = actionMap.FindAction("PageRight");
            _pageUpAction = actionMap.FindAction("PageUp");
            _pageDownAction = actionMap.FindAction("PageDown");
            _scrollAction = actionMap.FindAction("Scroll");

            _context1Action = actionMap.FindAction("Context1");
            _context2Action = actionMap.FindAction("Context2");
            _context3Action = actionMap.FindAction("Context3");
            _context4Action = actionMap.FindAction("Context4");

            _trackedPositionAction = actionMap.FindAction("TrackedPosition");
            _trackedRotationAction = actionMap.FindAction("TrackedRotation");
            _trackedTriggerAction = actionMap.FindAction("TrackedTrigger");
        }

        _actionsBound = true;
    }

    private void UnbindActions()
    {
        var actionMap = _actions?.FindActionMap(_actionMap ?? "");

        if (actionMap != null)
        {
            if (_actionMapEnabled)
            {
                actionMap.Disable();
                _actionMapEnabled = false;
            }

            _upAction = default;
            _downAction = default;
            _leftAction = default;
            _rightAction = default;

            _acceptAction = default;
            _cancelAction = default;

            _menuAction = default;
            _viewAction = default;

            _pageLeftAction = default;
            _pageRightAction = default;
            _pageUpAction = default;
            _pageDownAction = default;
            _scrollAction = default;

            _context1Action = default;
            _context2Action = default;
            _context3Action = default;
            _context4Action = default;

            _trackedPositionAction = default;
            _trackedRotationAction = default;
            _trackedTriggerAction = default;
        }

        _actionsBound = false;
    }

    private CommandBuffer _commands;

    private void EnsureCommandBuffer()
    {
        if (_commands == null)
        {
            _commands = new CommandBuffer();
        }
    }

    void Awake()
    {
        EnsureCommandBuffer();
    }

    private Camera _camera;

    void OnEnable()
    {
        EnsureCommandBuffer();
        TryGetComponent<Camera>(out _camera);

      #if !ENABLE_LEGACY_INPUT_MANAGER
        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        }
      #endif

        BindActions();
        LoadXaml(false);

        Camera.onPreRender += PreRender;

      #if ENABLE_URP_PACKAGE || ENABLE_HDRP_PACKAGE
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;

        #if ENABLE_URP_PACKAGE
          _scriptableRenderPass = new NoesisScriptableRenderPass(this);
        #endif
      #endif
    }

    void OnDisable()
    {
      #if !ENABLE_LEGACY_INPUT_MANAGER
        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
        }
      #endif

        UnbindActions();

        Camera.onPreRender -= PreRender;

      #if ENABLE_URP_PACKAGE || ENABLE_HDRP_PACKAGE
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= EndCameraRendering;
      #endif
    }

    private static class Profiling
    {
        public static readonly CustomSampler UpdateSampler = CustomSampler.Create("Noesis.Update");
        public static readonly string RegisterView = "Noesis.RegisterView";
        public static readonly string UnregisterView = "Noesis.UnregisterView";
        public static readonly string UpdateRenderTree = "Noesis.UpdateRenderTree";
        public static readonly string RenderOffScreen = "Noesis.RenderOffscreen";
        public static readonly string RenderOnScreen = "Noesis.RenderOnscreen";
        public static readonly string RenderTexture = "Noesis.RenderTexture";
    }

#region Universal Render Pipeline
#if ENABLE_URP_PACKAGE
    private class NoesisScriptableRenderPass: ScriptableRenderPass
    {
        public NoesisScriptableRenderPass(NoesisView view)
        {
            _view = view;
        }

      #if ENABLE_URP_PACKAGE_RENDER_GRAPH
        private class PassData
        {
            public bool flipY;
            public bool clearStencil;
            public NoesisView view;
            public UniversalCameraData cameraData;
        }

        ProfilingSampler _profilingSampler = new ProfilingSampler("Noesis.RenderOnscreen");

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_view._uiView != null && _view._visible)
            {
                // The D3D12 RenderGraph implementation in Unity is still highly unstable.
                // Tested with Unity 6.0.49f1 and 6.1.3f1, both versions crash internally
                // when using 'AddRasterRenderPass'. Even with all Noesis-related code removed,
                // Unity crashes on a simple IssuePluginEvent call. Additionally, macOS Silicon
                // builds (iOS not tested) render a black screen when using UnsafePass.
                // For now, we must support both paths. UnsafePass is expected to be deprecated
                // soon, and this will be revisited accordingly.

                if (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
                {
                    using (var builder = renderGraph.AddUnsafePass<PassData>("Noesis", out var passData, _profilingSampler))
                    {
                        var resourceData = frameData.Get<UniversalResourceData>();
                        var cameraData = frameData.Get<UniversalCameraData>();

                        passData.view = _view;
                        passData.clearStencil = _view._clearStencil;
                        passData.flipY = SystemInfo.graphicsUVStartsAtTop && !resourceData.isActiveTargetBackBuffer;
                        passData.cameraData = cameraData;

                        // Ensure that Unity does not cull this pass
                        builder.AllowPassCulling(false);

                        // These attachments are implicit; setting them appears to be unnecessary
                        //builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.WriteAll);
                        //builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                        builder.SetRenderFunc((PassData passData, UnsafeGraphContext context) =>
                        {
                        #if ENABLE_VR && ENABLE_XR_MODULE
                            if (passData.cameraData.xrRendering)
                            {
                                var cameraData = passData.cameraData;
                                var camera = passData.view._camera;
                                var width = camera.pixelWidth;
                                var height = camera.pixelHeight;

                            #if ENABLE_URP_PACKAGE_VR
                                // CameraData.xr available in URP 14.0+
                                if (cameraData.xr.singlePassEnabled)
                            #else
                                // https://forum.unity.com/threads/detect-single-pass-stereo-on-android-at-runtime.509304/
                                if (XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes)
                            #endif
                                {
                                    Matrix4x4 viewMatrix0 = cameraData.GetViewMatrix(0);
                                    Matrix4x4 projectionMatrix0 = cameraData.GetProjectionMatrix(0);
                                    Noesis.Matrix4 viewProj0 = NoesisMatrix(viewMatrix0, projectionMatrix0, width, height);

                                    Matrix4x4 viewMatrix1 = cameraData.GetViewMatrix(1);
                                    Matrix4x4 projectionMatrix1 = cameraData.GetProjectionMatrix(1);
                                    Noesis.Matrix4 viewProj1 = NoesisMatrix(viewMatrix1, projectionMatrix1, width, height);

                                    NoesisRenderer.RenderOnscreen_(passData.view._uiView, CameraMatrix(camera), viewProj0, viewProj1,
                                        passData.flipY, context.cmd, true, passData.clearStencil);
                                }
                                else
                                {
                                    Matrix4x4 viewMatrix = cameraData.GetViewMatrix(0);
                                    Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(0);
                                    Noesis.Matrix4 viewProj = NoesisMatrix(viewMatrix, projectionMatrix, width, height);
                                    NoesisRenderer.RenderOnscreen_(passData.view._uiView, viewProj, passData.flipY, context.cmd,
                                        true, passData.clearStencil);
                                }
                            }
                            else
                        #endif
                            {
                                NoesisRenderer.RenderOnscreen_(passData.view._uiView, passData.flipY, context.cmd, true,
                                    passData.clearStencil);
                            }
                        });
                    }
                }
                else
                {
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>("Noesis", out var passData, _profilingSampler))
                    {
                        var resourceData = frameData.Get<UniversalResourceData>();
                        var cameraData = frameData.Get<UniversalCameraData>();

                        passData.view = _view;
                        passData.clearStencil = _view._clearStencil;
                        passData.flipY = SystemInfo.graphicsUVStartsAtTop && !resourceData.isActiveTargetBackBuffer;
                        passData.cameraData = cameraData;

                        // Ensure that Unity does not cull this pass
                        builder.AllowPassCulling(false);

                        // These attachments are implicit; setting them appears to be unnecessary
                        //builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.WriteAll);
                        //builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                        builder.SetRenderFunc((PassData passData, RasterGraphContext context) =>
                        {
                        #if ENABLE_VR && ENABLE_XR_MODULE
                            if (passData.cameraData.xrRendering)
                            {
                                var cameraData = passData.cameraData;
                                var camera = passData.view._camera;
                                var width = camera.pixelWidth;
                                var height = camera.pixelHeight;

                            #if ENABLE_URP_PACKAGE_VR
                                // CameraData.xr available in URP 14.0+
                                if (cameraData.xr.singlePassEnabled)
                            #else
                                // https://forum.unity.com/threads/detect-single-pass-stereo-on-android-at-runtime.509304/
                                if (XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes)
                            #endif
                                {
                                    Matrix4x4 viewMatrix0 = cameraData.GetViewMatrix(0);
                                    Matrix4x4 projectionMatrix0 = cameraData.GetProjectionMatrix(0);
                                    Noesis.Matrix4 viewProj0 = NoesisMatrix(viewMatrix0, projectionMatrix0, width, height);

                                    Matrix4x4 viewMatrix1 = cameraData.GetViewMatrix(1);
                                    Matrix4x4 projectionMatrix1 = cameraData.GetProjectionMatrix(1);
                                    Noesis.Matrix4 viewProj1 = NoesisMatrix(viewMatrix1, projectionMatrix1, width, height);

                                    NoesisRenderer.RenderOnscreen_(passData.view._uiView, CameraMatrix(camera), viewProj0, viewProj1,
                                        passData.flipY, context.cmd, true, passData.clearStencil);
                                }
                                else
                                {
                                    Matrix4x4 viewMatrix = cameraData.GetViewMatrix(0);
                                    Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(0);
                                    Noesis.Matrix4 viewProj = NoesisMatrix(viewMatrix, projectionMatrix, width, height);
                                    NoesisRenderer.RenderOnscreen_(passData.view._uiView, viewProj, passData.flipY, context.cmd,
                                        true, passData.clearStencil);
                                }
                            }
                            else
                        #endif
                            {
                                NoesisRenderer.RenderOnscreen_(passData.view._uiView, passData.flipY, context.cmd, true,
                                    passData.clearStencil);
                            }
                        });
                    }
                }
            }
        }
      #endif

      #region Compatibility Mode (URP without Render Graph)
      #pragma warning disable CS0672
      #pragma warning disable CS0618

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_view._uiView != null && _view._visible)
            {
                bool flipY = SystemInfo.graphicsUVStartsAtTop && !IsBackbuffer(_view._camera);
                _view._commands.name = Profiling.RenderOnScreen;

              #if ENABLE_VR && ENABLE_XR_MODULE
                if (renderingData.cameraData.xrRendering)
                {
                    var cameraData = renderingData.cameraData;
                    var camera = _view._camera;
                    var width = camera.pixelWidth;
                    var height = camera.pixelHeight;

                  #if ENABLE_URP_PACKAGE_VR
                    // CameraData.xr available in URP 14.0+
                    if (cameraData.xr.singlePassEnabled)
                  #else
                    // https://forum.unity.com/threads/detect-single-pass-stereo-on-android-at-runtime.509304/
                    if (XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes)
                  #endif
                    {
                        Matrix4x4 viewMatrix0 = cameraData.GetViewMatrix(0);
                        Matrix4x4 projectionMatrix0 = cameraData.GetProjectionMatrix(0);
                        Noesis.Matrix4 viewProj0 = NoesisMatrix(viewMatrix0, projectionMatrix0, width, height);

                        Matrix4x4 viewMatrix1 = cameraData.GetViewMatrix(1);
                        Matrix4x4 projectionMatrix1 = cameraData.GetProjectionMatrix(1);
                        Noesis.Matrix4 viewProj1 = NoesisMatrix(viewMatrix1, projectionMatrix1, width, height);

                        NoesisRenderer.RenderOnscreen(_view._uiView, CameraMatrix(camera), viewProj0, viewProj1,
                            flipY, _view._commands, true, _view._clearStencil);
                    }
                    else
                    {
                        Matrix4x4 viewMatrix = cameraData.GetViewMatrix(0);
                        Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(0);
                        Noesis.Matrix4 viewProj = NoesisMatrix(viewMatrix, projectionMatrix, width, height);
                        NoesisRenderer.RenderOnscreen(_view._uiView, viewProj, flipY, _view._commands, true, _view._clearStencil);
                    }
                }
                else
              #endif
                {
                    NoesisRenderer.RenderOnscreen(_view._uiView, flipY, _view._commands, true, _view._clearStencil);
                }

                context.ExecuteCommandBuffer(_view._commands);
                _view._commands.Clear();
            }
        }

        private bool IsBackbuffer(Camera camera)
        {
            var scriptableRenderer = camera.GetUniversalAdditionalCameraData().scriptableRenderer;

            #if UNITY_2022_1_OR_NEWER
                return camera.targetTexture == null && scriptableRenderer.cameraColorTargetHandle.rt == null;
            #else
                return camera.targetTexture == null && scriptableRenderer.cameraColorTarget == BuiltinRenderTextureType.CameraTarget;
            #endif
        }

      #pragma warning restore CS0672
      #pragma warning restore CS0618
      #endregion

        NoesisView _view;
    }

    private NoesisScriptableRenderPass _scriptableRenderPass;

    private void RenderOffscreenUniversal(ScriptableRenderContext context)
    {
        if (_uiView != null && _visible)
        {
            _commands.name = Profiling.RenderOffScreen;
            NoesisRenderer.RenderOffscreen(_uiView, _commands, true);
            context.ExecuteCommandBuffer(_commands);
            _commands.Clear();
        }
    }

    private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        var cameraData = camera.GetUniversalAdditionalCameraData();

        // To avoid inefficient changes of render target, stacked cameras must render
        // their offscreen phase before the base camera is started
        if (cameraData.renderType == CameraRenderType.Base)
        {
            if (_camera == camera)
            {
                RenderOffscreenUniversal(context);
            }
            else
            {
                foreach (var stackedCamera in cameraData.cameraStack)
                {
                    if (_camera == stackedCamera)
                    {
                        RenderOffscreenUniversal(context);
                        break;
                    }
                }
            }
        }

        if (_camera == camera)
        {
            _scriptableRenderPass.renderPassEvent = _renderPassEvent;
            cameraData.scriptableRenderer.EnqueuePass(_scriptableRenderPass);
        }
    }

    private void EndCameraRendering(ScriptableRenderContext context, Camera camera) {}
#endif
#endregion

#region High Definition Render Pipeline
#if ENABLE_HDRP_PACKAGE
  #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] 
  #endif
    static class RegisterNoesisCustomPass
    {
        [HideInInspector]
        private class NoesisCustomPass: CustomPass
        {
            public CustomPassInjectionPoint InjectionPoint;

            protected override bool executeInSceneView { get { return false; } }

            protected override void Execute(CustomPassContext ctx) 
            {
                if (ctx.hdCamera.camera.TryGetComponent(out NoesisView view))
                {
                    if (InjectionPoint == view.InjectionPoint)
                    {
                        bool flipY = SystemInfo.graphicsUVStartsAtTop && !IsBackbuffer(ctx.cameraColorBuffer);
                        view.OnExecuteCustomPass(ctx.cmd, flipY);
                    }
                }
            }

            private static bool IsBackbuffer(RTHandle buffer)
            {
                var id = new RenderTargetIdentifier(buffer.nameID, 0, CubemapFace.Unknown, 0);
                return id == BuiltinRenderTextureType.CameraTarget;
            }
        }

        static RegisterNoesisCustomPass() => RegisterCustomPasses();

        [RuntimeInitializeOnLoadMethod]
        static void RegisterCustomPasses()
        {
            RegisterCustomPass(CustomPassInjectionPoint.BeforePostProcess);
            RegisterCustomPass(CustomPassInjectionPoint.AfterPostProcess);
        }

        static void RegisterCustomPass(CustomPassInjectionPoint injectionPoint)
        {
            var pass = new NoesisCustomPass
            {
                clearFlags = ClearFlag.Stencil,
                targetColorBuffer = CustomPass.TargetBuffer.Camera,
                targetDepthBuffer = CustomPass.TargetBuffer.Camera,
                InjectionPoint = injectionPoint
            };

            CustomPassVolume.RegisterUniqueGlobalCustomPass(injectionPoint, pass);
        }
    }

    private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (_camera == camera)
        {
            if (_uiView != null && _visible)
            {
                _commands.name = Profiling.RenderOffScreen;
                NoesisRenderer.RenderOffscreen(_uiView, _commands, true);

                context.ExecuteCommandBuffer(_commands);
                _commands.Clear();
            }
        }
    }

    public static readonly CustomSampler _customSampler = CustomSampler.Create("Noesis.RenderOnscreen", true);

    private void OnExecuteCustomPass(CommandBuffer commands, bool flipY)
    {
        if (_uiView != null && _visible)
        {
            commands.BeginSample(_customSampler);
            NoesisRenderer.RenderOnscreen(_uiView, flipY, commands, true, _clearStencil);
            commands.EndSample(_customSampler);
        }
    }

     private void EndCameraRendering(ScriptableRenderContext context, Camera camera) {}
#endif
#endregion

    void OnDestroy()
    {
        DestroyView();
    }

#if ENABLE_UGUI_PACKAGE
    UnityEngine.EventSystems.PointerEventData _pointerData;
#endif

    private UnityEngine.Vector2 ProjectPointer(float x, float y)
    {
        if (_camera != null)
        {
            return new UnityEngine.Vector2(x, UnityEngine.Screen.height - y);
        }
        else if (_texture != null)
        {
            // Project using texture coordinates

          #if ENABLE_UGUI_PACKAGE
            // First try with Unity UI RawImage objects
            UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;

            if (eventSystem != null && eventSystem.IsPointerOverGameObject())
            {
                UnityEngine.Vector2 pos = new UnityEngine.Vector2(x, y);

                if (_pointerData == null)
                {
                    _pointerData = new UnityEngine.EventSystems.PointerEventData(eventSystem)
                    {
                        pointerId = 0,
                        position = pos
                    };
                }
                else
                {
                    _pointerData.Reset();
                }

                _pointerData.delta = pos - _pointerData.position;
                _pointerData.position = pos;

                if (TryGetComponent(out RectTransform rect))
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                        _pointerData.position, _pointerData.pressEventCamera, out pos))
                    {
                        UnityEngine.Vector2 pivot = new UnityEngine.Vector2(
                            rect.pivot.x * rect.rect.width,
                            rect.pivot.y * rect.rect.height);

                        float texCoordX = (pos.x + pivot.x) / rect.rect.width;
                        float texCoordY = (pos.y + pivot.y) / rect.rect.height;

                        float localX = _texture.width * texCoordX;
                        float localY = _texture.height * (1.0f - texCoordY);
                        return new UnityEngine.Vector2(localX, localY);
                    }
                }
            }
          #endif

            // NOTE: A MeshCollider must be attached to the target to obtain valid
            // texture coordinates, otherwise Hit Testing won't work

            UnityEngine.Ray ray = UnityEngine.Camera.main.ScreenPointToRay(new UnityEngine.Vector3(x, y, 0));

            UnityEngine.RaycastHit hit;
            if (UnityEngine.Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    float localX = _texture.width * hit.textureCoord.x;
                    float localY = _texture.height * (1.0f - hit.textureCoord.y);
                    return new UnityEngine.Vector2(localX, localY);
                }
            }

            return new UnityEngine.Vector2(-1, -1);
        }

        return Vector2.zero;
    }

    private UnityEngine.Vector3 _mousePos;
    private int _activeDisplay = 0;

    private static bool HasMouse()
    {
      #if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePresent;
      #else
        return UnityEngine.InputSystem.Mouse.current != null;
      #endif
    }

    private static bool IsCursorVisible()
    {
        return UnityEngine.Cursor.visible && UnityEngine.Cursor.lockState != CursorLockMode.Locked;
    }

    private Vector3 MousePosition()
    {
      #if ENABLE_LEGACY_INPUT_MANAGER
        Vector3 mousePosition = UnityEngine.Input.mousePosition;
      #else
        Vector3 mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
      #endif

        Vector3 p = Display.RelativeMouseAt(mousePosition);

        if (p == Vector3.zero)
        {
            return mousePosition;
        }

        _activeDisplay = (int)p.z;
        return p;
    }

    private void UpdateMouse()
    {
        if (HasMouse() && IsCursorVisible())
        {
            Vector3 mousePos = MousePosition();

            // mouse move
            if ((_camera == null || _activeDisplay == _camera.targetDisplay) && _mousePos != mousePos)
            {
                _mousePos = mousePos;

                UnityEngine.Vector2 mouse = ProjectPointer(_mousePos.x, _mousePos.y);
                _uiView.MouseMove((int)mouse.x, (int)mouse.y);
            }
        }
    }

    private void UpdateTouch()
    {
      #if ENABLE_LEGACY_INPUT_MANAGER
        for (int i = 0; i < UnityEngine.Input.touchCount; i++) 
        {
            UnityEngine.Touch touch = UnityEngine.Input.GetTouch(i);
            UnityEngine.Vector2 pos = ProjectPointer(touch.position.x, touch.position.y);
            UnityEngine.TouchPhase phase = touch.phase;

            if (phase == UnityEngine.TouchPhase.Began)
            {
                _uiView.TouchDown((int)pos.x, (int)pos.y, (uint)touch.fingerId);
            }
            else if (phase == UnityEngine.TouchPhase.Moved || phase == UnityEngine.TouchPhase.Stationary)
            {
                _uiView.TouchMove((int)pos.x, (int)pos.y, (uint)touch.fingerId);
            }
            else
            {
                _uiView.TouchUp((int)pos.x, (int)pos.y, (uint)touch.fingerId);
            }
        }
      #else
        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                UnityEngine.Vector2 pos = ProjectPointer(touch.screenPosition.x, touch.screenPosition.y);

                if (touch.began)
                {
                    _uiView.TouchDown((int)pos.x, (int)pos.y, (uint)touch.touchId);
                }
                else if (touch.ended)
                {
                    _uiView.TouchUp((int)pos.x, (int)pos.y, (uint)touch.touchId);
                }
                else
                {
                    _uiView.TouchMove((int)pos.x, (int)pos.y, (uint)touch.touchId);
                }
            }
        }
      #endif
    }

    [FlagsAttribute]
    enum ActionButtons
    {
         Up = 1,
         Down = 2,
         Left = 4,
         Right = 8,
         Accept = 16,
         Cancel = 32,
         Menu = 64,
         View = 128,
         PageUp = 256,
         PageDown = 512,
         PageLeft = 1024,
         PageRight = 2048,
         Context1 = 4096,
         Context2 = 8192,
         Context3 = 16384,
         Context4 = 32768
    }

    private struct ButtonState
    {
        public ActionButtons button;
        public Noesis.Key key;
        public float t;
    }

    private ButtonState[] _buttonStates = new ButtonState[]
    {
        new ButtonState { button = ActionButtons.Up, key = Key.GamepadUp },
        new ButtonState { button = ActionButtons.Down, key = Key.GamepadDown },
        new ButtonState { button = ActionButtons.Left, key = Key.GamepadLeft },
        new ButtonState { button = ActionButtons.Right, key = Key.GamepadRight },
        new ButtonState { button = ActionButtons.Accept, key = Key.GamepadAccept },
        new ButtonState { button = ActionButtons.Cancel, key = Key.GamepadCancel },
        new ButtonState { button = ActionButtons.Menu, key = Key.GamepadMenu},
        new ButtonState { button = ActionButtons.View, key = Key.GamepadView },
        new ButtonState { button = ActionButtons.PageUp, key = Key.GamepadPageUp },
        new ButtonState { button = ActionButtons.PageDown, key = Key.GamepadPageDown },
        new ButtonState { button = ActionButtons.PageLeft, key = Key.GamepadPageLeft },
        new ButtonState { button = ActionButtons.PageRight, key = Key.GamepadPageRight },
        new ButtonState { button = ActionButtons.Context1, key = Key.GamepadContext1 },
        new ButtonState { button = ActionButtons.Context2, key = Key.GamepadContext2 },
        new ButtonState { button = ActionButtons.Context3, key = Key.GamepadContext3 },
        new ButtonState { button = ActionButtons.Context4, key = Key.GamepadContext4 }
    };

    private ActionButtons _actionButtons = 0;

    private void UpdateActions(float t)
    {
        int x = int.MaxValue;
        int y = int.MaxValue;
        bool trackedPos = false;

        if (_trackedPositionAction?.activeControl != null && _trackedRotationAction?.activeControl != null)
        {
            Vector3 pos_ = _trackedPositionAction.ReadValue<Vector3>();
            Vector3 dir_ = _trackedRotationAction.ReadValue<Quaternion>() * Vector3.forward;

            if (_xrTrackingOrigin != null)
            {
                pos_ = _xrTrackingOrigin.TransformPoint(pos_);
                dir_ = _xrTrackingOrigin.TransformVector(dir_);
            }

            Point3D pos = new Point3D(pos_.x, pos_.y, pos_.z);
            Vector3D dir = new Vector3D(dir_.x, dir_.y, dir_.z);

            var captured = Content.Mouse.Captured;
            if (captured != null)
            {
                // Visuals capturing the mouse must always utilize its 3D plane for hit testing
                // https://www.noesisengine.com/bugs/view.php?id=2837
                if (VisualTreeHelper.IntersectPlane(captured, pos, dir, out Point3D worldPos))
                {
                    Noesis.Matrix4 mtx = CameraMatrix(_camera);
                    Noesis.Vector4 cameraPos = new Noesis.Vector4(worldPos.X, worldPos.Y, worldPos.Z, 1.0f) * mtx;

                    x = (int)(cameraPos.X / cameraPos.W);
                    y = (int)(cameraPos.Y / cameraPos.W);

                    trackedPos = true;
                }
            }
            else
            {
                Visual root = (Visual)VisualTreeHelper.GetRoot(_uiView.Content);
                var hit = VisualTreeHelper.HitTest3D(root, pos, dir);

                if (hit.VisualHit != null)
                {
                    Noesis.Matrix4 mtx = CameraMatrix(_camera);
                    Noesis.Vector4 cameraPos = new Noesis.Vector4(hit.WorldPos.X, hit.WorldPos.Y, hit.WorldPos.Z, 1.0f) * mtx;

                    x = (int)(cameraPos.X / cameraPos.W);
                    y = (int)(cameraPos.Y / cameraPos.W);

                    trackedPos = true;
                }
            }

            MouseMove(x, y);
        }

        if (_trackedTriggerAction != null)
        {
            if (_trackedTriggerAction.WasPressedThisFrame())
            {
                MouseButtonDown(x, y, Noesis.MouseButton.Left);
            }

            if (_trackedTriggerAction.WasReleasedThisFrame())
            {
                MouseButtonUp(x, y, Noesis.MouseButton.Left);
            }
        }

        if (_scrollAction != null)
        {
            Vector2 v = _scrollAction.ReadValue<Vector2>();

            if (trackedPos)
            {
                if (v.y != 0.0f) _uiView.Scroll(x, y, v.y);
                if (v.x != 0.0f) _uiView.HScroll(x, y, v.x);
            }
            else
            {
                if (v.y != 0.0f) _uiView.Scroll(v.y);
                if (v.x != 0.0f) _uiView.HScroll(v.x);
            }
        }

        ActionButtons actionButtons = 0;

        if (_upAction != null && _upAction.IsPressed()) actionButtons |= ActionButtons.Up;
        if (_downAction != null && _downAction.IsPressed()) actionButtons |= ActionButtons.Down;
        if (_leftAction != null && _leftAction.IsPressed()) actionButtons |= ActionButtons.Left;
        if (_rightAction != null && _rightAction.IsPressed()) actionButtons |= ActionButtons.Right;

        if (_acceptAction != null && _acceptAction.IsPressed()) actionButtons |= ActionButtons.Accept;
        if (_cancelAction != null && _cancelAction.IsPressed()) actionButtons |= ActionButtons.Cancel;

        if (_menuAction != null && _menuAction.IsPressed()) actionButtons |= ActionButtons.Menu;
        if (_viewAction != null && _viewAction.IsPressed()) actionButtons |= ActionButtons.View;

        if (_pageUpAction != null && _pageUpAction.IsPressed()) actionButtons |= ActionButtons.PageUp;
        if (_pageDownAction != null && _pageDownAction.IsPressed()) actionButtons |= ActionButtons.PageDown;
        if (_pageLeftAction != null && _pageLeftAction.IsPressed()) actionButtons |= ActionButtons.PageLeft;
        if (_pageRightAction != null && _pageRightAction.IsPressed()) actionButtons |= ActionButtons.PageRight;

        if (_context1Action != null && _context1Action.IsPressed()) actionButtons |= ActionButtons.Context1;
        if (_context2Action != null && _context2Action.IsPressed()) actionButtons |= ActionButtons.Context2;
        if (_context3Action != null && _context3Action.IsPressed()) actionButtons |= ActionButtons.Context3;
        if (_context4Action != null && _context4Action.IsPressed()) actionButtons |= ActionButtons.Context4;

        ActionButtons delta = actionButtons ^ _actionButtons;
        if (delta != 0 || actionButtons != 0)
        {
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                if ((delta & _buttonStates[i].button) > 0)
                {
                    if ((actionButtons & _buttonStates[i].button) > 0)
                    {
                        _uiView.KeyDown(_buttonStates[i].key);
                        _buttonStates[i].t = t + _actionsRepeatDelay;
                    }
                    else
                    {
                        _uiView.KeyUp(_buttonStates[i].key);
                    }
                }
                else if ((actionButtons & _buttonStates[i].button) > 0)
                {
                    if (t >= _buttonStates[i].t)
                    {
                        _uiView.KeyDown(_buttonStates[i].key);
                        _buttonStates[i].t = t + _actionsRepeatRate;
                    }
                }
            }
        }

         _actionButtons = actionButtons;
    }

    private void UpdateInputs(float t)
    {
        if (_enableMouse)
        {
            UpdateMouse();
        }

        if (_enableTouch)
        {
            UpdateTouch();
        }

        if (_enableActions)
        {
            UpdateActions(t);
        }
    }

    private int _viewSizeX;
    private int _viewSizeY;
    private float _viewScale;
    private float _viewStereoScale = 1.0f;

    private void UpdateSize()
    {
        int sizeX = 0;
        int sizeY = 0;

        if (_camera != null)
        {
            sizeX = _camera.pixelWidth;
            sizeY = _camera.pixelHeight;
        }
        else if (_texture != null)
        {
            sizeX = _texture.width;
            sizeY = _texture.height;
        }

        if (sizeX != _viewSizeX || sizeY != _viewSizeY)
        {
            _uiView.SetSize(sizeX, sizeY);
            _viewSizeX = sizeX;
            _viewSizeY = sizeY;
        }

        float scale = (!WorldSpace && _dpiScale && Screen.dpi > 0.0f) ? Screen.dpi / 96.0f : 1.0f;

        if (scale != _viewScale)
        {
            _uiView.SetScale(scale);
            _viewScale = scale;
        }

        if (_stereoScale != _viewStereoScale)
        {
            _uiView.SetStereoOffscreenScaleFactor(_stereoScale);
            _viewStereoScale = _stereoScale;
        }
    }

    private bool _visible = true;

    void LateUpdate()
    {
        if (!_enableExternalUpdate)
        {
            UpdateInternal();
        }
    }

    public void ExternalUpdate()
    {
        Debug.Assert(_enableExternalUpdate, "Calling ExternalUpdate() with EnableExternalUpdate disabled", this);
        UpdateInternal();
    }

    private static Noesis.Matrix4 NoesisMatrix(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, float w, float h)
    {
        float hw = 0.5f * w;
        float hh = 0.5f * h;

        Matrix4x4 noesisMatrix;

        if (SystemInfo.usesReversedZBuffer)
        {
            noesisMatrix = new Matrix4x4
            (
                new UnityEngine.Vector4(hw,  0,  0,    0),
                new UnityEngine.Vector4(0, -hh,  0,    0),
                new UnityEngine.Vector4(0,   0, -0.5f, 0),
                new UnityEngine.Vector4(hw, hh,  0.5f, 1)
            );
        }
        else
        {
            noesisMatrix = new Matrix4x4
            (
                new UnityEngine.Vector4(hw,  0, 0,    0),
                new UnityEngine.Vector4(0, -hh, 0,    0),
                new UnityEngine.Vector4(0,   0, 0.5f, 0),
                new UnityEngine.Vector4(hw, hh, 0.5f, 1)
            );
        }

        Matrix4x4 _ = noesisMatrix * projectionMatrix * viewMatrix;

        return new Matrix4
        (
            _.m00, _.m10, _.m20, _.m30,
            _.m01, _.m11, _.m21, _.m31,
            _.m02, _.m12, _.m22, _.m32,
            _.m03, _.m13, _.m23, _.m33
        );
    }

    private static Noesis.Matrix4 CameraMatrix(Camera camera)
    {
        return NoesisMatrix(camera.worldToCameraMatrix, camera.projectionMatrix, camera.pixelWidth, camera.pixelHeight);
    }

    private static Matrix4 CameraStereoMatrix(Camera camera, Camera.StereoscopicEye eye)
    {
        Matrix4x4 viewMatrix = camera.GetStereoViewMatrix(eye);
        Matrix4x4 projectionMatrix = camera.GetStereoProjectionMatrix(eye);

        return NoesisMatrix(viewMatrix, projectionMatrix, camera.pixelWidth, camera.pixelHeight);
    }

    private static Matrix4 CameraActiveStereoMatrix(Camera camera)
    {
        Camera.StereoscopicEye eye;

        if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
        {
            eye = Camera.StereoscopicEye.Left;
        }
        else
        {
            Debug.Assert(camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right);
            eye = Camera.StereoscopicEye.Right;
        }

        return CameraStereoMatrix(camera, eye);
    }

    private void UpdateInternal()
    {
        if (_uiView != null && _visible)
        {
            Profiling.UpdateSampler.Begin();

            if (_camera != null && WorldSpace)
            {
                _uiView.SetProjectionMatrix(CameraMatrix(_camera));
            }

            float t = Time.realtimeSinceStartup;

            UpdateSize();
            UpdateInputs(t);

            NoesisUnity.IME.Update(_uiView);
            NoesisUnity.TouchKeyboard.Update();

            Noesis_UnityUpdate();
            _needsRendering = _uiView.Update(_useRealTimeClock ? t : Time.time);

            Profiling.UpdateSampler.End();

            if (_needsRendering)
            {
                _commands.name = Profiling.UpdateRenderTree;
                NoesisRenderer.UpdateRenderTree(_uiView, _commands);

                Graphics.ExecuteCommandBuffer(_commands);
                _commands.Clear();
            }

            if (_camera == null && _texture != null)
            {
                if (_continuousRendering || _needsRendering)
                {
                    _commands.name = Profiling.RenderTexture;
                    NoesisRenderer.RenderOffscreen(_uiView, _commands, false);
                    _commands.SetRenderTarget(_texture, LoadAction.DontCare, StoreAction.Store, LoadAction.DontCare, StoreAction.DontCare);
                    _commands.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
                    NoesisRenderer.RenderOnscreen(_uiView, SystemInfo.graphicsUVStartsAtTop, _commands, false, _clearStencil);

                    Graphics.ExecuteCommandBuffer(_commands);
                    _commands.Clear();

                    GL.InvalidateState();
                    _texture.DiscardContents(false, true);
                }
            }
        }
    }

    void OnBecameInvisible()
    {
        if (_uiView != null && _texture != null)
        {
            _visible = false;
        }
    }

    void OnBecameVisible()
    {
        if (_uiView != null && _texture != null)
        {
            _visible = true;
        }
    }

    private void RenderOffscreen(CommandBuffer commands)
    {
        NoesisRenderer.RenderOffscreen(_uiView, _commands, false);
    }

    private void RenderOnScreen(bool flipY, CommandBuffer commands)
    {
        if (_camera != null && _camera.stereoEnabled)
        {
          #if (ENABLE_VR_MODULE && ENABLE_VR)
            if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.MultiPass)
            {
                NoesisRenderer.RenderOnscreen(_uiView, CameraActiveStereoMatrix(_camera),
                    flipY, commands, false, _clearStencil);
            }
            else
            {
                NoesisRenderer.RenderOnscreen(_uiView, CameraMatrix(_camera),
                    CameraStereoMatrix(_camera, Camera.StereoscopicEye.Left),
                    CameraStereoMatrix(_camera, Camera.StereoscopicEye.Right),
                    flipY, commands, false, _clearStencil);
            }
          #endif
        }
        else
        {
            NoesisRenderer.RenderOnscreen(_uiView, flipY, commands, false, _clearStencil);
        }
    }

    private bool _updatePending = true;

    private void PreRender(Camera cam)
    {
        if (_camera != null)
        {
            // In case there are several cameras rendering to the same texture (Camera Stacking),
            // the camera rendered first (less depth) is the one that must apply our offscreen phase
            // to avoid inefficient Load/Store in Tiled architectures
            if (_updatePending && cam.targetTexture == _camera.targetTexture && cam.depth <= _camera.depth)
            {
                if (_uiView != null && _visible)
                {
                    _commands.name = Profiling.RenderOffScreen;
                    RenderOffscreen(_commands);

                    Graphics.ExecuteCommandBuffer(_commands);
                    _commands.Clear();

                    GL.InvalidateState();
                    ForceRestoreCameraRenderTarget();
                }

                _updatePending = false;
            }
        }
    }

    private void ForceRestoreCameraRenderTarget()
    {
        // Unity should automatically restore the render target but sometimes (for example a scene without lights)
        // it doesn't. We use this hack to flush the active render target and force unity to set the camera RT afterward
        RenderTexture surface = RenderTexture.GetTemporary(1,1);
        Graphics.SetRenderTarget(surface);
        RenderTexture.ReleaseTemporary(surface);
    }

    private bool IsEyeTexture(RenderTexture texture)
    {
        // In VR the Swap Chain is named 'XR Texture[#]' (before Unity 2020 it was 'RTDeviceEyeTextureArray')
        return texture.name.StartsWith("XR Texture");
    }

    private bool FlipRender()
    {
        if (SystemInfo.graphicsUVStartsAtTop)
        {
          #if ENABLE_VR && ENABLE_XR_MODULE
            return _camera.activeTexture != null && !IsEyeTexture(_camera.activeTexture);
          #else
            return _camera.activeTexture != null;
          #endif
        }

        return false;
    }

    private void OnPostRender()
    {
        if (_uiView != null && _visible)
        {
            _commands.name = Profiling.RenderOnScreen;
            RenderOnScreen(FlipRender(), _commands);

            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            GL.InvalidateState();
            _updatePending = true;
        }
    }

    private UnityEngine.EventModifiers _modifiers = 0;

    private void ProcessModifierKey(EventModifiers modifiers, EventModifiers delta, EventModifiers flag, Noesis.Key key)
    {
        if ((delta & flag) > 0)
        {
            if ((modifiers & flag) > 0)
            {
                _uiView.KeyDown(key);
            }
            else
            {
                _uiView.KeyUp(key);
            }
        }
    }

    private bool HitTest(float x, float y)
    {
        Visual root = (Visual)VisualTreeHelper.GetRoot(_uiView.Content);
        Point p = root.PointFromScreen(new Point(x, y));

        // Comment this define at the top of the file if you want to use the old behavior
      #if IGNORE_ISHITTESTVISIBLE_FALSE_ELEMENTS
        return Noesis_UnityHitTest(BaseComponent.getCPtr(root), ref p);
      #else
        return VisualTreeHelper.HitTest(root, p).VisualHit != null;
      #endif
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
    private static int lastFrame;
    private static Noesis.Key lastKeyDown;
#endif

    private bool MouseEmulated()
    {
      #if ENABLE_LEGACY_INPUT_MANAGER
        return Input.simulateMouseWithTouches && Input.touchCount > 0;
      #else
        // Unfortunately, with the Input System package active, 'Input.touchCount' is always zero,
        // so we can't detect emulated mouse events directly. Until we deprecate OnGUI events,
        // the workaround is to check whether a mouse device exists. If both mouse and touch devices
        // are available, this check won't be reliable. In that case, the current solution is to
        // disable one of these input methods in NoesisView
        // Note: Checking if 'InputSystem.Touchscreen.current' is null doesn't work because
        // recent Unity versions always report a Touchscreen device on PC, even when none exists.
        return UnityEngine.InputSystem.Mouse.current == null;
      #endif
    }

    private void ProcessEvent(UnityEngine.Event ev, bool enableKeyboard, bool enableMouse)
    {
        // Process keyboard modifiers
        if (enableKeyboard)
        {
            EventModifiers delta = ev.modifiers ^ _modifiers;
            if (delta > 0)
            {
                _modifiers = ev.modifiers;

                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Shift, Key.LeftShift);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Control, Key.LeftCtrl);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Command, Key.LeftCtrl);
                ProcessModifierKey(ev.modifiers, delta, EventModifiers.Alt, Key.LeftAlt);
            }
        }

        switch (ev.type)
        {
            case UnityEngine.EventType.MouseDown:
            {
                if (enableMouse && IsCursorVisible())
                {
                    UnityEngine.Vector2 mouse = ProjectPointer(ev.mousePosition.x, UnityEngine.Screen.height - ev.mousePosition.y);

                    if (HitTest(mouse.x, mouse.y))
                    {
                        ev.Use();
                    }

                    if (!MouseEmulated())
                    {
                        if (ev.clickCount == 1)
                        {
                            _uiView.MouseButtonDown((int)mouse.x, (int)mouse.y, (Noesis.MouseButton)ev.button);
                        }
                        else
                        {
                            _uiView.MouseDoubleClick((int)mouse.x, (int)mouse.y, (Noesis.MouseButton)ev.button);
                        }
                    }
                }
                break;
            }
            case UnityEngine.EventType.MouseUp:
            {
                if (enableMouse && IsCursorVisible())
                {
                    UnityEngine.Vector2 mouse = ProjectPointer(ev.mousePosition.x, UnityEngine.Screen.height - ev.mousePosition.y);

                    if (HitTest(mouse.x, mouse.y))
                    {
                        ev.Use();
                    }

                    if (!MouseEmulated())
                    {
                        _uiView.MouseButtonUp((int)mouse.x, (int)mouse.y, (Noesis.MouseButton)ev.button);
                    }
                }
                break;
            }
            case UnityEngine.EventType.ScrollWheel:
            {
                if (enableMouse && IsCursorVisible())
                {
                    UnityEngine.Vector2 mouse = ProjectPointer(ev.mousePosition.x, UnityEngine.Screen.height - ev.mousePosition.y);

                    if (ev.delta.y != 0.0f)
                    {
                        _uiView.MouseWheel((int)mouse.x, (int)mouse.y, -(int)(ev.delta.y * 40.0f));
                    }

                    if (ev.delta.x != 0.0f)
                    {
                        _uiView.MouseHWheel((int)mouse.x, (int)mouse.y, (int)(ev.delta.x * 40.0f));
                    }
                }
                break;
            }
            case UnityEngine.EventType.KeyDown:
            {
                if (enableKeyboard)
                {
                    // Don't process key when IME composition is being used
                    if (ev.keyCode != KeyCode.None && NoesisUnity.IME.compositionString == "")
                    {
                        Noesis.Key noesisKeyCode = NoesisKeyCodes.Convert(ev.keyCode);
                        if (noesisKeyCode != Noesis.Key.None)
                        {
                          #if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                            // In OSX Standalone, CMD + key always sends two KeyDown events for the key.
                            // This seems to be a bug in Unity. 
                            if (!ev.command || lastFrame != Time.frameCount || lastKeyDown != noesisKeyCode)
                            {
                                lastFrame = Time.frameCount;
                                lastKeyDown = noesisKeyCode;
                          #endif
                                _uiView.KeyDown(noesisKeyCode);
                          #if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                            }
                          #endif
                        }
                    }

                    if (ev.character != 0)
                    {
                        // Filter out character events when CTRL is down
                        bool isControl = (_modifiers & EventModifiers.Control) != 0 || (_modifiers & EventModifiers.Command) != 0;
                        bool isAlt = (_modifiers & EventModifiers.Alt) != 0;
                        bool filter = isControl && !isAlt;

                        if (!filter)
                        {
                          #if !UNITY_EDITOR && UNITY_STANDALONE_LINUX
                            // It seems that linux is sending KeySyms instead of Unicode points
                            // https://github.com/substack/node-keysym/blob/master/data/keysyms.txt
                            ev.character = NoesisKeyCodes.KeySymToUnicode(ev.character);
                          #endif
                            _uiView.Char((uint)ev.character);
                        }
                    }

                }
                break;
            }
            case UnityEngine.EventType.KeyUp:
            {
                // Don't process key when IME composition is being used
                if (enableKeyboard)
                {
                    if (ev.keyCode != KeyCode.None && NoesisUnity.IME.compositionString == "")
                    {
                        Noesis.Key noesisKeyCode = NoesisKeyCodes.Convert(ev.keyCode);
                        if (noesisKeyCode != Noesis.Key.None)
                        {
                            _uiView.KeyUp(noesisKeyCode);
                        }
                    }
                }
                break;
            }
        }
    }

    void OnGUI()
    {
        if (_uiView != null && (_camera == null || _activeDisplay == _camera.targetDisplay))
        {
            if (_camera)
            {
                UnityEngine.GUI.depth = -(int)_camera.depth;
            }

            ProcessEvent(UnityEngine.Event.current, _enableKeyboard, _enableMouse);
        }
    }

    void OnApplicationFocus(bool focused)
    {
        if (_uiView != null)
        {
            if (NoesisUnity.TouchKeyboard.keyboard == null)
            {
                if (focused)
                {
                    _uiView.Activate();
                }
                else
                {
                    _uiView.Deactivate();
                }
            }
        }
    }
#endregion

    private void CreateView(FrameworkElement content)
    {
        if (_uiView == null)
        {
            // Send settings for the internal device, created by the first view
            NoesisRenderer.SetRenderSettings();

            _viewSizeX = 0;
            _viewSizeY = 0;
            _viewScale = 1.0f;

            _uiView = new Noesis.View(content);
            _uiView.SetTessellationMaxPixelError(_tessellationMaxPixelError);
            _uiView.SetEmulateTouch(_emulateTouch);
            _uiView.SetFlags(_renderFlags);

            _commands.name = Profiling.RegisterView;
            NoesisRenderer.RegisterView(_uiView, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

          #if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += DestroyView;
          #endif
        }
    }

    private void DestroyView()
    {
        if (_uiView != null)
        {
            _commands.name = Profiling.UnregisterView;
            NoesisRenderer.UnregisterView(_uiView, _commands);
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();

            _uiView = null;
        }
    }

    public void OnBeforeSerialize() {}

    public void OnAfterDeserialize()
    {
        // (3.0) PPAA flag is now in view render flags 
        if (_isPPAAEnabled)
        {
            _renderFlags |= RenderFlags.PPAA;
            _isPPAAEnabled = false;
        }
    }

    private Noesis.View _uiView;
    private bool _needsRendering = false;
    private float _stereoScale = 1.0f;

#region Serialized properties
    [SerializeField] private NoesisXaml _xaml;
    [SerializeField] private RenderTexture _texture;

    [SerializeField] private bool _isPPAAEnabled = true;
    [SerializeField] private float _tessellationMaxPixelError = Noesis.TessellationMaxPixelError.MediumQuality.Error;
    [SerializeField] private RenderFlags _renderFlags = 0;
    [SerializeField] private bool _dpiScale = true;
    [SerializeField] private bool _continuousRendering = true;
    [SerializeField] private bool _enableExternalUpdate = false;
    [SerializeField] private bool _enableKeyboard = true;
    [SerializeField] private bool _enableMouse = true;
    [SerializeField] private bool _enableTouch = true;
    [UnityEngine.Serialization.FormerlySerializedAs("_enableGamepad")]
    [SerializeField] private bool _enableActions = false;
    [SerializeField] private bool _emulateTouch = false;
    [SerializeField] private bool _useRealTimeClock = false;
    [SerializeField] private bool _clearStencil = false;

    [SerializeField] private UnityEngine.InputSystem.InputActionAsset _actions;
    [SerializeField] private string _actionMap = "Gamepad";

    [UnityEngine.Serialization.FormerlySerializedAs("_gamepadRepeatDelay")]
    [SerializeField] private float _actionsRepeatDelay = 0.5f;

    [UnityEngine.Serialization.FormerlySerializedAs("_gamepadRepeatRate")]
    [SerializeField] private float _actionsRepeatRate = 0.1f;

    [SerializeField] private UnityEngine.Transform _xrTrackingOrigin;

  #if ENABLE_URP_PACKAGE
    [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
  #endif

  #if ENABLE_HDRP_PACKAGE
    [SerializeField] private CustomPassInjectionPoint _injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
  #endif

    private UnityEngine.InputSystem.InputAction _upAction;
    private UnityEngine.InputSystem.InputAction _downAction;
    private UnityEngine.InputSystem.InputAction _leftAction;
    private UnityEngine.InputSystem.InputAction _rightAction;

    private UnityEngine.InputSystem.InputAction _acceptAction;
    private UnityEngine.InputSystem.InputAction _cancelAction;

    private UnityEngine.InputSystem.InputAction _menuAction;
    private UnityEngine.InputSystem.InputAction _viewAction;

    private UnityEngine.InputSystem.InputAction _pageLeftAction;
    private UnityEngine.InputSystem.InputAction _pageRightAction;
    private UnityEngine.InputSystem.InputAction _pageUpAction;
    private UnityEngine.InputSystem.InputAction _pageDownAction;
    private UnityEngine.InputSystem.InputAction _scrollAction;

    private UnityEngine.InputSystem.InputAction _context1Action;
    private UnityEngine.InputSystem.InputAction _context2Action;
    private UnityEngine.InputSystem.InputAction _context3Action;
    private UnityEngine.InputSystem.InputAction _context4Action;

    private UnityEngine.InputSystem.InputAction _trackedPositionAction;
    private UnityEngine.InputSystem.InputAction _trackedRotationAction;
    private UnityEngine.InputSystem.InputAction _trackedTriggerAction;
#endregion

#region Imports
    [DllImport(Library.Name)]
    private static extern void Noesis_UnityUpdate();

    [DllImport(Library.Name)]
    private static extern bool Noesis_UnityHitTest(HandleRef root, ref Point point);
#endregion

#endregion
}