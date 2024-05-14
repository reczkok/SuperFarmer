using System.Collections.Generic;

public class DiceThrowHandler 
{
    public bool ThrowDone => _diceFaces.Count == _dice.Count;
    private List<AnimalDice> _dice;
    private List<DiceFace> _diceFaces = new();
    
    public DiceThrowHandler(List<AnimalDice> dice)
    {
        _dice = dice;
        foreach (var die in _dice)
        {
            die.OnDiceStoppedRolling += HandleDiceStoppedRolling;
        }
    }

    private void HandleDiceStoppedRolling(DiceFace obj)
    {
        _diceFaces.Add(obj);
    }
    
    public void ThrowAll()
    {
        _diceFaces.Clear();
        foreach (var die in _dice)
        {
            die.Throw();
        }
    }
    
    public List<DiceFace> GetDiceFaces()
    {
        return _diceFaces;
    }
}