using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEffectHandler : MonoBehaviour
{
    private UnitInstance unit;
    private List<ActiveEffect> activeEffects = new();

    public class ActiveEffect
    {
        public int atkChange;
        public int hpChange;
        public int turnsRemaining;

        public ActiveEffect(int atk, int hp, int duration)
        {
            atkChange = atk;
            hpChange = hp;
            turnsRemaining = duration;
        }
    }

    private void Awake()
    {
        unit = GetComponent<UnitInstance>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnRoundAdvanced += OnRoundAdvanced;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundAdvanced -= OnRoundAdvanced;
    }

    private void OnRoundAdvanced()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].turnsRemaining--;

            if (activeEffects[i].turnsRemaining <= 0)
            {
                // Revert effect
                unit.ModifyStats(-activeEffects[i].atkChange, -activeEffects[i].hpChange);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void ApplyEffect(SpellEffect effect)
    {
        if (effect is StatModifierEffect timedEffect)
        {
            unit.ModifyStats(timedEffect.attackDelta, timedEffect.healthDelta);
            activeEffects.Add(new ActiveEffect(timedEffect.attackDelta, timedEffect.healthDelta, timedEffect.durationTurns));
        }
        else
        {
            effect.ApplyEffect(unit);
        }
    }

    public void ApplyTimedStatEffect(int atk, int hp, int turns)
    {
        unit.ModifyStats(atk, hp);
        activeEffects.Add(new ActiveEffect(atk, hp, turns));
    }

}

