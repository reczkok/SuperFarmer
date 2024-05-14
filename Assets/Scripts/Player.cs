using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Dictionary<AnimalType, int> Animals { get; } = new();
    [field: SerializeField]
    public string PlayerName { get; set; }
    [field: SerializeField]
    public int PlayerNumber { get; set; }
    private readonly Dictionary<AnimalType, AnimalInfo> _animalInfos = new();
    private readonly Dictionary<AnimalType, TradeReferenceInfo> _tradeReferenceInfos = new();
    public List<TradeSelector> TradeSelectors { get; private set; } = new();
    private TextMeshProUGUI _playerNameText;
    private Canvas _tradeOverlay;
    private Canvas _referenceOverlay;

    private void Awake()
    {
        _playerNameText = transform.Find("PlayerTray").Find("PlayerName").GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        _tradeOverlay = transform.Find("TradeOverlay").GetComponent<Canvas>();
        _tradeOverlay.worldCamera = CameraScript.Instance.GetCamera();
        _referenceOverlay = transform.Find("ReferenceInfo").GetComponentInChildren<Canvas>(includeInactive: true);
        TradeSelectors = new List<TradeSelector>(GetComponentsInChildren<TradeSelector>(includeInactive: true));
        var animalInfos = GetComponentsInChildren<AnimalInfo>(includeInactive: true);
        foreach (var animalInfo in animalInfos)
        {
            if (_animalInfos.TryAdd(animalInfo.AnimalType, animalInfo)) continue;
            Debug.LogError($"Player {PlayerName} has multiple AnimalInfos for {animalInfo.AnimalType}");
        }
        var tradeReferenceInfos = GetComponentsInChildren<TradeReferenceInfo>(includeInactive: true);
        foreach (var tradeReferenceInfo in tradeReferenceInfos)
        {
            if (_tradeReferenceInfos.TryAdd(tradeReferenceInfo.AnimalType, tradeReferenceInfo)) continue;
            Debug.LogError($"Player {PlayerName} has multiple TradeReferenceInfos for {tradeReferenceInfo.AnimalType}");
        }
    }

    public void Start()
    {
        ToggleTradeOverlay(false);
        foreach (var animalInfo in _animalInfos.Values)
        {
            Animals.Add(animalInfo.AnimalType, 0);
            animalInfo.SetCount(0);
        }
        _playerNameText.text = PlayerName;
    }
    
    public void ToggleTradeOverlay(bool active)
    {
        _tradeOverlay.gameObject.SetActive(active);
        _referenceOverlay.gameObject.SetActive(active);
    }
    
    public int GetAnimalCount(AnimalType animalType)
    {
        return Animals[animalType];
    }
    
    public void ModifyAnimalCount(AnimalType animalType, int amount)
    {
        if (Animals[animalType] + amount < 0)
        {
            Debug.LogError($"Player {PlayerName} has negative {animalType} count");
            return;
        }
        Animals[animalType] += amount;
        _animalInfos[animalType].SetCount(Animals[animalType]);
    }
    
    public void UpdateWithDictionary(Dictionary<AnimalType, int> animals)
    {
        foreach (var (animalType, count) in animals)
        {
            ModifyAnimalCount(animalType, count);
        }
    }

    public void UpdateTradeReferenceInfo(Player other)
    {
        foreach (var (animalType, count) in other.Animals)
        {
            _tradeReferenceInfos[animalType].SetCount(count);
        }
    }
    
    public void UpdateTradeReferenceInfo(AnimalBank bank)
    {
        foreach (var (animalType, count) in bank.GetAnimalAmounts())
        {
            _tradeReferenceInfos[animalType].SetCount(count);
        }
    }

    public void HandleWolfAttack()
    {
        if(Animals[AnimalType.BigDog] > 0)
        {
            ModifyAnimalCount(AnimalType.BigDog, -1);
            GameManager.Instance.AnimalBank.ModifyAnimalAmount(AnimalType.BigDog, 1);
            GUIManager.Instance.SetInfoTextForSeconds("Wolf attack repelled by Big Dog", 2);
            return;
        }
        
        var toRemove = new Dictionary<AnimalType, int>();
        foreach (var (animalType, count) in Animals)
        {
            if (count == 0 || animalType == AnimalType.Rabbit) continue;
            toRemove.Add(animalType, count);
            break;
        }
        foreach (var (animalType, count) in toRemove)
        {
            ModifyAnimalCount(animalType, -count);
            GameManager.Instance.AnimalBank.ModifyAnimalAmount(animalType, count);
        }
    }
    
    public void HandleFoxAttack()
    {
        if(Animals[AnimalType.SmallDog] > 0)
        {
            ModifyAnimalCount(AnimalType.SmallDog, -1);
            GameManager.Instance.AnimalBank.ModifyAnimalAmount(AnimalType.SmallDog, 1);
            GUIManager.Instance.SetInfoTextForSeconds("Fox attack repelled by Small Dog", 2);
            return;
        }
        
        var rabbitCount = Animals[AnimalType.Rabbit];
        if (rabbitCount <= 1) return;
        GameManager.Instance.AnimalBank.ModifyAnimalAmount(AnimalType.Rabbit, rabbitCount - 1);
        ModifyAnimalCount(AnimalType.Rabbit, -(rabbitCount - 1));
    }
    
    public void MultiplyAnimals(Dictionary<AnimalType, int> animals)
    {
        foreach (var (animalType, count) in animals)
        {
            if (count == 0) continue;
            var animalTotal = count + Animals[animalType];
            var toAdd = animalTotal / 2;
            toAdd = Math.Min(toAdd, GameManager.Instance.AnimalBank.GetAnimalAmount(animalType));
            ModifyAnimalCount(animalType, toAdd);
            GameManager.Instance.AnimalBank.ModifyAnimalAmount(animalType, -toAdd);
        }
    }
    
    public void ResetTradeSelectors()
    {
        foreach (var tradeSelector in TradeSelectors)
        {
            tradeSelector.ResetCount();
        }
    }
}
