using System;
using LokiInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffCard : MonoBehaviour, IPointerClickHandler
{
    [TabGroup("Refs")]
    [SerializeField] private Image _iconImage;

    [TabGroup("Refs")]
    [SerializeField] private TMP_Text _nameText;

    [TabGroup("Refs")]
    [SerializeField] private TMP_Text _descriptionText;

    [TabGroup("Rarity Refs")]
    [SerializeField] private Image _frameImage;

    [TabGroup("Rarity Refs")]
    [SerializeField] private Image _backgroundImage;

    [TabGroup("Rarity Refs")]
    [SerializeField] private Transform _fxRoot;

    [TabGroup("Rarity Refs")]
    [SerializeField] private bool _tintNameWithAccent = true;

    private BuffSO _buff;
    private Action<BuffSO> _onSelected;
    private GameObject _spawnedFx;

    public void Bind(BuffSO buff, Action<BuffSO> onSelected)
    {
        _buff = buff;
        _onSelected = onSelected;

        if (_iconImage != null) _iconImage.sprite = buff.Icon;
        if (_nameText != null) _nameText.text = buff.DisplayName;
        if (_descriptionText != null) _descriptionText.text = buff.GetRuntimeDescription();

        ApplyRarityVisuals(buff.RarityVisuals);
    }

    private void OnDisable()
    {
        DestroySpawnedFx();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_buff != null) _onSelected?.Invoke(_buff);
    }

    private void ApplyRarityVisuals(RarityVisualsSO rv)
    {
        DestroySpawnedFx();

        if (rv == null) return;

        if (_frameImage != null)
        {
            if (rv.FrameSprite != null) _frameImage.sprite = rv.FrameSprite;
            _frameImage.color = rv.AccentColor;
        }

        if (_backgroundImage != null && rv.BackgroundSprite != null)
        {
            _backgroundImage.sprite = rv.BackgroundSprite;
        }

        if (_tintNameWithAccent && _nameText != null)
        {
            _nameText.color = rv.AccentColor;
        }

        if (_fxRoot != null && rv.CardFxPrefab != null)
        {
            _spawnedFx = Instantiate(rv.CardFxPrefab, _fxRoot);
            _spawnedFx.transform.localPosition = Vector3.zero;
            _spawnedFx.transform.localRotation = Quaternion.identity;
            _spawnedFx.transform.localScale = Vector3.one;
        }
    }

    private void DestroySpawnedFx()
    {
        if (_spawnedFx == null) return;
        Destroy(_spawnedFx);
        _spawnedFx = null;
    }
}
