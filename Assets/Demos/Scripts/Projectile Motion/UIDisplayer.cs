using Sirenix.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDisplayer : MonoBehaviour
{
    // A list to hold any projectile types (Arrow, Grenade, Molotov, etc.)
    public List<Projectile> Projectiles;
    public Dictionary<Projectile, List<IDisplayable.Displayable>> ProjectileDisplayFields = new();

    public RectTransform ObjectHolderTransform;
    public TMP_Text DescriptionTextField;
    public GameObject StatRow;

    public Dictionary<Projectile, GameObject> RuntimeUIGameObjects = new();

    public List<GameObject> StatRows = new();

    private void Start()
    {
        // Iterate over each projectile in the list
        foreach (var projectile in Projectiles)
        {
            UpdateProjectileDisplayFields(projectile);

            // Store the display fields in the dictionary for easy access
            ProjectileDisplayFields[projectile] = projectile.DisplayFields;

            // Store the runtime game objects in the dictionary for easy access
            var runtimeUIGameObject = Instantiate(projectile.UIGameObject, ObjectHolderTransform);
            runtimeUIGameObject.SetActive(false);
            RuntimeUIGameObjects[projectile] = runtimeUIGameObject;
        }

        if (!Projectiles.IsNullOrEmpty())
        {
            UpdateDisplayForProjectile(Projectiles[0]);
        }
    }

    private void UpdateProjectileDisplayFields(Projectile projectile)
    {
        if (ReflectionHelper.CastProjectile<Arrow>(projectile, out var arrow))
        {
            projectile.UpdateDisplayFieldsInfo(arrow);
        }
        else if (ReflectionHelper.CastProjectile<Grenade>(projectile, out var grenade))
        {
            projectile.UpdateDisplayFieldsInfo(grenade);
        }
        else if (ReflectionHelper.CastProjectile<Molotov>(projectile, out var molotov))
        {
            projectile.UpdateDisplayFieldsInfo(molotov);
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
        TurnOffAllUIGameObject();
        RemoveAllStatRows();

        RuntimeUIGameObjects[projectile].SetActive(true);
        DescriptionTextField.text = projectile.Description;

        foreach (var displayableField in ProjectileDisplayFields[projectile])
        {
            var statRow = Instantiate(StatRow, transform).GetComponent<StatRow>();
            statRow.IconDisplayer.sprite = displayableField.Icon;
            statRow.LabelField.text = displayableField.Field;
            statRow.ValueField.text = displayableField.Value;
            StatRows.Add(statRow.gameObject);
        }
    }

    private void TurnOffAllUIGameObject()
    {
        foreach (var go in RuntimeUIGameObjects.Values)
        {
            go.SetActive(false);
        }
    }

    private void RemoveAllStatRows()
    {
        for (int i = 0; i < StatRows.Count; i++)
        {
            Destroy(StatRows[i]);
        }

        StatRows.Clear();
    }
}
