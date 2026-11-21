using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private int asteroidPoolSize = 20;
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private AsteroidController asteroidPrefab;
    [SerializeField] private Transform asteroidContainer;
    private ObjectPool<AsteroidController> asteroidPool;
    private float timer = 0.0f;

    private Camera mainCamera;

    private void Awake()
    {
        asteroidPool = new ObjectPool<AsteroidController>(asteroidPrefab, asteroidPoolSize, asteroidContainer);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnAsteroid();
            timer = 0.0f;
        }
    }

    private void SpawnAsteroid()
    {
        AsteroidController newAsteroid = asteroidPool.GetAvailable();

        if (!newAsteroid)
        {
            return;
        }

        float positionX = GetRandomScreenPositionX(newAsteroid.HalfWidth);
        float positionY = mainCamera.GetTopEdge() + newAsteroid.HalfHeight;
        Vector3 newAsteroidPosition = new Vector3(positionX, positionY, 0.0f);

        newAsteroid.transform.position = newAsteroidPosition;
        newAsteroid.Activate();
    }

    private float GetRandomScreenPositionX(float asteroidHalfWidth)
    {
#if UNITY_ANDROID
        float left = mainCamera.GetSafeLeftEdge() + asteroidHalfWidth;
        float right = mainCamera.GetSafeRightEdge() - asteroidHalfWidth;
#else
        float left = mainCamera.GetLeftEdge() + asteroidHalfWidth;
        float right = mainCamera.GetRightEdge() - asteroidHalfWidth;
#endif

        return Random.Range(left, right);
    }
}