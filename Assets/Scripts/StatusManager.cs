using System;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Broadcast;

public class StatusManager : NetworkBehaviour
{
    [Header("STATUS")] 
    public float health;
    public float maxHealth;

    [Header("REFERENCES")]
    public GameObject uiPrefab;
    private UIManager _uiManager;
    private ScriptsReferences refs => ScriptsReferences.Instance;

    #region Events
    
    public delegate void HealthChange();
    public HealthChange onChangeHealt;
    
    #endregion
    
    #region Public methods
    
    public void ChangeHealth(float value)
    {
        health = value;
        if (health > maxHealth)
            health = maxHealth;
        onChangeHealt?.Invoke();
    }

    public void InitHealth(float value)
    {
        health = maxHealth = value;
        InitializeUI();
    }
    
    #endregion
    
    #region Private methods

    private void InitializeUI()
    {
        var playerUI = Instantiate(uiPrefab, refs.uiCanvasObject.transform);
        _uiManager = playerUI.GetComponent<UIManager>();
        _uiManager.AddUIEvents(this);
        onChangeHealt?.Invoke();
    }

    private void OnDestroy()
    {
        Destroy(_uiManager.gameObject);
    }
    
    #endregion

    #region Network methods
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            GetComponent<StatusManager>().enabled = false;
        }
        else
        {
            refs.myStatusManager = this;
        }
    }

    [ObserversRpc]
    public void ChangeHealth(GameObject obj, float value) => obj.GetComponent<StatusManager>().ChangeHealth(value);

    [ServerRpc]
    public void ChangeHealthServer(float value) => ChangeHealth(this.gameObject, value);
    
    #endregion
}
