using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Noesis;

[DefaultExecutionOrder(100)]
public class NoesisWorldUI : MonoBehaviour
{
    [SerializeField]
    private NoesisView _view;
    public NoesisView View
    {
        get => _view;
        set
        {
            if (_view != value)
            {
                _view = value;

                RemoveContent();
                FindContainer();
                AddContent();
            }
        }
    }

    [SerializeField]
    private string _container = "Root";
    public string Container
    {
        get => _container;
        set
        {
            if (_container != value)
            {
                _container = value;

                RemoveContent();
                FindContainer();
                AddContent();
            }
        }
    }

    [SerializeField]
    private NoesisXaml _xaml;
    public NoesisXaml Xaml
    {
        get => _xaml;
        set
        {
            if (_xaml != value)
            {
                _xaml = value;

                RemoveContent();
                LoadContent();
                AddContent();
            }
        }
    }

    [SerializeField]
    private float _scale = 0.005f;
    public float Scale { get => _scale; set { _scale = value; } }

    [SerializeField]
    private Vector3 _offset = new Vector3();
    public Vector3 Offset { get => _offset; set { _offset = value; } }

    [SerializeField]
    private bool _center = true;
    public bool Center { get => _center; set { _center = value; } }

    public FrameworkElement Content => _content;

    void OnEnable()
    {
        EnsureContainer();
        EnsureContent();
        AddContent();
    }

    void OnDisable()
    {
        RemoveContent();
    }

    void OnDestroy()
    {
        _content = null;
        _transform = null;
        _containerPanel = null;
        _camera = null;
        _view = null;
        _xaml = null;
    }

    void Update()
    {
        if (_content != null && _camera != null)
        {
            float width = 0.0f;
            float height = 0.0f;

            if (Center)
            {
                _content.Measure(new Size(float.PositiveInfinity, float.PositiveInfinity));
                width = _content.DesiredSize.Width;
                height = _content.DesiredSize.Height;
            }

            Matrix4x4 mtx = transform.localToWorldMatrix * Matrix4x4.Translate(Offset) *
                Matrix4x4.Scale(new Vector3(Scale, -Scale, Scale)) *
                Matrix4x4.Translate(new Vector3(-0.5f * width, -0.5f * height, 0.00f));

            _transform.Matrix = new Matrix3D
            (
                mtx[0, 0], mtx[1, 0], mtx[2, 0],
                mtx[0, 1], mtx[1, 1], mtx[2, 1],
                mtx[0, 2], mtx[1, 2], mtx[2, 2],
                mtx[0, 3], mtx[1, 3], mtx[2, 3]
            );

            float z = (_camera.worldToCameraMatrix * transform.localToWorldMatrix).m23;
            Panel.SetZIndex(_content, (int)(z * 100.0f));
        }
    }

    void EnsureContainer()
    {
        if (_containerPanel == null)
        {
            FindContainer();
        }
    }

    void FindContainer()
    {
        _containerPanel = null;
        _camera = null;

        if (View != null && !string.IsNullOrEmpty(Container))
        {
            _containerPanel = View.Content?.FindName(Container) as Panel;
            _camera = View.GetComponent<Camera>();
        }
    }

    void EnsureContent()
    {
        if (_content == null)
        {
            LoadContent();
        }
    }

    void LoadContent()
    {
        _content = null;
        _transform = null;

        if (Xaml != null)
        {
            _content = Xaml.Load() as FrameworkElement;
            if (_content != null)
            {
                _transform = new MatrixTransform3D();
                _content.Transform3D = _transform;
            }
        }
    }

    void AddContent()
    {
        if (enabled)
        {
            if (_containerPanel != null && _content != null)
            {
                _containerPanel.Children.Add(_content);
            }
        }
    }

    void RemoveContent()
    {
        if (_containerPanel != null && _content != null)
        {
            _containerPanel.Children.Remove(_content);
        }
    }

    private Panel _containerPanel;
    private Camera _camera;
    private FrameworkElement _content;
    private MatrixTransform3D _transform;
}
