using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obvious.Soap.Example
{
    // This class is a simple example of a Save Data class.
    // This should be in its own file, but for readability, it is included here.
    [Serializable]
    public class SaveData
    {
        public int Version = 1;
        public int Level = 0;
        public List<Item> Items = new List<Item>();
    }

    [Serializable]
    public class Item
    {
        public string Id;
        public string Name;

        public Item(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }
    }

    [CreateAssetMenu(fileName = "scriptableSaveExample.asset", menuName = "Soap/Examples/ScriptableSaves/ScriptableSaveExample")]
    public class ScriptableSaveExample : ScriptableSave<SaveData>
    {
        //Useful getters
        public int Version => _saveData.Version;
        public int Level => _saveData.Level;
        public IReadOnlyList<Item> Items => _saveData.Items.AsReadOnly();

        #region Useful Methods

        public void AddRandomItem() => AddItem(new Item("RandomItem_" + Items.Count));

        public void AddItem(Item item)
        {
            _saveData.Items.Add(item);
            Save();
        }

        public Item GetItemById(string id)
        {
            return _saveData.Items.Find(item => item.Id == id);
        }

        public void ClearItems()
        {
            _saveData.Items.Clear();
            Save();
        }
        
        public void IncrementLevel(int value)
        {
            _saveData.Level += value;
            Save();
        }
        
        public void SetLevel(int value)
        {
            _saveData.Level = value;
            Save();
        }

        #endregion
        
        protected override void UpgradeData(SaveData oldData)
        {
            if (_debugLogEnabled)
                Debug.Log("Upgrading data from version " + oldData.Version + " to " + _saveData.Version);
            // Implement additional upgrade logic here
            oldData.Version = _saveData.Version;
        }

        protected override bool NeedsUpgrade(SaveData saveData)
        {
            return saveData.Version < _saveData.Version;
        }
    }
}