#nullable enable
using UnityEngine;

namespace Nuclear.Services
{
    public interface IGameObjectCreator
    {
        T Instantiate<T>(T prefab, Transform parent, bool worldPositionStays = false)
            where T : Object;
    }
}