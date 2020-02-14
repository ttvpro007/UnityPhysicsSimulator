using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class DamageIndicator : MonoBehaviour
    {
        [SerializeField] private Image damageIndicatorHUD = null;
        [Range(0f, 1f)]
        [SerializeField] private float alphaModValue = 0;
        private Color initialColor = Color.white;
        private Color modifiedColor = Color.white;

        private void Start()
        {
            damageIndicatorHUD.color = TurnOffAlpha(damageIndicatorHUD.color);
            initialColor = damageIndicatorHUD.color;
            modifiedColor = initialColor;
        }

        private void Update()
        {
            if (damageIndicatorHUD.color.a > 0)
            {
                float alphaValue = Mathf.Lerp(damageIndicatorHUD.color.a, 0, Time.deltaTime);
                damageIndicatorHUD.color = SetAlpha(damageIndicatorHUD.color, alphaValue);
            }
        }

        private Color TurnOffAlpha(Color color)
        {
            color.a = 0;
            return color;
        }

        private Color SetAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public void Trigger()
        {
            damageIndicatorHUD.color = SetAlpha(damageIndicatorHUD.color, alphaModValue);
        }
    }
}