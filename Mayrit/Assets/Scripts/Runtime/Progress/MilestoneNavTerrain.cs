using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class MilestoneNavTerrain : MilestoneTracker
{
    #region EDITOR PROPERTIES
    [Header("Agents avoidance time")]
    public float _agentsAvoidancePredictionTime = 0.5f;
    public int _pathFindingIterationsPerFrame = 1000;
    [Header("References")]
    public List<Terrain> _terrains = new();

    [Header("Trees Obstacle Configuration")]
    public Vector3 _treesObstacleSize = new(2.0f, 4.0f, 2.0f);
    #endregion

    #region INTERNAL PROPERTIES
    private NavMeshSurface _navMeshSurface;
    private readonly List<GameObject> _createdObjects = new();
    #endregion

    void Awake()
    {
        NavMesh.avoidancePredictionTime = _agentsAvoidancePredictionTime;
        NavMesh.pathfindingIterationsPerFrame = _pathFindingIterationsPerFrame;
    }

    #region INHERITED METHODS
    protected override void SetChildrenActive(bool isActive)
    {
        if (this == null) return;

        _isActive = isActive;

        if (!ValidateReferences()) return;

        _navMeshSurface.enabled = isActive;

        foreach (Transform child in transform)
        {
            if (child == null || child.gameObject == null) continue;
            if (child.gameObject.activeSelf != isActive)
                child.gameObject.SetActive(isActive);
        }
    }

    //     protected override void OnEditorUpdateChanged(bool updateInEditor)
    //     {
    // #if UNITY_EDITOR
    //         if (Application.isPlaying || this == null) return;

    //         if (!updateInEditor)
    //         {
    //             // Only active if last milestone
    //             SetChildrenActive((int)milestonesActivated.x == 7 && (int)milestonesActivated.y == 7);
    //             return;
    //         }

    //         var progressManager = FindAnyObjectByType<ProgressManager>();
    //         if (progressManager == null) return;

    //         int milestone = progressManager.CurrentMilestoneIndex;
    //         int min = Mathf.Min((int)milestonesActivated.x, (int)milestonesActivated.y);
    //         int max = Mathf.Max((int)milestonesActivated.x, (int)milestonesActivated.y);
    //         SetChildrenActive(milestone >= min && milestone <= max);
    // #endif
    //     }
    #endregion

    #region PUBLIC METHODS
    [ContextMenu("Extract Tree Modifiers")]
    public int ExtractTreeModifiers()
    {
        if (!ValidateActivation() || !ValidateReferences()) return 0;

        ClearPreviousModifiers();

        foreach (Terrain terrain in _terrains)
        {
            if (terrain != null && terrain.gameObject.activeInHierarchy)
            {
                ProcessTerrain(terrain);
            }
        }

        Debug.Log($"<color=#99ff99>Generated {_createdObjects.Count} NavMesh modifiers based on the terrains' trees.</color>");
        return _createdObjects.Count;
    }

    [ContextMenu("Delete Tree Modifiers")]
    public void DestroyCachedObjects()
    {
        foreach (var obj in _createdObjects)
        {
            if (obj != null) DestroyImmediate(obj);
        }

        _createdObjects.Clear();
        Debug.Log("<color=#ff9999>Deleted all cached modifier objects.</color>");
    }
    #endregion

    #region PRIVATE METHODS
    private bool ValidateActivation()
    {
        if (_isActive) return true;

        Debug.LogWarning($"{gameObject.name} is not active. Operation cancelled.");
        return false;
    }

    private bool ValidateReferences()
    {
        // Ensure NavMeshSurface component exists
        if (_navMeshSurface == null)
        {
            _navMeshSurface = GetComponent<NavMeshSurface>();

            if (_navMeshSurface == null)
            {
                Debug.LogError($"{gameObject.name}: NavMeshSurface component could not be found or created.");
                return false;
            }
        }

        // Find terrains if not assigned
        if (_terrains == null || _terrains.Count == 0)
        {
            _terrains = new List<Terrain>(GetComponentsInChildren<Terrain>());

            if (_terrains.Count == 0)
            {
                Debug.LogError($"{gameObject.name}: No terrains found. Please assign Terrains or ensure they exist as children.");
                return false;
            }
        }

        return true;
    }

    private void ProcessTerrain(Terrain terrain)
    {
        TerrainData data = terrain.terrainData;
        if (data == null)
        {
            Debug.LogWarning($"Terrain {terrain.name} has no TerrainData.");
            return;
        }

        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = data.size;
        TreePrototype[] prototypes = data.treePrototypes;

        foreach (TreeInstance tree in data.treeInstances)
        {
            Vector3 worldPos = Vector3.Scale(tree.position, terrainSize) + terrainPosition;

            if (IsPointInNavMeshSurfaceBounds(worldPos))
            {
                CreateModifier(prototypes[tree.prototypeIndex].prefab.name, worldPos);
            }
        }
    }

    private void CreateModifier(string treeName, Vector3 worldPos)
    {
        GameObject volumeObj = new GameObject(treeName + "_ModifierVolume");
        volumeObj.transform.position = worldPos;
        volumeObj.transform.parent = _navMeshSurface.transform;

        // Add NavMeshModifierVolume for carving
        NavMeshModifierVolume modifierVolume = volumeObj.AddComponent<NavMeshModifierVolume>();
        modifierVolume.size = _treesObstacleSize;
        modifierVolume.area = NavMesh.GetAreaFromName("Not Walkable");

        _createdObjects.Add(volumeObj);
    }

    private bool IsPointInNavMeshSurfaceBounds(Vector3 worldPos)
    {
        if (_navMeshSurface == null) return false;

        // Transform the world position into the NavMeshSurface's local space
        Transform t = _navMeshSurface.transform;
        Vector3 localPos = t.InverseTransformPoint(worldPos) - _navMeshSurface.center;
        Vector3 halfSize = _navMeshSurface.size * 0.5f;

        return Mathf.Abs(localPos.x) <= halfSize.x &&
               Mathf.Abs(localPos.y) <= halfSize.y &&
               Mathf.Abs(localPos.z) <= halfSize.z;
    }

    private void ClearPreviousModifiers()
    {
        foreach (var obj in _createdObjects)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        _createdObjects.Clear();
    }
    #endregion
}