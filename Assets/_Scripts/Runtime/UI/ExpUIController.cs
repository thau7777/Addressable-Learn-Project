using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpUIController : MonoBehaviour
{
    [SerializeField] private Slider _expSlider;
    [SerializeField] private TextMeshProUGUI _levelText;

    private EventBinding<ExpProgressChangedEvent> _expBinding;
    private EventBinding<LevelUpEvent> _levelBinding;

    private void Awake()
    {
        if (_expSlider != null)
            _expSlider.value = 0;
        if (_levelText != null)
            _levelText.text = "1";
    }

    private void OnEnable()
    {
        _expBinding = new EventBinding<ExpProgressChangedEvent>(OnExpChanged);
        _levelBinding = new EventBinding<LevelUpEvent>(OnLevelUp);
        EventBus<ExpProgressChangedEvent>.Register(_expBinding);
        EventBus<LevelUpEvent>.Register(_levelBinding);
    }

    private void OnDisable()
    {
        EventBus<ExpProgressChangedEvent>.Deregister(_expBinding);
        EventBus<LevelUpEvent>.Deregister(_levelBinding);
    }

    private void OnExpChanged(ExpProgressChangedEvent evt)
    {
        if (_expSlider == null) return;
        _expSlider.value = evt.value01;
    }

    private void OnLevelUp(LevelUpEvent evt)
    {
        if (_levelText == null) return;
        _levelText.text = evt.NewLevel.ToString();
    }
}
