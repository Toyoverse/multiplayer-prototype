using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerManager : MonoBehaviour
{
    [Header("REFERENCES")] 
    [SerializeField] private Image healthImage;
    [SerializeField] private StatusManager _statusManager;
    [SerializeField] private TextMeshProUGUI comboText;
    private const string comboTitle = "Combo: ";
    private bool myHealth = false;

    //private methods
    private void UpdateHealthUI() => healthImage.fillAmount = 
        (myHealth ? _statusManager.health : _statusManager.opHealth) / _statusManager.maxHealth;

    //public methods
    public void AddUIEvents(StatusManager statusManager)
    {
        myHealth = true;
        _statusManager = statusManager;
        statusManager.onMyHpChange += UpdateHealthUI;
    }
    
    public void AddOpUIEvents(StatusManager statusManager)
    {
        _statusManager = statusManager;
        statusManager.onOpHpChange += UpdateHealthUI;
    }

    public void UpdateCombo(int value)
    {
        comboText.enabled = !(value <= 0);
        comboText.text = comboTitle + value;
    }
}
