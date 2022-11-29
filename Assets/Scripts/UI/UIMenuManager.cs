using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    #region Public Methods
    
    public void ConnectedSuccess() => LogMessage(successConnectionMessage + "\n" + waitOpponentMessage);

    public void GoToStartGame()
    {
        mainMenuObj ??= this.gameObject.GetComponent<Canvas>();
        mainMenuObj.enabled = false;
    }

    public void BackToMenu()
    {
        //refs.localManager.DisconnectToServer();
        mainMenuObj.enabled = true;
    }
    
    #endregion
    
    #region Private Methods
    
    private void Start()
    {
        exitButton.onClick.AddListener(ExitGame);
        startButton.onClick.AddListener(StartGame);
        refs.myTugboat.NetworkManager.onCustomConnectError += OnConnectError;
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

    private void LogMessage(string message) => labelLogs.text = message;

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
