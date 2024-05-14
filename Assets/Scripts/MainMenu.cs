using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [field: SerializeField]
    public List<GameObject> PlayerInputFields { get; private set; } = new();

    private void Awake()
    {
        var go = GameObject.FindGameObjectsWithTag("PlayerNameInputField");
        foreach (var playerInputField in go)
        {
            PlayerInputFields.Add(playerInputField);
        }
    }
    
    public void StartGame()
    {
        var playerNames = PlayerInputFields
            .Where(x => x.GetComponent<TMP_InputField>().text.Trim() != "")
            .Select(x => x.GetComponent<TMP_InputField>().text).ToList();
        if (playerNames.Count < 2)
        {
            Debug.LogError("Not enough players");
            return;
        }
        GameData.Instance.PlayerNames = playerNames;
        SceneManager.LoadScene("GameScene");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
