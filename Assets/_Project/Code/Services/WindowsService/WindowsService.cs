#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Nuclear.Services.GUI;
using Nuclear.WindowsManager;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nuclear.Services
{
    [UsedImplicitly]
    public class WindowsService : IWindowsService
    {
        public event Action<Window>? OnWindowOpened;
        public event Action<Window>? OnWindowHidden;

        private IWindowsManager _mainWindowsManager;
        private readonly IWindowsManager _messagesWindowsManager;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IObjectResolverProvider _objectResolverProvider;
        private readonly IGameObjectCreator _gameObjectCreator;
        private readonly Stack<Window> _windows = new();

        public WindowsService(IWindowsManager mainWindowsManager, 
            IWindowsManager messagesWindowsManager,
            IViewModelFactory viewModelFactory, 
            IObjectResolverProvider objectResolverProvider, 
            IGameObjectCreator gameObjectCreator)
        {
            _mainWindowsManager = mainWindowsManager;
            _messagesWindowsManager = messagesWindowsManager;
            _viewModelFactory = viewModelFactory;
            _objectResolverProvider = objectResolverProvider;
            _gameObjectCreator = gameObjectCreator;
        }

        public void Dispose()
        {
            _mainWindowsManager.Dispose();
            _messagesWindowsManager.Dispose();
        }

        public TWindow CreateWindow<TWindow, TViewModel, TViewModelData>(TViewModelData data) 
            where TWindow : MWindow<TViewModel> 
            where TViewModel : IViewModel 
        {
            var window = (TWindow)_mainWindowsManager.CreateWindow(GetPath(typeof(TWindow)),
                window =>
                {
                    var tWindow = (MWindow<TViewModel>) window;
                    _objectResolverProvider.Container.InjectGameObject(tWindow.gameObject);
                    var tViewModel = _viewModelFactory.Create<TViewModel, TViewModelData>(data);
                    tWindow.SetViewModel(tViewModel);
                    tWindow.SetGameObjectCreator(_gameObjectCreator);
                });

            window.OnHidden += OnWindowHiddenCallback;
            OnWindowOpened?.Invoke(window);
            _windows.Push(window);
            return window;
        }

        public TWindow CreateWindow<TWindow, TViewModel>() 
            where TWindow : MWindow<TViewModel> 
            where TViewModel : IViewModel
        {
            var window = (TWindow)_mainWindowsManager.CreateWindow(GetPath(typeof(TWindow)),
                window =>
                {
                    var tWindow = (MWindow<TViewModel>) window;
                    _objectResolverProvider.Container.InjectGameObject(tWindow.gameObject);
                    var tViewModel = _viewModelFactory.Create<TViewModel>();
                    tWindow.SetViewModel(tViewModel);
                    tWindow.SetGameObjectCreator(_gameObjectCreator);
                });
            window.OnHidden += OnWindowHiddenCallback;
            _windows.Push(window);
            OnWindowOpened?.Invoke(window);
            return window;
        }

        public TTooltip CreateTooltip<TTooltip, TViewModel, TViewModelData>(TViewModelData data, RectTransform anchor) 
            where TTooltip : BaseTooltip<TViewModel> where TViewModel : IViewModel
        {
            var tooltip = CreateWindow<TTooltip, TViewModel, TViewModelData>(data);
            tooltip.SetAnchor(anchor);
            return tooltip;
        }

        public void CloseAllAndClearState()
        {
            var siblingIndex = _mainWindowsManager.GetRoot().transform.GetSiblingIndex();
            _mainWindowsManager.Dispose();
            _mainWindowsManager = _objectResolverProvider.Container.Resolve<IWindowsManager>();
            _mainWindowsManager.GetRoot().transform.SetSiblingIndex(siblingIndex);
        }

        public bool IsTopWindow(Type type)
        {
            return _windows.Count != 0 && type == _windows.Peek().GetType();
        }

        private static string GetPath(MemberInfo type)
        {
            var attribute = type.GetCustomAttribute<PathAttribute>(inherit: true);
            if (attribute == null)
            {
                throw new($"No Path attribute for type {type.Name}");
            }
            
            return attribute.Path;
        }

        private void OnWindowHiddenCallback(Window window)
        {
            _windows.Pop();
            OnWindowHidden?.Invoke(window);
        }
    }
}