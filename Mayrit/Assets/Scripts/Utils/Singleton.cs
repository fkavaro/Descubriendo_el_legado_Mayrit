using UnityEngine;

/// <summary>
/// Generic Singleton class for MonoBehaviours.
/// Ensures only one instance exists and destroys duplicates.
/// </summary>
/// <typeparam name="T">The type that extends Singleton.</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Wether to destroy the instance when loading a new scene.
    [SerializeField] protected bool dontDestroyOnLoad = true;

    // Static reference to the single instance of this class.
    private static T _instance;

    // Lock object to ensure thread safety when creating the instance.
    private static readonly object _lock = new object();

    /// <summary>
    /// Public property to access the singleton instance.
    /// Ensures that only one instance exists.
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (_lock) // Ensures that only one thread at a time can execute this block.
            {
                if (_instance == null) // If no instance exists, try to find one.
                {
                    _instance = FindAnyObjectByType<T>(); // Searches for an existing instance in the scene.

                    if (_instance == null) // If still null, create a new instance.
                    {
                        GameObject singletonObj = new GameObject(typeof(T).Name); // Creates a new GameObject named after the type.
                        _instance = singletonObj.AddComponent<T>(); // Adds the singleton component to the new GameObject.
                    }
                }
                return _instance; // Returns the instance.
            }
        }
    }

    /// <summary>
    /// Ensures only one instance exists in the scene.
    /// Destroys any duplicate instances.
    /// </summary>
    protected virtual void Awake()
    {
        lock (_lock) // Ensures thread safety when setting the instance.
        {
            if (_instance == null) // If no instance exists, set this as the instance.
            {
                _instance = this as T;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject); // Prevents the instance from being destroyed when loading new scenes.
            }
            else if (_instance != this) // If an instance already exists and it's not this one, destroy this object.
            {
                Debug.LogWarning($"Duplicate Singleton<{typeof(T).Name}> found. Destroying...");
                DestroyImmediate(gameObject);
            }
        }
    }
}
