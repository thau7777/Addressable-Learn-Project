using System.Collections.Generic;
using UnityEngine;
public class BuffManager : Singleton<BuffManager>
{
    private readonly List<BuffSO> _activeBuffs = new();
    [SerializeField]
    private List<BuffSO> _testBuffs = new();

    private void Start()
    {
        foreach(var buff in _testBuffs)
        {
            ApplyBuff(buff);
        }
    }
    public void ApplyBuff(BuffSO buff)
    {
        _activeBuffs.Add(buff);
        buff.Apply();
    }

    public void RemoveBuff(BuffSO buff)
    {
        if (!_activeBuffs.Remove(buff)) return;
        buff.Remove();
    }
}
