using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleAudio : MonoBehaviour
{
    [Header("Ses Dosyas�")]
    [SerializeField] private AudioClip engineClip;

    [Header("Motor Sesi Ayarlar�")]
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 2.0f;
    [SerializeField] private float maxSpeedForPitch = 25f;

    [Header("Dinamik Titre�im (Yeni)")]
    [SerializeField] private float loadPitchMultiplier = 0.2f; // �vmelenirken (gaza y�klenirken) eklenecek ekstra ba��rma
    [SerializeField] private float vibrationIntensity = 0.05f; // Y�ksek h�zdaki rastgele dalgalanma (titreme) miktar�

    private VehicleController _vehicle;
    private AudioSource _engineSource;
    private float _previousSpeed;

    private void Awake()
    {
        _vehicle = GetComponent<VehicleController>();

        _engineSource = gameObject.AddComponent<AudioSource>();
        _engineSource.loop = true;
        _engineSource.spatialBlend = 1f;
        _engineSource.volume = 0.7f;
        _engineSource.dopplerLevel = 0f;
    }

    private void Start()
    {
        if (engineClip != null)
        {
            _engineSource.clip = engineClip;
            _engineSource.Play();
        }
    }

    private void Update()
    {
        if (_vehicle == null) return;

        float currentSpeed = _vehicle.CurrentSpeed;

        // 1. Temel Pitch (H�za G�re)
        float pitchRatio = currentSpeed / maxSpeedForPitch;
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, pitchRatio);

        // 2. Motor Y�k� (�vmelenme)
        // E�er araba h�zlan�yorsa (�u anki h�z, bir �nceki kareden b�y�kse), motora y�k biniyordur.
        float acceleration = (currentSpeed - _previousSpeed) / Time.deltaTime;
        if (acceleration > 0.5f) // Sadece belirgin bir h�zlanma varsa
        {
            // �vmeye g�re anl�k bir ba��rma (pitch art���) ekle
            targetPitch += Mathf.Clamp(acceleration * 0.02f, 0f, loadPitchMultiplier);
        }

        // 3. Y�ksek H�z Titre�imi (Zorlanma Hissi)
        if (currentSpeed > 5f)
        {
            // PerlinNoise kullanarak mekanik, organik bir titre�im yarat�yoruz (Robotik olmamas� i�in Random kullanmad�k)
            float noise = Mathf.PerlinNoise(Time.time * 15f, 0f) * 2f - 1f; // -1 ile 1 aras� dalgalanma
            targetPitch += noise * vibrationIntensity * pitchRatio; // H�z artt�k�a titre�im de arts�n
        }

        // 4. Sesi Uygulama (Yumu�ak Ge�i�)
        // Kulak t�rmalamamas� i�in hesaplanan yeni Pitch de�erine yumu�ak bir ge�i� (Lerp) yap�yoruz
        _engineSource.pitch = Mathf.Lerp(_engineSource.pitch, targetPitch, Time.deltaTime * 10f);

        // Bir sonraki kare i�in h�z� kaydet
        _previousSpeed = currentSpeed;
    }
}