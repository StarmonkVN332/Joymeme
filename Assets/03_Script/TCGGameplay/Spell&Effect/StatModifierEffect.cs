using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spell Effects/Stat Modifier")]
public class StatModifierEffect : SpellEffect
{
    public int attackValue;
    public StatModificationMode attackMode;

    public int healthValue;
    public StatModificationMode healthMode;

    public int durationTurns = 0;

    public bool targetEnemyInstead = false; // If true, target enemy unit instead of self
    public bool requireConditions = false;
    public List<StatCondition> conditions;
    public SpellTargetMode targetMode = SpellTargetMode.SingleTarget;

    public override void ApplyEffect(UnitInstance target)
    {
        Debug.Log($"[ApplySpellEffect] Applying effect to unit {target.name}");

        if (target == null) return;

        if (requireConditions && conditions.Exists(c => !c.IsMet(target)))
        {
            Debug.Log("[StatModifierEffect] Conditions not met. Effect not applied.");
            return;
        }

        var handler = target.GetComponent<UnitEffectHandler>();
        if (handler == null)
        {
            Debug.LogWarning("[StatModifierEffect] No UnitEffectHandler found!");
            return;
        }

        int atkChange = 0;
        int hpChange = 0;

        if (attackMode == StatModificationMode.Add)
            atkChange = attackValue;
        else if (attackMode == StatModificationMode.Set)
            atkChange = attackValue - target.currentAttack;

        if (healthMode == StatModificationMode.Add)
            hpChange = healthValue;
        else if (healthMode == StatModificationMode.Set)
            hpChange = healthValue - target.currentHealth;

        handler.ApplyTimedStatEffect(atkChange, hpChange, durationTurns);
    }

    public object[] GetEffectData()
    {
        return new object[]
        {
            attackValue, (int)attackMode,
            healthValue, (int)healthMode,
            durationTurns,
            requireConditions
        };
    }
}
public enum StatType { Attack, Health }
public enum ComparisonType { GreaterThan, LessThan, EqualTo, GreaterOrEqual, LessOrEqual }
public enum StatModificationMode
{
    Add,    // currentAttack += X
    Set     // currentAttack = X
}
[System.Serializable]
public class StatCondition
{
    public StatType stat;
    public ComparisonType comparison;
    public int value;

    public bool IsMet(UnitInstance unit)
    {
        int statValue = stat == StatType.Attack ? unit.currentAttack : unit.currentHealth;

        return comparison switch
        {
            ComparisonType.GreaterThan => statValue > value,
            ComparisonType.LessThan => statValue < value,
            ComparisonType.EqualTo => statValue == value,
            ComparisonType.GreaterOrEqual => statValue >= value,
            ComparisonType.LessOrEqual => statValue <= value,
            _ => false
        };
    }
}