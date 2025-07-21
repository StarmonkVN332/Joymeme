using UnityEngine;

[CreateAssetMenu(menuName = "Spell Effects/Timed Stat Modifier")]
public class StatModifierEffect : SpellEffect
{
    public int attackDelta;
    public int healthDelta;
    public int durationTurns;

    public override void ApplyEffect(UnitInstance target)
    {
        if (target == null) return;

        target.GetComponent<UnitEffectHandler>()?.ApplyTimedStatEffect(attackDelta, healthDelta, durationTurns);
    }

    public override object[] GetEffectData()
    {
        return new object[] { attackDelta, healthDelta, durationTurns };
    }

    public override void ApplyEffectFromData(UnitInstance target, object[] data)
    {
        int atk = (int)data[0];
        int hp = (int)data[1];
        int duration = (int)data[2];

        target.GetComponent<UnitEffectHandler>()?.ApplyTimedStatEffect(atk, hp, duration);
    }
}
