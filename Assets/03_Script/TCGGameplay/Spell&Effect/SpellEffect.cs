using System.Collections.Generic;
using UnityEngine;

public abstract class SpellEffect : ScriptableObject
{
    public enum SpellTargetMode
    {
        SingleTarget,
        GlobalAllies,
        GlobalEnemies,
        GlobalAll
    }
    public void ApplySpellEffect(UnitInstance target, StatModifierEffect effect)
    {
        List<UnitInstance> targets = new();
        Debug.Log($"[ApplySpellEffect] TargetMode: {effect.targetMode}, TargetOwnerId: {target.OwnerActorId}");

        switch (effect.targetMode)  
        {
            case SpellTargetMode.SingleTarget:
                targets.Add(target);
                break;

            case SpellTargetMode.GlobalAllies:
                targets.AddRange(BoardManager.Instance.GetAllUnitsForPlayer(target.OwnerActorId));
                Debug.Log($"[ApplySpellEffect] Found {targets.Count} target(s) for effect {effect.name}");
                break;

            case SpellTargetMode.GlobalEnemies:
                targets.AddRange(BoardManager.Instance.GetAllUnitsForOpponent(target.OwnerActorId));
                break;

            case SpellTargetMode.GlobalAll:
                targets.AddRange(BoardManager.Instance.GetAllUnits());
                break;
        }

        foreach (var unit in targets)
        {
            UnitEffectHandler handler = unit.GetComponent<UnitEffectHandler>();
            if (handler != null)
            {
                handler.ApplyEffect(effect);
            }
        }
    }

    public abstract void ApplyEffect(UnitInstance target);

    // Optional: serialize effect for RPC use
    public virtual object[] GetEffectData() => null;
    public virtual void ApplyEffectFromData(UnitInstance target, object[] data) => ApplyEffect(target);
}
