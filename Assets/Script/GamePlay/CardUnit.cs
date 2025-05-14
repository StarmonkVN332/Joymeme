using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardUnit : MonoBehaviour, IUnit
{
    public void Attack()
    {
        Debug.Log(name + " đang tấn công!");
    }
}
