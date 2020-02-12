using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private HealthBarUI healthBar = null;
    [SerializeField] private float maxHealth = 0;
    [SerializeField] private int totalHPCell = 0;
    private float currentHealth = 0;
    private float cellPercentage = 0;
    private int currentCellNumber = 0;
    private int previousCellNumber = 0;

    private void Start()
    {
        if (totalHPCell == 0) totalHPCell = 1;
        cellPercentage = maxHealth / totalHPCell;
        currentHealth = maxHealth;
        currentCellNumber = totalHPCell;
        previousCellNumber = currentCellNumber;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        currentCellNumber = Mathf.FloorToInt(currentHealth / cellPercentage);

        if (currentHealth == 0) Destroy();

        if (currentCellNumber != previousCellNumber) ToggleHPCell();
    }

    private void ToggleHPCell()
    {
        for (int i = 1; i <= totalHPCell; i++)
        {
            healthBar.SetCell(i, false);
        }

        for (int i = 1; i <= currentCellNumber; i++)
        {
            healthBar.SetCell(i, true);
        }
    }

    private void Destroy()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 2);
    }
}
