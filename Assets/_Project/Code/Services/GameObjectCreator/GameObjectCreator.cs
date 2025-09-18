#nullable enable
using JetBrains.Annotations;
using UnityEngine;
using VContainer.Unity;

namespace Nuclear.Services
{
    [UsedImplicitly]
    public sealed class GameObjectCreator : IGameObjectCreator
    {
        private readonly IObjectResolverProvider _objectResolverProvider;

        public GameObjectCreator(IObjectResolverProvider objectResolverProvider)
        {
            _objectResolverProvider = objectResolverProvider;
        }

        T IGameObjectCreator.Instantiate<T>(T prefab, Transform parent, bool worldPositionStays)
        {
            if (prefab is GameObject gameObject)
            {
                return (_objectResolverProvider.Container.Instantiate(gameObject, parent, worldPositionStays) as T)!;
            }
            return (_objectResolverProvider.Container.Instantiate(prefab as Component, parent, worldPositionStays) as T)!;
        }
    }
}