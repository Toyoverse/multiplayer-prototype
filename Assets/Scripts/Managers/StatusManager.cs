using System;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Broadcast;
using Tools;

public class StatusManager : MonoBehaviour
{
    [Header("STATUS")] 
    public float health;
    public float maxHealth;
    public float opHealth;

    [Header("REFERENCES")]
    public GameObject uiPrefab;
    public GameObject opUiPrefab;
    private UIPlayerManager _uiPlayerManager;
    private ScriptsReferences refs => ScriptsReferences.Instance;

    #region Events
    
    public delegate void HealthChange();
    public HealthChange onChangeHealt;
    
    public delegate void OpHpChange();
    public OpHpChange onOpHpChange;

    #endregion
    
    #region Public methods

    public void ChangeHealth(float value)
    {
        health = value;
        if (health > maxHealth)
            health = maxHealth;
        onChangeHealt?.Invoke();
    }
    
    public void ChangeOpHealth(float value)
    {
        opHealth = value;
        if (opHealth > maxHealth)
            opHealth = maxHealth;
        onOpHpChange?.Invoke();
    }

    public void InitHealth(float value)
    {
        InitializeUI();
        InitializeOpponentUI();
        health = maxHealth = value;
        //InitializeUI();
        ChangeHealth(value);
        ChangeOpHealth(value);
    }
    
    #endregion
    
    #region Private methods

    private void InitializeUI()
    {
        var playerUI = Instantiate(uiPrefab, refs.uiMyHpTarget.transform);
        _uiPlayerManager = playerUI.GetComponent<UIPlayerManager>();
        _uiPlayerManager.AddUIEvents(this);
        onChangeHealt?.Invoke();
    }
    
    private void InitializeOpponentUI()
    {
        var playerUI = Instantiate(opUiPrefab, refs.uiOpponentHpTarget.transform);
        _uiPlayerManager = playerUI.GetComponent<UIPlayerManager>();
        _uiPlayerManager.AddOpUIEvents(this);
        onOpHpChange?.Invoke();
    }

    private void OnDestroy()
    {
        if(health > 0)
            refs.localManager.OnSelfKick();
        
        if(_uiPlayerManager != null)
            Destroy(_uiPlayerManager.gameObject);
    }
    
    #endregion
}
