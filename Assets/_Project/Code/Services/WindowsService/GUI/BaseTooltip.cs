#nullable enable
using UnityEngine;

namespace Nuclear.Services.GUI
{
    public abstract class BaseTooltip<TViewModel> : MWindow<TViewModel> where TViewModel : IViewModel
    {
        public void SetAnchor(RectTransform anchor)
        {
            transform.position = anchor.position;
        }
    }
}