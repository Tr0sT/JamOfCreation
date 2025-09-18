#nullable enable
using R3;

namespace NuclearBand.Game
{
    public sealed class Currency : ICurrency
    {
        private readonly ReactiveProperty<int> _max = new();
        private readonly ReactiveProperty<float> _current = new();
        private readonly ReactiveProperty<float> _regenerationRate = new();
        private readonly ReactiveProperty<bool> _visible = new();

        // хранит желаемую (базовую) скорость регенерации, которую можно переопределить когда Current > Max
        private float _baseRegenerationRate;
        private float _maxCoeff = 1.0f;
        private float _regenCoeff = 1.0f;
        private int _maxValue;
        private int _regenValue;

        public Currency(CurrencyType currencyType, int current, int max, float regenerationRate, bool visible)
        {
            CurrencyType = currencyType;
            _max.Value = _maxValue = max;
            _current.Value = current;
            _baseRegenerationRate = regenerationRate;
            _visible.Value = visible;

            // устанавливаем фактическую скорость с учётом текущего состояния
            UpdateRegenerationRate();
        }

        public CurrencyType CurrencyType { get; }
        public ReadOnlyReactiveProperty<int> Max => _max;
        public ReadOnlyReactiveProperty<float> Current => _current;
        public ReadOnlyReactiveProperty<float> RegenerationRate => _regenerationRate;
        public ReadOnlyReactiveProperty<bool> Visible => _visible;

        public void SetNewCurrentValue(float newCurrent)
        {
            _current.Value = newCurrent;
            UpdateRegenerationRate();
        }
        public void SetNewMaxValue(int newMax)
        {
            _maxValue = newMax;
            _max.Value = (int) (newMax * _maxCoeff);
            UpdateRegenerationRate();
        }
        public void SetNewRegenerationRate(float newRegenerationRate)
        {
            _baseRegenerationRate = newRegenerationRate;
            UpdateRegenerationRate();
        }

        public void Unlock() => _visible.Value = true;

        private void UpdateRegenerationRate()
        {
            // если текущее зна��ение больше максимума — принудительно ставим -0.2, иначе базовая скорость
            if (_current.Value > _max.Value)
                _regenerationRate.Value = -0.2f;
            else
                _regenerationRate.Value = _baseRegenerationRate * _regenCoeff;
        }

        public void SetLimitCoeff(float coeff)
        {
            _maxCoeff = coeff;
            SetNewMaxValue(_maxValue);
        }

        public void SetRegenCoeff(float coeff)
        {
            _regenCoeff = coeff;
            UpdateRegenerationRate();
        }
    }
}