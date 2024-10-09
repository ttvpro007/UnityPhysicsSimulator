using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public UIDisplayer UIDisplayer;
    public Transform WeaponHolder;
    public List<Projectile> Projectiles;
    public Dictionary<Projectile, GameObject> RuntimeWeaponGameObject = new();

    // Start is called before the first frame update
    void Start()
    {
        // Iterate over each projectile in the list
        foreach (var projectile in Projectiles)
        {
            // Store the runtime game objects in the dictionary for easy access
            var runtimeGameplayGameObject = Instantiate(projectile.GameplayGameObject, WeaponHolder);
            runtimeGameplayGameObject.SetActive(false);
            RuntimeWeaponGameObject[projectile] = runtimeGameplayGameObject;
        }

        if (!Projectiles.IsNullOrEmpty())
        {
            UpdateDisplayForProjectile(Projectiles[0]);
        }
    }

    public void UpdateDisplayForProjectile(Projectile projectile)
    {
        if (ReflectionHelper.CastProjectile<Arrow>(projectile, out var arrow))
        {
            SetDisplay(arrow);
        }
        else if (ReflectionHelper.CastProjectile<Grenade>(projectile, out var grenade))
        {
            SetDisplay(grenade);
        }
        else if (ReflectionHelper.CastProjectile<Molotov>(projectile, out var molotov))
        {
            SetDisplay(molotov);
        }
    }

    public void SetDisplay<T>(T projectile) where T : Projectile
    {
        TurnOffAllGameplayGameObject();
        RuntimeWeaponGameObject[projectile].SetActive(true);
    }

    private void TurnOffAllGameplayGameObject()
    {
        foreach (var go in RuntimeWeaponGameObject.Values)
        {
            go.SetActive(false);
        }
    }
}
