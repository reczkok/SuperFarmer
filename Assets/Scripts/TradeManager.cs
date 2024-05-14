using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TradeManager : MonoBehaviour
{
    public static TradeManager Instance { get; private set; }
    public static Transaction CurrentTransaction { get; private set; }

    private static bool ValidateTransaction()
    {
        var allAnimals = CurrentTransaction.Offer.Values.Sum(Mathf.Abs);
        if (allAnimals == 0) return false;
        bool multiplePositive = CurrentTransaction.Offer.Count(animal => animal.Value > 0) > 1;
        bool multipleNegative = CurrentTransaction.Offer.Count(animal => animal.Value < 0) > 1;
        if (multiplePositive)
        {
            var negativeValue = CurrentTransaction.Offer.FirstOrDefault(animal => animal.Value < 0);
            if (negativeValue.Value != -1) return false;
        }
        if (multipleNegative)
        {
            var positiveValue = CurrentTransaction.Offer.FirstOrDefault(animal => animal.Value > 0);
            if (positiveValue.Value != 1) return false;
        }
        if (!multiplePositive && !multipleNegative)
        {
            // if there is one positive and one negative, one of the values must be 1 or -1
            var positiveValue = CurrentTransaction.Offer.FirstOrDefault(animal => animal.Value > 0);
            var negativeValue = CurrentTransaction.Offer.FirstOrDefault(animal => animal.Value < 0);
            if (!(positiveValue.Value == 1 || negativeValue.Value == -1)) return false;
        }
        var rabbitValue = CurrentTransaction.Offer.Sum(animal => Transaction.ToRabbit(animal.Key, animal.Value));
        return rabbitValue == 0;
    }

    public static void UpdateConfirmButton()
    {
        GUIManager.Instance.CanConfirm = ValidateTransaction();
    }
    
    private void Awake()
    {
        Instance = this;
    }

    public void SkipTrade()
    {
        GUIManager.Instance.HideAllPanels();
        GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].ToggleTradeOverlay(false);
        GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].ResetTradeSelectors();
        GameManager.Instance.NextState();
    }
    
    public void BeginTradeWithBank()
    {
        GUIManager.Instance.HideAllPanels();
        GUIManager.Instance.ShowExchangeConfPanel();
        GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].ToggleTradeOverlay(true);
        GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].UpdateTradeReferenceInfo(GameManager.Instance.AnimalBank);
        CurrentTransaction = new BankTrade {Bank = GameManager.Instance.AnimalBank};
        CurrentTransaction.Initiator = GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1];
        CurrentTransaction.Offer = new Dictionary<AnimalType, int>();
        
        foreach (var animal in GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].Animals)
        {
            CurrentTransaction.Offer.Add(animal.Key, 0);
        }
        
        GUIManager.Instance.CanConfirm = false;
    }

    public void BeginTradeWithPlayer(int playerNumber)
    {
        GUIManager.Instance.HideAllPanels();
        GUIManager.Instance.ShowExchangeConfPanel();
        GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].ToggleTradeOverlay(true);
        GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].UpdateTradeReferenceInfo(GameManager.Instance.Players[playerNumber - 1]);
        CurrentTransaction = new PlayerTrade {Receiver = GameManager.Instance.Players[playerNumber - 1]};
        CurrentTransaction.Initiator = GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1];
        CurrentTransaction.Offer = new Dictionary<AnimalType, int>();
        
        foreach (var animal in GameManager.Instance.Players[GameManager.Instance.CurrentPlayerNumber - 1].Animals)
        {
            CurrentTransaction.Offer.Add(animal.Key, 0);
        }
        
        GUIManager.Instance.CanConfirm = false;
    }

    public static void ClearCurrentTransaction()
    {
        CurrentTransaction = null;
    }
}

public abstract class Transaction
{
    public Player Initiator { get;  set; }
    public Dictionary<AnimalType, int> Offer { get; set; }
    public static int ToRabbit(AnimalType animalType, int count)
    {
        return animalType switch
        {
            AnimalType.Rabbit => count,
            AnimalType.Sheep => count * 6,
            AnimalType.Pig => count * 12,
            AnimalType.Cow => count * 36,
            AnimalType.Horse => count * 72,
            AnimalType.SmallDog => count * 6,
            AnimalType.BigDog => count * 36,
            _ => 0
        };
    }
    public static Dictionary<AnimalType, int> ReverseOffer(Dictionary<AnimalType, int> offer)
    {
        return offer.ToDictionary(animal => animal.Key, animal => -animal.Value);
    }
    
    public abstract void Execute();

    public abstract bool TryAdd(TradeSelector selector);
    public abstract bool TrySubtract(TradeSelector selector);
}

public class PlayerTrade : Transaction
{
    public Player Receiver { get;  set; }
    public override void Execute()
    {
        Initiator.ToggleTradeOverlay(false);
        Initiator.UpdateWithDictionary(Offer);
        Initiator.ResetTradeSelectors();
        Receiver.UpdateWithDictionary(ReverseOffer(Offer));
    }

    public override bool TryAdd(TradeSelector selector)
    {
        if (Receiver.GetAnimalCount(selector.AnimalType) - (selector.Count + 1) < 0) return false;
        if (selector.Count != 0)
        {
            Offer[selector.AnimalType] = selector.Count + 1;
            return true;
        }
        var neagtiveCount = Offer.Count(animal => animal.Value < 0);
        var positiveCount = Offer.Count(animal => animal.Value > 0);
        if(neagtiveCount > 1 && positiveCount == 1) return false;
        Offer[selector.AnimalType] = selector.Count + 1;
        return true;
    }

    public override bool TrySubtract(TradeSelector selector)
    {
        if (Initiator.GetAnimalCount(selector.AnimalType) + (selector.Count - 1) < 0) return false;
        if (selector.Count != 0)
        {
            Offer[selector.AnimalType] = selector.Count - 1;
            return true;
        }
        var neagtiveCount = Offer.Count(animal => animal.Value < 0);
        var positiveCount = Offer.Count(animal => animal.Value > 0);
        if(neagtiveCount == 1 && positiveCount > 1) return false;
        Offer[selector.AnimalType] = selector.Count - 1;
        return true;
    }
}

public class BankTrade : Transaction
{
    public AnimalBank Bank { get; set; }
    public override void Execute()
    {
        Initiator.ToggleTradeOverlay(false);
        Initiator.UpdateWithDictionary(Offer);
        Initiator.ResetTradeSelectors();
        Bank.UpdateWithDictionary(ReverseOffer(Offer));
    }
    
    public override bool TryAdd(TradeSelector selector)
    {
        if (Bank.GetAnimalAmount(selector.AnimalType) - (selector.Count + 1) < 0) return false;
        if (selector.Count != 0)
        {
            Offer[selector.AnimalType] = selector.Count + 1;
            return true;
        }
        var neagtiveCount = Offer.Count(animal => animal.Value < 0);
        var positiveCount = Offer.Count(animal => animal.Value > 0);
        if(neagtiveCount > 1 && positiveCount == 1) return false;
        Offer[selector.AnimalType] = selector.Count + 1;
        return true;
    }
    
    public override bool TrySubtract(TradeSelector selector)
    {
        if (Initiator.GetAnimalCount(selector.AnimalType) + (selector.Count - 1) < 0) return false;
        if (selector.Count != 0)
        {
            Offer[selector.AnimalType] = selector.Count - 1;
            return true;
        }
        var neagtiveCount = Offer.Count(animal => animal.Value < 0);
        var positiveCount = Offer.Count(animal => animal.Value > 0);
        if(neagtiveCount == 1 && positiveCount > 1) return false;
        Offer[selector.AnimalType] = selector.Count - 1;
        return true;
    }
}
