using UnityEngine;

namespace Weapons
{
    public enum WeaponType
    {
        Pistol,
        GravityGun,
        GrappleGun
    }

    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] private WeaponType type = WeaponType.Pistol;
        [SerializeField] private NormalGun pistol = null;
        [SerializeField] private GravityGun gravityGun = null;
        [SerializeField] private GrappleGun grappleGun = null;
        [SerializeField] private WeaponTextUpdator weaponText = null;
        public WeaponType Type { get { return type; } }

        private void Start()
        {
            if (!pistol) pistol = GetComponentInChildren<NormalGun>();
            if (!gravityGun) gravityGun = GetComponentInChildren<GravityGun>();
            if (!grappleGun) grappleGun = GetComponentInChildren<GrappleGun>();
            weaponText.UpdateText(type.ToString());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                type = WeaponType.Pistol;
                ChangeWeapon(type);
                weaponText.UpdateText(type.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                type = WeaponType.GravityGun;
                ChangeWeapon(type);
                weaponText.UpdateText(type.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                type = WeaponType.GrappleGun;
                ChangeWeapon(type);
                weaponText.UpdateText(type.ToString());
            }
        }

        private void ChangeWeapon(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Pistol:
                    pistol.enabled = true;
                    gravityGun.enabled = false;
                    grappleGun.enabled = false;
                    break;
                case WeaponType.GravityGun:
                    pistol.enabled = false;
                    gravityGun.enabled = true;
                    grappleGun.enabled = false;
                    break;
                case WeaponType.GrappleGun:
                    pistol.enabled = false;
                    gravityGun.enabled = false;
                    grappleGun.enabled = true;
                    break;
                default:
                    break;
            }
        }
    }
}