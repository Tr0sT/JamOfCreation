#nullable enable
using System;
using Nuclear.WindowsManager;

namespace Nuclear.Services.GUI
{
    public abstract class MWindow<TViewModel> : Window
    {
        protected TViewModel _viewModel = default!;
        protected IGameObjectCreator _gameObjectCreator = null!;
        public void SetViewModel(TViewModel viewModel)
        {
            _viewModel = viewModel;
            if (_viewModel is IDisposable disposable)
                AddDeInitAction(disposable.Dispose);
        }

        public void SetGameObjectCreator(IGameObjectCreator gameObjectCreator)
        {
            _gameObjectCreator = gameObjectCreator;
        }
    }
}
