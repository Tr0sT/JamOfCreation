#nullable enable
using Sirenix.OdinInspector;
using VContainer;

namespace Nuclear.Services.GUI
{
    public abstract class InitableMonoBehaviour<T> : SerializedMonoBehaviour where T : class?
    {
        protected T _viewModel = null!;
        protected IGameObjectCreator _gameObjectCreator = null!;

        [Inject]
        private void Inject(IGameObjectCreator gameObjectCreator)
        {
            _gameObjectCreator = gameObjectCreator;
        }
        
        public void Init(T viewModel)
        {
            _viewModel = viewModel;
            Init();
        }

        protected abstract void Init();
    }
}