using System;
using System.Collections.Generic;
using UnityEngine;

/// Lives in MapScene. Holds the view prefabs and instantiates each one lazily (once).
public class ViewManager : MonoBehaviour
{
    [SerializeField] private ViewBase[] viewPrefabs;

    private readonly Dictionary<Type, ViewBase> instances = new Dictionary<Type, ViewBase>();

    public T Get<T>() where T : ViewBase
    {
        if (instances.TryGetValue(typeof(T), out ViewBase existing))
            return existing as T;

        foreach (ViewBase prefab in viewPrefabs)
        {
            if (prefab is not T) continue;

            ViewBase instance = Instantiate(prefab, transform);
            instances[typeof(T)] = instance;
            return instance as T;
        }

        Debug.LogError($"ViewManager: no prefab of type {typeof(T).Name} in the list.", this);
        return null;
    }
}