﻿using TMPro;
using UnityEngine;

namespace Obvious.Soap.Example
{
    public class SaveReader : MonoBehaviour
    {
        [SerializeField] private ScriptableSaveExample _scriptableSaveExample;
        [SerializeField] private TextMeshProUGUI _nameText;
        
        private void Awake()
        {
            _scriptableSaveExample.OnLoaded += RefreshText;
            _scriptableSaveExample.OnSaved += RefreshText;
            _scriptableSaveExample.OnDeleted += RefreshText;

            if (_scriptableSaveExample.LoadMode == ScriptableSaveBase.ELoadMode.Automatic) 
                RefreshText();
        }

        private void OnDestroy()
        {
            _scriptableSaveExample.OnLoaded -= RefreshText;
            _scriptableSaveExample.OnSaved -= RefreshText;
            _scriptableSaveExample.OnDeleted -= RefreshText;
        }

        private void RefreshText()
        {
            _nameText.text = _scriptableSaveExample.LastJsonString;
        }
    }
}