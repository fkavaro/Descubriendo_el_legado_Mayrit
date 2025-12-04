using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Service Locator pattern for Dependency Injection in Unity.
/// This provides a centralized registry for services without using Singletons.
/// </summary>
public class ServiceLocator
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new();

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ServiceLocator();
            return _instance;
        }
    }

    /// <summary>
    /// Register a service instance
    /// </summary>
    public void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"Service {type.Name} is already registered. Overwriting...");
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
        }
    }

    /// <summary>
    /// Get a registered service
    /// </summary>
    public T Get<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
        {
            return service as T;
        }

        Debug.LogError($"Service {type.Name} not found!");
        return null;
    }

    /// <summary>
    /// Check if a service is registered
    /// </summary>
    public bool Has<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Unregister a service
    /// </summary>
    public void Unregister<T>() where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
        }
    }

    /// <summary>
    /// Clear all services (useful for cleanup or scene transitions)
    /// </summary>
    public void Clear()
    {
        _services.Clear();
    }

    /// <summary>
    /// Reset the instance (mainly for testing)
    /// </summary>
    public static void Reset()
    {
        _instance = null;
    }
}

