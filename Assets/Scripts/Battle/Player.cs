using System.Collections.Generic;

public class Player : IBattlable
{
    public E_Camp Camp => E_Camp.玩家方;
    public bool IsAlive => true;

    public Player() { }

    public BattleDamageHandler battleDamageHandler { get; set; }

    List<E_WeaknessType> _weaknesses = new List<E_WeaknessType>();
    public List<E_WeaknessType> weaknesses => _weaknesses;

    public System.Action OnWeaknessChanged { get; set; }

    public Player(BattleDamageHandler _damageHandle)
    {
        battleDamageHandler = _damageHandle;
    }

    public bool GetWeakAttack(E_WeaknessType attackWeakType)
    {
        for (int i = 0; i < _weaknesses.Count; i++)
            if (_weaknesses[i] == attackWeakType)
                return true;
        return false;
    }

    public void AddWeakness(E_WeaknessType w)
    {
        if (!_weaknesses.Contains(w))
        {
            _weaknesses.Add(w);
            OnWeaknessChanged?.Invoke();
        }
    }

    public void RemoveWeakness(E_WeaknessType w)
    {
        if (_weaknesses.Remove(w))
            OnWeaknessChanged?.Invoke();
    }
}
