using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerManager : MonoBehaviour
{
    [Header("REFERENCES")] 
    [SerializeField] private Image healthImage;
    [SerializeField] private StatusManager _statusManager;
    private bool myHealth = false;

    //private methods
    private void UpdateHealthUI() => healthImage.fillAmount = 
        (myHealth ? _statusManager.health : _statusManager.opHealth) / _statusManager.maxHealth;

    //public methods
    public void AddUIEvents(StatusManager statusManager)
    {
        myHealth = true;
        _statusManager = statusManager;
        statusManager.onChangeHealt += UpdateHealthUI;
    }
    
    public void AddOpUIEvents(StatusManager statusManager)
    {
        _statusManager = statusManager;
        statusManager.onOpHpChange += UpdateHealthUI;
    }
}
