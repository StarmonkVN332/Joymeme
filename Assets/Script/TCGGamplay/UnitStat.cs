using UnityEngine;

public class UnitStats : MonoBehaviour
{
    public int attack;
    public int health;

    public bool IsDead => health <= 0;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining HP: {health}");
    }
}
