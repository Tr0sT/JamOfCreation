#nullable enable
using System;
using Nuclear.Services.GUI;
using Nuclear.WindowsManager;
using UnityEngine;

namespace Nuclear.Services
{
    public interface IWindowsService : IDisposable
    {
        event Action<Window>? OnWindowOpened;
        event Action<Window>? OnWindowHidden;

        TWindow CreateWindow<TWindow, TViewModel, TViewModelData>(TViewModelData data)
            where TWindow : MWindow<TViewModel>
            where TViewModel : IViewModel;

        TWindow CreateWindow<TWindow, TViewModel>()
            where TWindow : MWindow<TViewModel>
            where TViewModel : IViewModel;

        public TTooltip CreateTooltip<TTooltip, TViewModel, TViewModelData>(TViewModelData data, RectTransform anchor)
            where TTooltip : BaseTooltip<TViewModel>
            where TViewModel : IViewModel;

        void CloseAllAndClearState();

        bool IsTopWindow(Type type);
    }
}