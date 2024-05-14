using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [field: SerializeField]
    public GameState CurrentState { get; private set; } = GameState.StartGame;
    [field: SerializeField]
    public int PlayerCount { get; private set; } = 2;
    [field: SerializeField]
    public int CurrentPlayerNumber { get; private set; } = 1;
    [field: SerializeField]
    public List<Player> Players { get; private set; } = new();
    [field: SerializeField]
    public AnimalBank AnimalBank { get; private set; } = new();
    private Table _table;
    private bool _nextState;
    private bool _nextPlayer;
    private DiceThrowHandler _diceThrowHandler;
    public bool DebugAddAllAnimalsToCurrentPlayer;

    private void Awake()
    {
        Instance = this;
        _table = GetComponentInChildren<Table>();
        var dice = new List<AnimalDice>();
        dice.AddRange(GetComponentsInChildren<AnimalDice>());
        _diceThrowHandler = new DiceThrowHandler(dice);
    }
    
    private void Start()
    {
        var playerNames = GameData.Instance.PlayerNames;
        Players = _table.CreatePlayers(playerNames);
        PlayerCount = Players.Count;
        StartCoroutine(GameStart());
    }

    private void Update()
    {
        if (DebugAddAllAnimalsToCurrentPlayer)
        {
            DebugAddAllAnimalsToCurrentPlayer = false;
            foreach (var animalType in Enum.GetValues(typeof(AnimalType)))
            {
                Players[CurrentPlayerNumber - 1].ModifyAnimalCount((AnimalType) animalType, 1);
            }
        }
        if (_nextState) SwitchState();
        else return;
        if (_nextPlayer) SwitchPlayer();
        
        switch (CurrentState)
        {
            case GameState.StartGame:
                break;
            case GameState.Exchange:
                CameraScript.Instance.SetCameraToPlayer(CurrentPlayerNumber);
                Exchange();
                break;
            case GameState.RollDice:
                CameraScript.Instance.SetCameraPosition(CameraPositionType.DiceView);
                if (CheckForWinner(out var winner))
                {
                    CameraScript.Instance.SetCameraPosition(CameraPositionType.EndGame);
                    GUIManager.Instance.SetInfoTextForSeconds($"{winner.PlayerName} wins!", 5);
                    CurrentState = GameState.EndGame;
                    _nextState = true;
                    return;
                }
                GUIManager.Instance.DisplayRollDicePanel();
                break;
            case GameState.EndGame:
                CameraScript.Instance.SetCameraPosition(CameraPositionType.EndGame);
                StartCoroutine(WaitForEndGame());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator WaitForEndGame()
    {
        yield return new WaitForSeconds(5);
        QuitToMenu();
    }

    public void NextState()
    {
        _nextState = true;
    }

    private void SwitchState()
    {
        _nextState = false;
        CurrentState = CurrentState switch
        {
            GameState.StartGame => GameState.Exchange,
            GameState.Exchange => GameState.RollDice,
            GameState.RollDice => GameState.Exchange,
            GameState.EndGame => GameState.EndGame,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public void NextPlayer()
    {
        _nextPlayer = true;
    }
    
    private void SwitchPlayer()
    {
        _nextPlayer = false;
        CurrentPlayerNumber = CurrentPlayerNumber == PlayerCount ? 1 : CurrentPlayerNumber + 1;
        GUIManager.Instance.UpdateTradeWithPlayerButtons();
    }

    private IEnumerator GameStart()
    {
        yield return new WaitForSeconds(0.1f);
        GUIManager.Instance.SetInfoTextForSeconds("Game starting!", 4);
        yield return new WaitForSeconds(2);
        Players.ForEach(player => player.ModifyAnimalCount(AnimalType.Rabbit, 1));
        _nextState = true;
    }
    
    private void Exchange()
    {
        GUIManager.Instance.DisplayExchangeScreen();
    }
    
    public void RollDice()
    {
        _diceThrowHandler.ThrowAll();
        StartCoroutine(WaitForDiceThrow());
    }
    
    private IEnumerator WaitForDiceThrow()
    {
        yield return new WaitUntil(() => _diceThrowHandler.ThrowDone);
        HandleRollResults(_diceThrowHandler.GetDiceFaces());
    }

    private void HandleRollResults(List<DiceFace> faces)
    {
        if (faces.Contains(DiceFace.Wolf))
        {
            GUIManager.Instance.SetInfoTextForSeconds("Wolf! You lose all your animals", 2);
            Players[CurrentPlayerNumber - 1].HandleWolfAttack();
        }
        if (faces.Contains(DiceFace.Fox))
        {
            GUIManager.Instance.SetInfoTextForSeconds("Fox! You lose all but one rabbit", 2);
            Players[CurrentPlayerNumber - 1].HandleFoxAttack();
        }
        
        var animalCounts = new Dictionary<AnimalType, int>
        {
            {AnimalType.Rabbit, 0},
            {AnimalType.Sheep, 0},
            {AnimalType.Pig, 0},
            {AnimalType.Cow, 0},
            {AnimalType.Horse, 0}
        };
        
        foreach (var face in faces)
        {
            if (face == DiceFace.Rabbit) animalCounts[AnimalType.Rabbit]++;
            if (face == DiceFace.Sheep) animalCounts[AnimalType.Sheep]++;
            if (face == DiceFace.Pig) animalCounts[AnimalType.Pig]++;
            if (face == DiceFace.Cow) animalCounts[AnimalType.Cow]++;
            if (face == DiceFace.Horse) animalCounts[AnimalType.Horse]++;
        }
        
        Players[CurrentPlayerNumber - 1].MultiplyAnimals(animalCounts);
        if (CheckForWinner(out var winner))
        {
            GUIManager.Instance.SetInfoTextForSeconds($"{winner.PlayerName} wins!", 5);
            CurrentState = GameState.EndGame;
            _nextState = true;
            return;
        }
        var delay = faces.Contains(DiceFace.Wolf) || faces.Contains(DiceFace.Fox) ? 2f : 0f;
        StartCoroutine(NextPlayerAfterSeconds(2, delay));
    }
    
    private IEnumerator NextPlayerAfterSeconds(float seconds, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        var nextPlayer = CurrentPlayerNumber == PlayerCount ? 1 : CurrentPlayerNumber + 1;
        var nextPlayerName = Players[nextPlayer - 1].PlayerName;
        GUIManager.Instance.SetInfoTextForSeconds($"{nextPlayerName}'s turn", seconds);
        yield return new WaitForSeconds(seconds);
        NextPlayer();
        NextState();
    }
    
    private bool CheckForWinner(out Player winner)
    {
        foreach (var player in Players)
        {
            if (player.Animals[AnimalType.Rabbit] > 0 &&
                player.Animals[AnimalType.Sheep] > 0 &&
                player.Animals[AnimalType.Pig] > 0 &&
                player.Animals[AnimalType.Cow] > 0 &&
                player.Animals[AnimalType.Horse] > 0)
            {
                winner = player;
                return true;
            }
        }
        winner = null;
        return false;
    }
    

    public void QuitToMenu()
    {
        GameData.Instance.PlayerNames.Clear();
        SceneManager.LoadScene("MainMenu");
    }
}

public class AnimalBank
{
    [field: SerializeField]
    public int RabbitAmount { get; set; } = 60;
    [field: SerializeField]
    public int SheepAmount { get; set; } = 24;
    [field: SerializeField]
    public int PigAmount { get; set; } = 20;
    [field: SerializeField]
    public int CowAmount { get; set; } = 12;
    [field: SerializeField]
    public int HorseAmount { get; set; } = 6;
    [field: SerializeField]
    public int SmallDogAmount { get; set; } = 4;
    [field: SerializeField]
    public int BigDogAmount { get; set; } = 2;
    
    public int GetAnimalAmount(AnimalType animalType)
    {
        return animalType switch
        {
            AnimalType.Rabbit => RabbitAmount,
            AnimalType.Sheep => SheepAmount,
            AnimalType.Pig => PigAmount,
            AnimalType.Cow => CowAmount,
            AnimalType.Horse => HorseAmount,
            AnimalType.SmallDog => SmallDogAmount,
            AnimalType.BigDog => BigDogAmount,
            _ => throw new ArgumentOutOfRangeException(nameof(animalType), animalType, null)
        };
    }
    
    public Dictionary<AnimalType, int> GetAnimalAmounts()
    {
        return new Dictionary<AnimalType, int>
        {
            {AnimalType.Rabbit, RabbitAmount},
            {AnimalType.Sheep, SheepAmount},
            {AnimalType.Pig, PigAmount},
            {AnimalType.Cow, CowAmount},
            {AnimalType.Horse, HorseAmount},
            {AnimalType.SmallDog, SmallDogAmount},
            {AnimalType.BigDog, BigDogAmount}
        };
    }
    
    public void ModifyAnimalAmount(AnimalType animalType, int amount)
    {
        switch (animalType)
        {
            case AnimalType.Rabbit:
                RabbitAmount += amount;
                break;
            case AnimalType.Sheep:
                SheepAmount += amount;
                break;
            case AnimalType.Pig:
                PigAmount += amount;
                break;
            case AnimalType.Cow:
                CowAmount += amount;
                break;
            case AnimalType.Horse:
                HorseAmount += amount;
                break;
            case AnimalType.SmallDog:
                SmallDogAmount += amount;
                break;
            case AnimalType.BigDog:
                BigDogAmount += amount;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(animalType), animalType, null);
        }
    }
    
    public void UpdateWithDictionary(Dictionary<AnimalType, int> dictionary)
    {
        foreach (var (key, value) in dictionary)
        {
            ModifyAnimalAmount(key, value);
        }
    }
}

public enum GameState
{
    StartGame,
    Exchange,
    RollDice,
    EndGame,
}
