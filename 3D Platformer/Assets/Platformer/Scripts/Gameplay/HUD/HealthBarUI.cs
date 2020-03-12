using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private List<Image> cells = new List<Image>();

        public int TotalCells { get { return cells.Count; } }

        private void Start()
        {
            EnableAllCells();
        }

        private void EnableAllCells()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].enabled = true;
            }
        }

        public void ToggleOnOff(int cellsToToggle, int startIndex, bool isTurnOn)
        {
            // if already toggle all cells, return
            if (cellsToToggle == startIndex) return;

            for (int i = startIndex; i < cellsToToggle; i++)
            {
                // if cell already turned off, call function on next cell
                if (cells[i].enabled == isTurnOn)
                    ToggleOnOff(cellsToToggle, startIndex + 1, isTurnOn);

                cells[i].enabled = isTurnOn;
            }
        }
    }
}