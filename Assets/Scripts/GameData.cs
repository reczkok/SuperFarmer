using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;
    [field: SerializeField]
    public List<string> PlayerNames { get; set; } = new();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
