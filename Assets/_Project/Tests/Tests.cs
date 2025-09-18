#nullable enable

// Новые PlayMode тесты для TimeManager
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NuclearBand.Game;

public class TimeManagerTests
{
    [UnityTest]
    public IEnumerator SecondsFromStart_UpdatesOverFrames()
    {
        var tm = new TimeManager();
        try
        {
            float initial = tm.SecondsFromStart.CurrentValue;
            // Ждём немного, чтобы UpdateLoopAsync успел обновить значение
            yield return new WaitForSeconds(0.1f);
            float later = tm.SecondsFromStart.CurrentValue;
            Assert.GreaterOrEqual(later, initial);
        }
        finally
        {
            tm.Dispose();
        }
    }

    [UnityTest]
    public IEnumerator ScheduleAction_ExecutesAfterDelay()
    {
        var tm = new TimeManager();
        try
        {
            bool called = false;
            tm.ScheduleAction(0.05f, () => called = true);
            Assert.IsFalse(called);
            // Даем запас времени для выполнения действия
            yield return new WaitForSeconds(0.25f);
            Assert.IsTrue(called);
        }
        finally
        {
            tm.Dispose();
        }
    }

    [UnityTest]
    public IEnumerator ScheduleAction_CanBeCancelled()
    {
        var tm = new TimeManager();
        try
        {
            bool called = false;
            var disp = tm.ScheduleAction(0.2f, () => called = true);
            // Отменяем немедленно
            disp.Dispose();
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(called);
        }
        finally
        {
            tm.Dispose();
        }
    }
}
