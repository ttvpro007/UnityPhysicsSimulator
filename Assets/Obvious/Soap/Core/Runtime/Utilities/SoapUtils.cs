using UnityEngine;

namespace Obvious.Soap
{
    public static class SoapUtils 
    {
        /// <summary>
        /// Creates a new instance of a ScriptableVariableBase subclass with the given name.
        /// </summary>
        /// <typeparam name="T">The type of the ScriptableVariableBase subclass to create.</typeparam>
        /// <param name="name">The name of the new ScriptableVariableBase instance.</param>
        /// <returns>The newly created ScriptableVariableBase instance.</returns>
        public static T CreateRuntimeInstance<T>(string name = "") where T : ScriptableVariableBase
        {
            var runtimeVariable = ScriptableObject.CreateInstance<T>();
            runtimeVariable.name = name;
            runtimeVariable.ResetType = ResetType.None;
            return runtimeVariable;
        }
    }
}