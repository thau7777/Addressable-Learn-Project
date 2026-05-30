using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffSelector
{
    private readonly List<BuffSO> _buffer = new(30);
    private readonly List<float> _weights = new(30);

    public BuffSO[] Pick(IReadOnlyList<BuffSO> pool, ISet<BuffSO> active, float activeBoost, int count)
    {
        _buffer.Clear();
        _weights.Clear();
        if (pool == null || count <= 0) return Array.Empty<BuffSO>();

        float boostedWeight = 1f + Mathf.Max(0f, activeBoost);

        // setup each buff with its weight: rarity base weight * active boost (if active)
        for (int i = 0; i < pool.Count; i++)
        {
            var buff = pool[i];
            if (buff == null) continue;
            _buffer.Add(buff);
            float rarityWeight = buff.RarityVisuals != null ? buff.RarityVisuals.BaseWeight : 1f;
            float boost = active != null && active.Contains(buff) ? boostedWeight : 1f;
            _weights.Add(rarityWeight * boost);
        }

        // create result array with count
        int take = Mathf.Min(count, _buffer.Count);
        var result = new BuffSO[take];

        for (int picked = 0; picked < take; picked++)
        {
            float total = 0f;
            for (int i = picked; i < _buffer.Count; i++) total += _weights[i];

            float roll = UnityEngine.Random.Range(0f, total);
            int winner = _buffer.Count - 1;
            float acc = 0f;
            for (int i = picked; i < _buffer.Count; i++)
            {
                acc += _weights[i];
                if (roll < acc) { winner = i; break; }
            }

            (_buffer[picked], _buffer[winner]) = (_buffer[winner], _buffer[picked]);
            (_weights[picked], _weights[winner]) = (_weights[winner], _weights[picked]);
            result[picked] = _buffer[picked];
        }

        return result;
    }
}
