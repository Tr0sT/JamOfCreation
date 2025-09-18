#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace NuclearBand.Game
{
    public sealed class TimeManager : ITimeManager
    {
        private readonly ReactiveProperty<float> _seconds = new ReactiveProperty<float>(0f);
        private ReadOnlyReactiveProperty<float>? _secondsReadOnly;
        private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();
        private int _disposed;

        public TimeManager()
        {
            _seconds.Value = Time.time;
            _secondsReadOnly = _seconds.ToReadOnlyReactiveProperty();
            // Запускаем обновление каждое обновление кадра
            _ = UpdateLoopAsync(_lifetimeCts.Token);
        }
        
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            try { _lifetimeCts.Cancel(); } catch { }
            _lifetimeCts.Dispose();

            try { _secondsReadOnly?.Dispose(); } catch { }
            try { _seconds.Dispose(); } catch { }
        }

        public ReadOnlyReactiveProperty<float> SecondsFromStart => _secondsReadOnly!;

        public IDisposable ScheduleAction(float delay, Action action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            var cts = new CancellationTokenSource();
            _ = RunScheduledActionAsync(cts.Token, delay, action);
            return new CancellationDisposable(cts);
        }

        private async UniTaskVoid UpdateLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _seconds.Value = Time.time;
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                // ожидаемо при отмене
            }
        }

        private async UniTaskVoid RunScheduledActionAsync(CancellationToken token, float delay, Action action)
        {
            try
            {
                // задержка в миллисекундах, поддержка отмены
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token, ignoreTimeScale:false);
                if (!token.IsCancellationRequested)
                    action();
            }
            catch (OperationCanceledException)
            {
                // отменено — ничего не делаем
            }
        }

        // Простейшая обёртка, возвращающая IDisposable для отмены запланированного действия
        private sealed class CancellationDisposable : IDisposable
        {
            private CancellationTokenSource? _cts;
            public CancellationDisposable(CancellationTokenSource cts) => _cts = cts;
            public void Dispose()
            {
                var cts = Interlocked.Exchange(ref _cts, null);
                if (cts is null) return;
                try { cts.Cancel(); } catch { }
                cts.Dispose();
            }
        }

        
    }
}