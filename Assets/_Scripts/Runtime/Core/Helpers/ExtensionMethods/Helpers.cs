using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class Helpers
{
    public static WaitForSeconds GetWaitForSeconds(float seconds)
    {
        return WaitFor.Seconds(seconds);
    }

    public static async UniTask LerpValueAsync<T>(
        T from,
        T to,
        float duration,
        Func<T, T, float, T> lerpFunc,
        Action<T> onUpdate,
        CancellationToken cancellationToken = default)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (cancellationToken.IsCancellationRequested) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            onUpdate?.Invoke(lerpFunc(from, to, t));
            bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
            if (canceled) return;
        }

        onUpdate?.Invoke(to);
    }
}
