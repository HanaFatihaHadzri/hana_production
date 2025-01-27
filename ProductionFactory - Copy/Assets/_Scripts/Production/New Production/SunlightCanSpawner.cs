using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunlightCanSpawner : MonoBehaviour
{
    [System.Serializable]

    public class SnapPoint
    {
        public Transform point;          // The Transform for the snap point
        public string requiredIngredient; // The type of ingredient allowed for this snap point (e.g., "tortilla", "meat")
        public bool isOccupied = false;  // Tracks if the snap point is already occupied
        public GameObject snappedIngredient; // Reference to the snapped ingredient
    }

    public List<SnapPoint> snapPoints;  // List of snap points with assigned ingredient types

    public float snapRange = 0.5f;

    public GameObject sunlightPrefab; // Reference to the battery prefab
    public int batteryAmount = 1;

    public List<ProperItemHolder> sunlightHolders;
    private List<Draggable> draggableObjects = new List<Draggable>();

    void OnEnable()
    {
        Draggable.OnDraggableCreated += RegisterNewDraggable;
    }

    void OnDisable()
    {
        Draggable.OnDraggableCreated -= RegisterNewDraggable;
    }

    private void RegisterNewDraggable(Draggable draggable)
    {
        if (!draggableObjects.Contains(draggable))
        {
            draggableObjects.Add(draggable);
            draggable.dragEndedCallback = OnDragEnded;
        }
    }

    void Start()
    {
        foreach (Draggable draggable in draggableObjects)
        {
            draggable.dragEndedCallback = OnDragEnded;
        }
    }

    private void OnDragEnded(Draggable draggable)
    {
        float closestDistance = float.MaxValue;
        SnapPoint closestSnapPoint = null;

        Ingredient draggableIngredient = draggable.GetComponent<Ingredient>();
        if (draggableIngredient == null) return;

        foreach (SnapPoint snapPoint in snapPoints)
        {
            if (snapPoint.isOccupied) continue;

            if (snapPoint.requiredIngredient == draggableIngredient.type)
            {
                float currentDistance = Vector3.Distance(draggable.transform.position, snapPoint.point.position);

                if (currentDistance < closestDistance)
                {
                    closestSnapPoint = snapPoint;
                    closestDistance = currentDistance;
                }
            }
        }

        if (closestSnapPoint != null && closestDistance < snapRange)
        {
            draggable.transform.position = closestSnapPoint.point.position;
            closestSnapPoint.isOccupied = true;
            closestSnapPoint.snappedIngredient = draggable.gameObject;

            draggable.DisableDragging();
            draggable.GetComponent<Collider2D>().enabled = false;

            CheckAllIngredientsInPlace();
        }
    }


    // Method to check if all ingredients are in place
    private void CheckAllIngredientsInPlace()
    {
        bool allInPlace = true;

        // Check if all snap points are occupied
        foreach (SnapPoint snapPoint in snapPoints)
        {
            if (!snapPoint.isOccupied)
            {
                allInPlace = false;
                break;
            }
        }

        // If all snap points are occupied, create the burrito and destroy ingredients
        if (allInPlace)
        {
            InstantiateSunlightCan(sunlightPrefab, batteryAmount, sunlightHolders);
        }
    }

    // Method to instantiate the sunlight can and destroy all ingredients
    private void InstantiateSunlightCan(GameObject prefab, int amount, List<ProperItemHolder> holders)
    {

        for (int i = 0; i < amount; i++)
        {
            ProperItemHolder availableHolder = FindAvailableHolder(holders);
            if (availableHolder != null)
            {
                GameObject newItem = Instantiate(prefab, availableHolder.transform.position, Quaternion.identity);
                availableHolder.AddItem(newItem);
                destroyAllSnapIngredient();
            }
            else
            {
                destroyAllSnapIngredient();
                Debug.LogWarning("No available holders for new item");
                break;
            }
        }

    }
    private void destroyAllSnapIngredient()
    {
        // Destroy all snapped ingredients
        foreach (SnapPoint snapPoint in snapPoints)
        {
            if (snapPoint.snappedIngredient != null)
            {
                Destroy(snapPoint.snappedIngredient);
                snapPoint.snappedIngredient = null; // Clear the reference
                snapPoint.isOccupied = false; // Mark the snap point as no longer occupied
            }
        }
    }

    private ProperItemHolder FindAvailableHolder(List<ProperItemHolder> holders)
    {
        foreach (ProperItemHolder holder in holders)
        {
            if (holder.CanAddItem())
            {
                return holder;
            }
        }
        return null;
    }
}
