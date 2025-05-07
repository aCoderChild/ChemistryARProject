using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTracking : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager = default;

    // A dictionary to hold each tracked image and its corresponding GameObject
    private Dictionary<string, GameObject> activeTrackedImages = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (trackedImageManager)
            return;

        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void Start()
    {
        // Generate the library of objects for image tracking
        ObjectLibrary.Instance.GenerateLibrary();
    }

    private void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Handle added images
        foreach (var added in eventArgs.added)
        {
            AddObject(added);
        }

        // Handle updated images
        foreach (var updated in eventArgs.updated)
        {
            UpdateObject(updated);
        }

        // Handle removed images
        foreach (var removed in eventArgs.removed)
        {
            RemoveObject(removed);
        }
    }

    private void AddObject(ARTrackedImage trackedImage)
    {
        // Check if the object for this image is in the library
        if (ObjectLibrary.Instance.Objects.ContainsKey(trackedImage.referenceImage.name))
        {
            // Instantiate the object if it's not already active
            if (!activeTrackedImages.ContainsKey(trackedImage.referenceImage.name))
            {
                GameObject obj = ObjectLibrary.Instance.Objects[trackedImage.referenceImage.name];
                GameObject instantiatedObject = Instantiate(obj, trackedImage.transform.position, trackedImage.transform.rotation);
                instantiatedObject.SetActive(true);
                
                // Add the instantiated object to the active dictionary
                activeTrackedImages[trackedImage.referenceImage.name] = instantiatedObject;
            }
        }
        else
        {
            Debug.LogWarning($"Object with ID {trackedImage.referenceImage.name} not found in the library.");
        }
    }

    private void UpdateObject(ARTrackedImage trackedImage)
    {
        // Only update objects if they are actively being tracked
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            if (activeTrackedImages.ContainsKey(trackedImage.referenceImage.name))
            {
                GameObject obj = activeTrackedImages[trackedImage.referenceImage.name];
                obj.SetActive(true);
                obj.transform.position = trackedImage.transform.position;
                obj.transform.rotation = trackedImage.transform.rotation;
            }
        }
        else
        {
            // Deactivate the object if it's no longer being tracked
            if (activeTrackedImages.ContainsKey(trackedImage.referenceImage.name))
            {
                activeTrackedImages[trackedImage.referenceImage.name].SetActive(false);
            }
        }
    }

    private void RemoveObject(ARTrackedImage trackedImage)
    {
        // Deactivate and remove the object when it's no longer tracked
        if (activeTrackedImages.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject obj = activeTrackedImages[trackedImage.referenceImage.name];
            Destroy(obj); // Destroy the instantiated object
            activeTrackedImages.Remove(trackedImage.referenceImage.name);
        }
    }
}