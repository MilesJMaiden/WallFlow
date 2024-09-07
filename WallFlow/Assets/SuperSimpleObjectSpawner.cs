using UnityEngine;

public class SuperSimpleObjectSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject[] prefabs; // Array of prefabs to spawn
    [SerializeField] private Camera mainCamera; // Reference to the player's main camera (headset)

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Audio source for playing sounds
    [SerializeField] private AudioClip spawnSound; // Sound to play when spawning an object

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 1.5f; // Distance in front of the player to spawn the object

    /// <summary>
    /// Spawns a random object from the array in front of the player.
    /// </summary>
    public void SpawnRandomObject()
    {
        if (prefabs.Length == 0)
        {
            Debug.LogWarning("Prefab array is empty. Cannot spawn an object.");
            return;
        }

        // Select a random prefab from the array
        GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

        // Calculate spawn position in front of the player's camera
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * spawnDistance;

        // Instantiate the prefab at the calculated position
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // Set initial scale to zero for tweening effect
        spawnedObject.transform.localScale = Vector3.zero;

        // Tween the scale from 0 to the prefab's original scale using LeanTween
        LeanTween.scale(spawnedObject, prefabToSpawn.transform.localScale, 0.5f).setEaseOutBounce();

        // Play spawn sound
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or spawn sound is not assigned.");
        }

        // Rotate the object to face the player
        spawnedObject.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, Vector3.up);
    }

    private void Start()
    {
        // Fallback to main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
}
