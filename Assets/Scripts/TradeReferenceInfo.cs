using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TradeReferenceInfo : MonoBehaviour
{
    [field: SerializeField]
    public AnimalType AnimalType { get; set; }
    [field: SerializeField]
    public int Count { get; set; }
    private TextMeshProUGUI _countText;
    
    private void Awake()
    {
        _countText = GetComponentInChildren<TextMeshProUGUI>();
        _countText.text = $"{Count}";
    }
    
    public void SetCount(int count)
    {
        Count = count;
        _countText.text = $"{Count}";
    }
}
