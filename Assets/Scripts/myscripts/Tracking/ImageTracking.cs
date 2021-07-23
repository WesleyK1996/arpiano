using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTracking : MonoBehaviour
{
    private GameObject Piano;

    private ARTrackedImageManager trackedImageManager;

    private void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        Piano = Instantiate(trackedImageManager.trackedImagePrefab);
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage img in eventArgs.added)
            UpdateImage(img);
        foreach (ARTrackedImage img in eventArgs.updated)
            UpdateImage(img);
    }

    private void UpdateImage(ARTrackedImage img)
    {
        Piano.transform.position = img.transform.position;
        Piano.SetActive(true);
    }
}
