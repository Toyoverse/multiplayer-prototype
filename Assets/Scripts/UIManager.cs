using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("REFERENCES")] 
    [SerializeField] private Image healthImage;
    [SerializeField] private StatusManager _statusManager;
    [SerializeField] private TextMeshProUGUI roundCount;
    private ScriptsReferences refs => ScriptsReferences.Instance;

    //private methods
    private void UpdateHealthUI() => healthImage.fillAmount = _statusManager.health / _statusManager.maxHealth;

    //public methods
    public void AddUIEvents(StatusManager statusManager)
    {
        _statusManager = statusManager;
        statusManager.onChangeHealt += UpdateHealthUI;
        refs.localManager.onRoundStart += UpdateRoundUI;
    }

    public void UpdateRoundUI() => roundCount.text = refs.localManager.round.ToString();
}
