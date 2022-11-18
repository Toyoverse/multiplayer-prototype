using System;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

public class StatusManager : NetworkBehaviour
{
    [Header("STATUS")] 
    public float Health;
    public float Mana;
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxMana;

    [Header("REFERENCES")] 
    private ScriptsReferences refs;
    public GameObject uiPrefab;
    private UIManager _uiManager;
    
    [Header("Values per Attack")] 
    [SerializeField] private float manaCost;
    [SerializeField] private float hpDamage;
    [Header("Mana regen config")]
    [SerializeField] private float manaRegenValue;
    [SerializeField] private float manaRegenTime;
    
    //events
    public delegate void HealthChange();
    public HealthChange onChangeHealt;
    public delegate void ManaChange();
    public ManaChange onChangeMana;

    //public methods
    public void AddHealth(float value)
    {
        Health += value;
        if (Health > maxHealth)
            Health = maxHealth;
        onChangeHealt?.Invoke();
    }

    public void AddMana(float value)
    {
        Mana += value;
        if (Mana > maxMana)
            Mana = maxMana;
        onChangeMana?.Invoke();
    }

    //private methods
    private void StartStatus()
    {
        //ResetStats();
        InvokeRepeating(nameof(ManaRegen), 0, manaRegenTime);
    }
    
    private void ResetStats()
    {
        Health = maxHealth;
        Mana = maxMana;
        onChangeHealt?.Invoke();
        onChangeMana?.Invoke();
    }

    private void Update()
    {
        if (refs == null)
            return;
        if (refs.playerInput.buttonA && Mana >= manaCost)
        {
            ChangeManaServer(this.gameObject, -manaCost);
            ChangeHealthServer(this.gameObject, -hpDamage);
        }
    }

    private void ManaRegen() => ChangeManaServer(this.gameObject, manaRegenValue);

    private void InitializeUI()
    {
        refs ??= FindObjectOfType<ScriptsReferences>();
        var playerUI = Instantiate(uiPrefab, refs.uiCanvasObject.transform);
        _uiManager = playerUI.GetComponent<UIManager>();
        _uiManager.AddUIEvents(this);
        /*if(IsOwner)
            ResetStats();*/
        onChangeHealt?.Invoke();
        onChangeMana?.Invoke();
    }

    private void OnDestroy()
    {
        Destroy(_uiManager.gameObject);
    }

    //network methods
    public override void OnStartClient()
    {
        base.OnStartClient();
        InitializeUI();
        if (base.IsOwner)
        {
            StartStatus();
        }
        else
        {
            GetComponent<StatusManager>().enabled = false;
        }
    }

    [ObserversRpc]
    public void ChangeMana(GameObject gameObject, float value) => gameObject.GetComponent<StatusManager>().AddMana(value);

    [ServerRpc]
    public void ChangeManaServer(GameObject gameObject, float value) => ChangeMana(gameObject, value);

    [ObserversRpc]
    public void ChangeHealth(GameObject gameObject, float value) => gameObject.GetComponent<StatusManager>().AddHealth(value);

    [ServerRpc]
    public void ChangeHealthServer(GameObject gameObject, float value) => ChangeHealth(gameObject, value);
}
