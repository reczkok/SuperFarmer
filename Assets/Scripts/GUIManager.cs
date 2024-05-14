using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }
    private Canvas _canvas;
    private TextMeshProUGUI _infoText;
    private GameObject _exchangeScreen;
    private GameObject _exchangeConfPanel;
    private GameObject _rollDicePanel;
    
    private Button _skipButton;
    private Button _tradeWithBankButton;
    private Dictionary<int, Button> _tradeWithPlayerButtons = new();
    
    private Button _cancelButton;
    private Button _confirmButton;
    public bool CanConfirm { set => _confirmButton.interactable = value; }
    
    private Button _rollButton;
    
    private void Awake()
    {
        Instance = this;
        _canvas = GetComponentInChildren<Canvas>();
        _infoText = _canvas.transform.Find("Info").GetComponentInChildren<TextMeshProUGUI>();
        
        _exchangeScreen = _canvas.transform.Find("Exchange").gameObject;
        _exchangeConfPanel = _canvas.transform.Find("ExchangeConf").gameObject;
        _rollDicePanel = _canvas.transform.Find("Roll").gameObject;
        _rollButton = _rollDicePanel.transform.Find("RollB").GetComponent<Button>();
        _rollButton.onClick.AddListener(() =>
        {
            HideAllPanels();
            GameManager.Instance.RollDice();
        });
        
        _skipButton = _exchangeScreen.transform.Find("SkipTrade").GetComponent<Button>();
        
        _tradeWithBankButton = _exchangeScreen.transform.Find("EWBB").GetComponent<Button>();
        
        _cancelButton = _exchangeConfPanel.transform.Find("Cancel").GetComponent<Button>();
        _cancelButton.onClick.AddListener(() =>
        {
            HideAllPanels();
            TradeManager.ClearCurrentTransaction();
            GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].ToggleTradeOverlay(false);
            DisplayExchangeScreen();
        });
        _confirmButton = _exchangeConfPanel.transform.Find("Confirm").GetComponent<Button>();
        _confirmButton.onClick.AddListener(() =>
        {
            TradeManager.CurrentTransaction.Execute();
            HideAllPanels();
            GameManager.Instance.NextState();
        });
    }

    private void Start()
    {
        var pCount = GameManager.Instance.PlayerCount;
        var pNames = GameData.Instance.PlayerNames;
        for (var i = 1; i <= 4; i++)
        {
            var button = _exchangeScreen.transform.Find($"EWP{i}").GetComponent<Button>();
            _tradeWithPlayerButtons.Add(i, button);
            button.interactable = true;
            if (i == GameManager.Instance.CurrentPlayerNumber)
            {
                button.interactable = false;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Trade with " + pNames[i - 1];
            }else if (i <= pCount)
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Trade with " + pNames[i - 1];
            } else
            {
                button.gameObject.SetActive(false);
            }
        }
        
        HideAllPanels();
        _infoText.text = "";
    }
    
    public void UpdateTradeWithPlayerButtons()
    {
        var pCount = GameManager.Instance.PlayerCount;
        var pNames = GameData.Instance.PlayerNames;
        for (var i = 1; i <= 4; i++)
        {
            var button = _tradeWithPlayerButtons[i];
            button.interactable = true;
            if (i == GameManager.Instance.CurrentPlayerNumber)
            {
                button.interactable = false;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Trade with " + pNames[i - 1];
            }else if (i <= pCount)
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Trade with " + pNames[i - 1];
            } else
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    public void SetInfoText(string text)
    {
        _infoText.text = text;
    }
    
    public void SetInfoTextForSeconds(string text, float seconds)
    {
        _infoText.text = text;
        Instance.StartCoroutine(ClearTextAfterSeconds(seconds));
    }

    private IEnumerator ClearTextAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _infoText.text = "";
    }
    
    public void DisplayExchangeScreen()
    {
        _exchangeScreen.SetActive(true);
    }
    
    public void DisplayRollDicePanel()
    {
        _rollDicePanel.SetActive(true);
    }
    
    public void HideAllPanels()
    {
        _exchangeScreen.SetActive(false);
        _exchangeConfPanel.SetActive(false);
        _rollDicePanel.SetActive(false);
    }

    public void ShowExchangeConfPanel()
    {
        _exchangeConfPanel.SetActive(true);
    }
    
    public void QuitToMenu()
    {
        GameManager.Instance.QuitToMenu();
    }
}
