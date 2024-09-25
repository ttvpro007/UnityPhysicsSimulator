using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Obvious.Soap
{
    /// <summary>
    /// Base classes of all ScriptableObjects in Soap
    /// </summary>
    public abstract class ScriptableBase : ScriptableObject
    {
        internal virtual void Reset()
        {
            TagIndex = 0;
            Description = "";
        }
        [HideInInspector]
        public Action RepaintRequest;
        [FormerlySerializedAs("CategoryIndex")] [HideInInspector]
        public int TagIndex = 0;
        [HideInInspector]
        public string Description = "";
        public virtual Type GetGenericType { get; }
    }
}