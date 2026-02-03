using UnityEngine;

public class DestroyOnLoad : MonoBehaviour
{
    void Awake()
    {
        // Destroy this GameObject
        Destroy(gameObject);
    }
}
