using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("REFERENCES")] 
    [SerializeField] private Image healthImage;
    [SerializeField] private Image manaImage;
    [SerializeField] private StatusManager _statusManager;

    //private methods
    private void UpdateHealthUI() => healthImage.fillAmount = _statusManager.Health / 100;
    private void UpdateManaUI() => manaImage.fillAmount = _statusManager.Mana / 100;

    //public methods
    public void AddUIEvents(StatusManager statusManager)
    {
        _statusManager = statusManager;
        statusManager.onChangeHealt += UpdateHealthUI;
        statusManager.onChangeMana += UpdateManaUI;
    }
}
