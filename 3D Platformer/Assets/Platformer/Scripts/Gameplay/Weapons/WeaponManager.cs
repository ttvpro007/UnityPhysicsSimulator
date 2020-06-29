using System;
using System.Collections.Generic;
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
        private static int currentGunIndex = 0;
        private static Dictionary<int, WeaponType> gunDictionary = new Dictionary<int, WeaponType>();

        private void Start()
        {
            gunDictionary.Add(0, WeaponType.Pistol);
            gunDictionary.Add(1, WeaponType.GravityGun);
            gunDictionary.Add(2, WeaponType.GrappleGun);

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

            //if (Input.GetKeyDown(KeyCode.Alpha1))
            //{
            //    LoopLeft();

            //    UpdateGun();
            //}
            //else if (Input.GetKeyDown(KeyCode.Alpha2))
            //{
            //    LoopRight();

            //    UpdateGun();
            //}
        }

        private static void LoopLeft()
        {
            if (currentGunIndex > 0)
                currentGunIndex--;
            else
                currentGunIndex = gunDictionary.Count - 1;
        }

        private static void LoopRight()
        {
            if (currentGunIndex < gunDictionary.Count - 1)
                currentGunIndex++;
            else
                currentGunIndex = 0;
        }

        private void UpdateGun()
        {
            type = gunDictionary[currentGunIndex];
            ChangeWeapon(type);
            weaponText.UpdateText(type.ToString());
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