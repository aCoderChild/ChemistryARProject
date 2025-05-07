using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLibrary : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectLibrary = new List<GameObject>();  // Ensure this is populated in the Inspector.

    private Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    #region Singleton activation
    public static ObjectLibrary instance;

    public static ObjectLibrary Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("ObjectLibrary instance is not initialized.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Optionally keep this object persistent across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // Property to access 'objects' dictionary
    public Dictionary<string, GameObject> Objects
    {
        get { return objects; }
    }

    public void GenerateLibrary()
    {
        // Check if objectLibrary is assigned or empty
        if (objectLibrary == null || objectLibrary.Count == 0)
        {
            Debug.LogError("ObjectLibrary: objectLibrary is not assigned or empty. Please assign objects in the inspector.");
            return;
        }

        foreach (var gameObject in objectLibrary)
        {
            // Use the name as the key (no need to remove suffixes anymore)
            string key = gameObject.name;

            // Ensure that the key is not duplicated in the dictionary
            if (objects.ContainsKey(key))
            {
                Debug.LogWarning($"Duplicate GameObject name found: {gameObject.name}. Overwriting the existing entry.");
            }

            objects[key] = gameObject;  // Add or overwrite the object by name
        }
    }
}