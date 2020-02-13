using UnityEngine;
using HUD;

namespace Core
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private HealthBarUI healthBar = null;
        [SerializeField] private float maxHealth = 0;
        private int totalHPCell = 0;
        private float currentHealth = 0;
        private float healthPerCell = 0;
        private int currentCellNumber = 0;
        private int previousCellNumber = 0;

        private void Start()
        {
            totalHPCell = healthBar.TotalCells;
            totalHPCell = Mathf.Max(1, totalHPCell);

            healthPerCell = maxHealth / totalHPCell;
            currentHealth = maxHealth;
            currentCellNumber = totalHPCell;
            previousCellNumber = currentCellNumber;
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);
            currentCellNumber = Mathf.CeilToInt(currentHealth / healthPerCell);

            if (currentHealth == 0) Destroy();

            if (currentCellNumber != previousCellNumber)
                healthBar.ToggleOnOff(totalHPCell - currentCellNumber, 0, false);
        }

        private void Destroy()
        {
            gameObject.SetActive(false);
            Destroy(gameObject, 2);
        }
    }
}