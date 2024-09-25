using TMPro;
using UnityEngine;

namespace Obvious.Soap.Example
{
    [RequireComponent(typeof(TMP_Text))]
    public class ListCount : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private ScriptableListPlayer scriptableListPlayer = null;

        void Awake()
        {
            scriptableListPlayer.OnItemCountChanged += UpdateText;
        }

        private void OnDestroy()
        {
            scriptableListPlayer.OnItemCountChanged -= UpdateText;
        }

        private void UpdateText()
        {
            _text.text = $"Count : {scriptableListPlayer.Count}";
        }
    }
}