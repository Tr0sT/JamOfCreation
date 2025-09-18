#nullable enable
using System;
using UnityEngine;

namespace Nuclear.Services
{
    public class MonoBehaviourProvider : MonoBehaviour, IUpdateProvider
    {
        public event Action OnUpdate = delegate { };

        private void Update()
        {
            OnUpdate.Invoke();
        }
    }
}