using UnityEngine;

public abstract class SpellEffect : ScriptableObject
{
    public abstract void ApplyEffect(UnitInstance target);

    // Optional: serialize effect for RPC use
    public virtual object[] GetEffectData() => null;
    public virtual void ApplyEffectFromData(UnitInstance target, object[] data) => ApplyEffect(target);
}
