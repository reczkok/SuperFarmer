using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeSelector : MonoBehaviour
{
    [field: SerializeField]
    public AnimalType AnimalType { get; private set; }
    [field: SerializeField]
    public int Count { get; private set; }
    
    private Button _addButton;
    private Button _subtractButton;
    private TextMeshProUGUI _countText;
    
    private void Awake()
    {
        _addButton = transform.Find("AddButton").GetComponent<Button>();
        _subtractButton = transform.Find("SubtractButton").GetComponent<Button>();
        _countText = transform.Find("CountText").GetComponent<TextMeshProUGUI>();
        
        _addButton.onClick.AddListener(TryAdd);
        _subtractButton.onClick.AddListener(TrySubtract);
    }
    
    public void TryAdd()
    {
        if(!TradeManager.CurrentTransaction.TryAdd(this)) return;
        TradeManager.UpdateConfirmButton();
        Count++;
        _countText.text = $"{Count}";
    }
    
    public void TrySubtract()
    {
        if(!TradeManager.CurrentTransaction.TrySubtract(this)) return;
        TradeManager.UpdateConfirmButton();
        Count--;
        _countText.text = $"{Count}";
    }
    
    public void ResetCount()
    {
        Count = 0;
        _countText.text = $"{Count}";
    }
}
