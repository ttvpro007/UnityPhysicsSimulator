using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Obvious.Soap.Editor
{
    public class SoapSettings : ScriptableObject
    {
        public EVariableDisplayMode VariableDisplayMode = EVariableDisplayMode.Default;
        public ENamingCreationMode NamingOnCreationMode = ENamingCreationMode.Auto;
        public ECreatePathMode CreatePathMode = ECreatePathMode.Auto;
        [FormerlySerializedAs("Categories")] public List<string> Tags = new List<string> { "None" };
    }

    public enum EVariableDisplayMode
    {
        Default,
        Minimal
    }

    public enum ENamingCreationMode
    {
        Auto,
        Manual
    }


    public enum ECreatePathMode
    {
        Auto,
        Manual
    }
}