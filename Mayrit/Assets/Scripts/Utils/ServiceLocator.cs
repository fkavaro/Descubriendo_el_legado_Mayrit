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
            _instance ??= new ServiceLocator();
            return _instance;
        }
    }

    // Event
    public event Action<object> OnDuplicatedServiceEvent;

    /// <summary>
    /// Register a service instance
    /// </summary>
    public void Register<T>(T service) where T : MonoBehaviour
    {
        if (service == null)
        {
            Debug.LogError($"Cannot register service {typeof(T).Name}: service reference is null.");
            return;
        }

        var type = typeof(T);

        if (_services.ContainsKey(type))
        {
            if (service.gameObject.activeInHierarchy)
                Debug.Log($"Service {type.Name} is already registered. Destroying instance from scene: {service.gameObject}.");

            // Service already exists - destroy the new one trying to register
            UnityEngine.Object.Destroy(service.gameObject);
            OnDuplicatedServiceEvent?.Invoke(Get<T>());
        }
        else
        {
            // Register new service
            _services.Add(type, service);

            if (!service.gameObject.activeInHierarchy)
                service.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Get a registered service
    /// </summary>
    public T Get<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
            return service as T;
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
    /// Check if a service is registered and is not the same as the requester
    /// </summary>
    public bool HasOther<T>(object requester) where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
            return !ReferenceEquals(service, requester);
        return false;
    }

    /// <summary>
    /// Unregister a service
    /// </summary>
    public void Unregister<T>(T service) where T : class
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

