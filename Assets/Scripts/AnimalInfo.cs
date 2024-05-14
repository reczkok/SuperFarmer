using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimalInfo : MonoBehaviour
{
    [field: SerializeField] 
    public AnimalType AnimalType { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI Text { get; private set; }

    public void Awake()
    {
        Text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetCount(int count)
    {
        Text.text = $"{count}";
    }
}
