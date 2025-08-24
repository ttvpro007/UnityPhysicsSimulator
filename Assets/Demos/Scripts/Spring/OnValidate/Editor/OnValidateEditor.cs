#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
public class OnValidateCallEditor : Editor
{
    private static readonly Dictionary<Type, MethodInfo[]> Cache = new();
    private static readonly HashSet<int> SeenOnAdd = new(); // instanceIDs we've already fired for

    static OnValidateCallEditor()
    {
        // Primary hook (when available)
#if UNITY_2019_3_OR_NEWER
        ObjectFactory.componentWasAdded += comp =>
        {
            if (comp is MonoBehaviour mb && IsSceneObject(mb))
            {
                // Defer one tick so SerializedObject values are fully initialized
                EditorApplication.delayCall += () => InvokeOnAddOnce(mb);
            }
        };
#endif

        // Fallback hook: detect new components via hierarchy changes (covers prefab drops, duplications, etc.)
        EditorApplication.hierarchyChanged += () =>
        {
            // Defer scanning to avoid multiple triggers within the same edit operation
            EditorApplication.delayCall += ScanForNewlyAddedComponents;
        };
    }

    // -------- Inspector change -> invoke --------
    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            DrawDefaultInspector();
            if (check.changed)
            {
                foreach (var obj in targets)
                    if (obj is MonoBehaviour mb) InvokeAttributedMethods(mb);
            }
        }
    }

    // -------- Core invocation helpers --------
    private static void ScanForNewlyAddedComponents()
    {
        // Find all scene MonoBehaviours and run just-added ones that have [OnValidateCall]
        var all = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        foreach (var mb in all)
        {
            if (!IsSceneObject(mb)) continue;
            if (!HasAttributedMethods(mb.GetType())) continue;

            InvokeOnAddOnce(mb);
        }
    }

    private static void InvokeOnAddOnce(MonoBehaviour mb)
    {
        if (!mb) return;
        var id = mb.GetInstanceID();
        if (SeenOnAdd.Contains(id)) return;  // already fired for this instance

        // Mark and invoke
        SeenOnAdd.Add(id);
        InvokeAttributedMethods(mb);
    }

    private static void InvokeAttributedMethods(MonoBehaviour mb)
    {
        if (!mb) return;
        var type = mb.GetType();

        if (!Cache.TryGetValue(type, out var methods))
        {
            methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          .Where(m => m.GetParameters().Length == 0 &&
                                      m.GetCustomAttribute<OnValidateCallAttribute>(true) != null)
                          .ToArray();
            Cache[type] = methods;
        }

        if (methods.Length == 0) return;

        foreach (var m in methods)
        {
            try { m.Invoke(mb, null); }
            catch (Exception ex)
            {
                Debug.LogException(new TargetInvocationException(
                    $"[OnValidateCall] {type.Name}.{m.Name} threw:", ex));
            }
        }
    }

    private static bool HasAttributedMethods(Type type)
    {
        if (Cache.TryGetValue(type, out var cached)) return cached.Length > 0;
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          .Where(m => m.GetParameters().Length == 0 &&
                                      m.GetCustomAttribute<OnValidateCallAttribute>(true) != null)
                          .ToArray();
        Cache[type] = methods;
        return methods.Length > 0;
    }

    private static bool IsSceneObject(UnityEngine.Object obj)
    {
        var go = (obj as Component)?.gameObject;
        // Scene object or prefab stage object; ignore importer previews & assets
        return go == null || go.scene.IsValid();
    }
}
#endif
