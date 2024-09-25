﻿using System.Collections.Generic;
using UnityEngine;

namespace Obvious.Soap
{
    /// <summary>
    /// Interface to get objects that can be drawn in the inspector
    /// </summary>
    public interface IDrawObjectsInInspector
    {
        List<Object> GetAllObjects();
    }
}