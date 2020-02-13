using UnityEngine;
using UnityEngine.UI;

public class WeaponTextUpdator : MonoBehaviour
{
    [SerializeField] private Text weaponText = null;

    public void UpdateText(string weaponName)
    {
        weaponText.text = "Weapon: " + weaponName;
    }
}
