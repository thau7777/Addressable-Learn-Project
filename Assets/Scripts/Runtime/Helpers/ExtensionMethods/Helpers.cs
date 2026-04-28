using System;
using System.Collections;
using UnityEngine;

public static class Helpers
{
    public static WaitForSeconds GetWaitForSeconds(float seconds)
    {
        return WaitFor.Seconds(seconds);
    }
    public static IEnumerator LerpValue<T>(
    T from,
    T to,
    float duration,
    Func<T, T, float, T> lerpFunc,
    Action<T> onUpdate)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            T currentValue = lerpFunc(from, to, t);
            onUpdate?.Invoke(currentValue);
            yield return null;
        }

        onUpdate?.Invoke(to);
    }

}
