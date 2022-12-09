using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tools;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuManager : Tools.Singleton<UIMenuManager>
{
    [Header("REFERENCES")] 
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TextMeshProUGUI labelLogs;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Canvas mainMenuObj;

    private ScriptsReferences refs => ScriptsReferences.Instance;

    private const string incorrectIpMessage = "Address was entered incorrectly. Please try again.";
    private const string connectionErrorMessage = "Connection error. Please try again.";
    private const string waitOpponentMessage = "Waiting for an opponent.";
    private const string successConnectionMessage = "Connected successfully.";
    private const string enterIpMessage = "Enter server adress and start...";

    #region Public Methods
    
    public void ConnectedSuccess() => LogMessage(successConnectionMessage + "\n" + waitOpponentMessage);

    public void GoToStartGame()
    {
        mainMenuObj ??= this.gameObject.GetComponent<Canvas>();
        mainMenuObj.enabled = false;
        refs.localManager.ChangeLocalState(LOCAL_STATE.GAMEPLAY);
    }

    public void BackToMenu()
    {
        //refs.localManager.DisconnectToServer();
        mainMenuObj.enabled = true;
        LogMessage(enterIpMessage);
        refs.localManager.ChangeLocalState(LOCAL_STATE.MENU);
    }

    public void LogMessage(string message) => labelLogs.text = message;
    
    #endregion
    
    #region Private Methods
    
    private void Start()
    {
        exitButton.onClick.AddListener(ExitGame);
        startButton.onClick.AddListener(StartGame);
        refs.myTugboat.NetworkManager.onCustomConnectError += OnConnectError;
        LogMessage(enterIpMessage);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void StartGame()
    {
        if (ipInputField.text is "")
        {
            LogMessage(incorrectIpMessage);
            return;
        }
        refs.myTugboat.SetClientAddress(ipInputField.text);
        refs.myNetHudCanvas.OnlyConnect();
    }

    private void OnConnectError()
    {
        LogMessage(connectionErrorMessage);
    }

    private void OnDisable()
    {
        refs.myTugboat.NetworkManager.onCustomConnectError -= OnConnectError;
    }
    
    #endregion
}
