using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : Entity
{
    [Header("Input Actions References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference pauseAction;

    [Header("Movement Setting")]
    [SerializeField] private float speed = 15.0f;
    private float moveInput;
    private bool canPlay = true;

    [Header("Shooting Settings")]
    [SerializeField] private int bulletPoolSize = 10;
    [SerializeField] private float fireCooldown = 0.1f;
    [SerializeField] private BulletController bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform bulletContainer;
    private ObjectPool<BulletController> bulletPool;
    private float lastFireTime;

    [Header("Clips References")]
    [SerializeField] private AudioClip[] shotClips;
    private int lastShotClipIndex = -1;

    protected override void Awake()
    {
        base.Awake();

        bulletPool = new ObjectPool<BulletController>(bulletPrefab, bulletPoolSize, bulletContainer);
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (shootAction != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShootInput;
        }

        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPauseInput;
        }

        EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (shootAction != null)
        {
            shootAction.action.performed -= OnShootInput;
            shootAction.action.Disable();
        }

        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPauseInput;
            pauseAction.action.Disable();
        }

        EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
    }

    private void Update()
    {   
        if (canPlay)
        {
            ReadGameplayInput();
            Movement();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Asteroid"))
        {
            SceneManager.LoadScene("Gameplay");
        }
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        EventBus.Publish(new PauseRequestedEvent());
    }

    private void ReadGameplayInput()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>().x;
        }
    }

    private void OnShootInput(InputAction.CallbackContext context)
    {
        if (canPlay && CanFire())
        {
            Shoot();
        }
    }

    private void Movement()
    {
        Vector3 newPosition = transform.position;
        newPosition.x += moveInput * speed * Time.deltaTime;

        float leftLimit = MainCamera.GetLeftEdge() + HalfWidth;
        float rightLimit = MainCamera.GetRightEdge() - HalfWidth;

        newPosition.x = Mathf.Clamp(newPosition.x, leftLimit, rightLimit);
        transform.position = newPosition;
    }

    private bool CanFire() => Time.time >= lastFireTime + fireCooldown;

    private void Shoot()
    {
        BulletController newBullet = bulletPool.GetAvailable();

        if (!newBullet)
        {
            return;
        }

        AudioClip shotClip = GetShotClip();

        if (shotClip)
        {
            AudioManager.Instance.PlaySFX(shotClip);
        }

        newBullet.transform.position = firePoint.position;
        newBullet.Activate();

        lastFireTime = Time.time;
    }

    private AudioClip GetShotClip()
    {
        if (shotClips.Length == 0)
        {
            return null;
        }

        int randomClipIndex;

        do
        {
            randomClipIndex = Random.Range(0, shotClips.Length);
        }
        while (randomClipIndex == lastShotClipIndex && shotClips.Length > 1);

        lastShotClipIndex = randomClipIndex;

        return shotClips[randomClipIndex];
    }

    private void OnGamePaused(GamePausedEvent gamePausedEvent) => canPlay = !gamePausedEvent.IsPaused;
}