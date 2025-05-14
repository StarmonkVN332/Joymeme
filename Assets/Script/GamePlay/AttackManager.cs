using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackManager : MonoBehaviour
{
    public List<Transform> columns; // Gán 5 column rỗng trong Inspector
    public Button attackButton;

    private void Start()
    {
        attackButton.onClick.AddListener(StartAttackPhase);
    }

    void StartAttackPhase()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        for (int i = 0; i < columns.Count; i++)
        {
            Transform column = columns[i];
            Debug.Log($"[Column {i}] Đang kiểm tra...");

            if (column.childCount == 0)
            {
                Debug.Log($"[Column {i}] Không có đơn vị nào.");
            }
            else
            {
                Debug.Log($"[Column {i}] Có {column.childCount} đơn vị.");
                foreach (Transform unit in column)
                {
                    IUnit iUnit = unit.GetComponent<IUnit>();
                    if (iUnit != null)
                    {
                        Debug.Log($"[Column {i}] {unit.name} đang tấn công.");
                        iUnit.Attack();
                        yield return new WaitForSeconds(0.5f); // Delay giữa các đơn vị trong cùng column
                    }
                    else
                    {
                        Debug.LogWarning($"[Column {i}] {unit.name} không có script IUnit.");
                    }
                }
            }

            yield return new WaitForSeconds(2f); // Delay giữa các column
        }

        Debug.Log("Tấn công kết thúc.");
    }
}
