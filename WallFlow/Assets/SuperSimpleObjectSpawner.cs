using UnityEngine;

public class SuperSimpleObjectSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject[] prefabs; // Array of prefabs to spawn

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint; // Reference transform for spawning objects

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Audio source for playing sounds
    [SerializeField] private AudioClip spawnSound; // Sound to play when spawning an object

    [Header("Input Settings")]
    [Tooltip("Key to spawn an object manually.")]
    [SerializeField] private KeyCode spawnKey = KeyCode.Space; // Default key to spawn objects

    private void Update()
    {
        // Check if the spawn key is pressed
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnRandomObject();
        }
    }

    /// <summary>
    /// Spawns a random object from the array at the specified spawn point.
    /// </summary>
    public void SpawnRandomObject()
    {
        if (prefabs.Length == 0)
        {
            Debug.LogWarning("Prefab array is empty. Cannot spawn an object.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point is not assigned. Cannot spawn an object.");
            return;
        }

        // Select a random prefab from the array
        GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

        // Use the position, rotation, and scale of the spawnPoint for spawning
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;
        Vector3 spawnScale = prefabToSpawn.transform.localScale; // Using prefab's original scale

        // Instantiate the prefab at the spawnPoint's position and rotation
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);

        // Set initial scale to zero for tweening effect
        spawnedObject.transform.localScale = Vector3.zero;

        // Tween the scale from 0 to the prefab's original scale using LeanTween
        LeanTween.scale(spawnedObject, spawnScale, 0.5f).setEaseOutBounce();

        // Play spawn sound
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or spawn sound is not assigned.");
        }
    }
}
