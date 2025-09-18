#nullable enable
using Newtonsoft.Json;
using Nuclear.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NuclearBand.Game
{
    public sealed class GameEntry : LifetimeScope
    {
        [SerializeField] private bool _startClean;
        [SerializeField] private float _debugBonus;
        [SerializeField] private string _saveString = string.Empty;
        public static float DebugBonus { get; private set; }
        private IObjectResolver _di = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterBuildCallback(container =>
            {
                _di = container;
                container.Resolve<IMiningManager>();
                var windowsService = container.Resolve<IWindowsService>();
                windowsService.CreateWindow<GreetingWindow, IGreetingWindowViewModel>();
            });
        }

        protected override void Awake()
        {
            if (_startClean)
            {
                PlayerPrefs.DeleteAll();
            }

            DebugBonus = _debugBonus;
            using (Enqueue(new WindowServiceInstaller()))
            using (Enqueue(new FeatureInstallers()))
            using (Enqueue(new ViewModelsInstaller()))
            {
                base.Awake();
            }
            DontDestroyOnLoad(gameObject);
        }

        [Button]
        public void Cheat()
        {
            var cm = _di.Resolve<ICurrenciesManager>();
            cm.SetNewCurrentValue(CurrencyType.Energy, cm.GetCurrency(CurrencyType.Energy).Max.CurrentValue);
            cm.SetNewCurrentValue(CurrencyType.Code, cm.GetCurrency(CurrencyType.Code).Max.CurrentValue);
            cm.SetNewCurrentValue(CurrencyType.Art, cm.GetCurrency(CurrencyType.Art).Max.CurrentValue);
            cm.SetNewCurrentValue(CurrencyType.Ideas, cm.GetCurrency(CurrencyType.Ideas).Max.CurrentValue);
        }
        
        [Button]
        public void SetSaveString()
        {
            var saver = _di.Resolve<ISaver>();
            var s= JsonConvert.SerializeObject(saver.Save);
            _saveString = s;
        }
        
        [Button]
        public void LoadSaveString()
        {
            PlayerPrefs.SetString("Save", _saveString);
        }
    }
}