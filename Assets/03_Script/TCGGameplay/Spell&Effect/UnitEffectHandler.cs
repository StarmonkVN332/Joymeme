using Photon.Pun;
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
        // Only MasterClient controls expiration logic
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].turnsRemaining--;

            if (activeEffects[i].turnsRemaining <= 0)
            {
                // Revert stats locally
                unit.ModifyStats(-activeEffects[i].atkChange, -activeEffects[i].hpChange);

                // Broadcast to others
                PhotonView pv = unit.photonView;
                pv.RPC("RPC_RevertStats", RpcTarget.Others, pv.ViewID, -activeEffects[i].atkChange, -activeEffects[i].hpChange);

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
    [PunRPC]
    public void RPC_RevertStats(int viewID, int atk, int hp)
    {
        if (PhotonView.Find(viewID)?.GetComponent<UnitInstance>() is UnitInstance target)
        {
            target.ModifyStats(atk, hp); // Will update visuals too
        }
    }

    public void ApplyTimedStatEffect(int atk, int hp, int turns)
    {
        unit.ModifyStats(atk, hp);
        activeEffects.Add(new ActiveEffect(atk, hp, turns));
    }

}

