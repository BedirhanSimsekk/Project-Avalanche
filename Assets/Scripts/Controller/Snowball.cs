using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(AudioSource))]
public class Snowball : MonoBehaviour
{
    [Header("Growth Settings")]
    [SerializeField] private float growthMultiplier = 0.01f;
    [SerializeField] private float maxScale = 5f;
    [SerializeField] private float releaseForce = 10f;

    [Header("Lifecycle & Melting Settings")]
    [SerializeField] private float lifeTimeAfterRelease = 5f;
    [SerializeField] private float killZ = -10f;
    [SerializeField] private float maxIdleTime = 5f;
    [SerializeField] private float shrinkSpeed = 0.5f;
    [SerializeField] private float minScaleBeforeExplosion = 0.5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionVFXPrefab;
    [SerializeField] private ParticleSystem meltingVFX;

    [Header("Audio")]
    [SerializeField] private AudioClip throwSound;
    [SerializeField][Range(0f, 1f)] private float throwVolume = 0.4f;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField][Range(0f, 1f)] private float explosionVolume = 0.8f;

    [Header("Rigidbody Settings")]
    [SerializeField] private float rbMassGain = 5f;
    [SerializeField] private float baseMass = 1f;

    private VehicleController _vehicle;
    private Rigidbody _vehicleRb;
    private Rigidbody _rb;
    private AudioSource _actionSource;
    private AudioSource _rollSource;
    private IObjectPool<Snowball> _pool;

    private bool _isAttached = true;
    private bool _isShrinking = false;
    private float _idleTimer = 0f;
    private Vector3 _initialScale;

    private void Awake()
    {
        CacheComponents();
        _initialScale = transform.localScale;
    }

    private void CacheComponents()
    {
        _rb = GetComponent<Rigidbody>();
        _actionSource = GetComponent<AudioSource>();
        _actionSource.playOnAwake = false;
        _actionSource.spatialBlend = 1f;

        _rollSource = gameObject.AddComponent<AudioSource>();
        _rollSource.loop = true;
        _rollSource.spatialBlend = 1f;
    }

    public void Initialize(VehicleController vehicle, Transform spawnPoint, IObjectPool<Snowball> pool)
    {
        _pool = pool;
        _vehicle = vehicle;
        _vehicleRb = vehicle.GetComponent<Rigidbody>();

        ResetSnowballState(spawnPoint);
        IgnoreVehicleCollisions(vehicle);
    }

    private void Update()
    {
        if (CheckFallLimit()) return;
        if (!_isAttached || _vehicle == null) return;

        HandleSnowballLogic();
    }

    private void HandleSnowballLogic()
    {
        float speed = _vehicle.CurrentSpeed;

        if (speed < 0.1f)
        {
            HandleIdleAndMelting();
        }
        else
        {
            HandleMovementAndGrowth(speed);
        }
    }

    private void HandleIdleAndMelting()
    {
        if (_rollSource.isPlaying) _rollSource.Pause();

        if (!_isShrinking)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= maxIdleTime) StartMelting();
        }
        else
        {
            ApplyShrink();
        }
    }

    private void HandleMovementAndGrowth(float speed)
    {
        StopMelting();
        _idleTimer = 0f;

        UpdateRollingSound();
        ApplyRotation(speed);
        ApplyGrowth(speed);
    }

    private void ApplyGrowth(float speed)
    {
        if (transform.localScale.x < maxScale)
        {
            float growth = speed * growthMultiplier * Time.deltaTime;
            UpdateScaleAndPosition(growth);
            UpdateMass();
        }
    }

    private void ApplyShrink()
    {
        float shrinkAmount = -shrinkSpeed * Time.deltaTime;
        UpdateScaleAndPosition(shrinkAmount);
        UpdateMass();

        if (meltingVFX != null) meltingVFX.transform.localScale = transform.localScale;
        if (transform.localScale.x <= minScaleBeforeExplosion) SelfDestruct();
    }

    private void UpdateScaleAndPosition(float amount)
    {
        transform.localScale += new Vector3(amount, amount, amount);
        transform.localPosition += new Vector3(0f, amount / 2f, amount / 2f);
    }

    private void ApplyRotation(float speed)
    {
        float radius = transform.localScale.x / 2f;
        float distanceThisFrame = speed * Time.deltaTime;
        float rotationAngle = (distanceThisFrame * 360f) / (2f * Mathf.PI * radius);
        transform.Rotate(_vehicle.transform.right, rotationAngle, Space.World);
    }

    private void UpdateMass()
{
    // Küp yerine kare kullanarak daha az agresif bir artış sağlıyoruz
    float squaredScale = Mathf.Pow(transform.localScale.x, 2);
    _rb.mass = baseMass + (squaredScale * rbMassGain);
}

    private void UpdateRollingSound()
    {
        if (rollSound == null) return;
        if (_rollSource.clip == null) _rollSource.clip = rollSound;
        if (!_rollSource.isPlaying) _rollSource.Play();

        float sizeRatio = transform.localScale.x / maxScale;
        _rollSource.volume = Mathf.Lerp(0.3f, 1f, sizeRatio);
        _rollSource.pitch = Mathf.Lerp(1.3f, 0.7f, sizeRatio);
    }

    private void StartMelting()
    {
        _isShrinking = true;
        if (meltingVFX != null && !meltingVFX.isPlaying) meltingVFX.Play();
    }

    private void StopMelting()
    {
        if (!_isShrinking) return;
        _isShrinking = false;
        if (meltingVFX != null) meltingVFX.Stop();
    }

    private bool CheckFallLimit()
    {
        if (transform.position.y < killZ)
        {
            SelfDestruct();
            return true;
        }
        return false;
    }

    public void Release()
    {
        _isAttached = false;
        transform.SetParent(null);
        _rb.isKinematic = false;

        StopMelting();
        if (_rollSource.isPlaying) _rollSource.Stop();
        PlayThrowSound();

        _rb.linearVelocity = _vehicleRb.linearVelocity;
        _rb.AddForce(_vehicle.transform.forward * releaseForce, ForceMode.Impulse);

        Invoke(nameof(SelfDestruct), lifeTimeAfterRelease);

        // Eğer Curling oyunundaysak, fırlatıldığı an kamerayı bu topa kilitle!
        if (CurlingArenaController.Instance != null && CurlingArenaController.Instance.IsArenaActive)
        {
            CurlingArenaController.Instance.SetCameraTarget(transform);
        }
    }

    private void PlayThrowSound()
    {
        if (throwSound != null && gameObject.activeInHierarchy && _actionSource.isActiveAndEnabled)
            _actionSource.PlayOneShot(throwSound, throwVolume);
    }

    private void ResetSnowballState(Transform spawnPoint)
    {
        CancelInvoke(nameof(SelfDestruct));
        transform.SetParent(spawnPoint);
        transform.position = spawnPoint.position;
        transform.localRotation = Quaternion.identity;
        transform.localScale = _initialScale;

        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        _rb.mass = baseMass;

        _isAttached = true;
        _idleTimer = 0f;
        _isShrinking = false;

        if (meltingVFX != null) meltingVFX.Stop();
        _rollSource.Stop();
    }

    private void IgnoreVehicleCollisions(VehicleController vehicle)
    {
        Collider sc = GetComponent<Collider>();
        Collider vc = vehicle.GetComponent<Collider>();
        if (sc && vc) Physics.IgnoreCollision(sc, vc);
    }

    private void SelfDestruct()
    {
        CancelInvoke(nameof(SelfDestruct));
        if (explosionVFXPrefab) VFXManager.Instance.PlayVFX(explosionVFXPrefab, transform.position, Quaternion.identity, transform.localScale, 2f);
        if (explosionSound) AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);

        // Eğer sahnede aktif bir Curling arenası varsa, patlama noktamı ona gönder!
        CurlingArenaController.Instance?.RegisterExplosion(transform.position);

        // Patladığı an kamerayı serbest bırak, pürüzsüzce arabaya geri dönsün!
        if (CurlingArenaController.Instance != null && CurlingArenaController.Instance.IsArenaActive)
        {
            CurlingArenaController.Instance.ResetCamera();
        }

        if (_pool != null) _pool.Release(this);
        else Destroy(gameObject);
    }
}