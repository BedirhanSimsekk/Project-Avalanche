using UnityEngine;

public class PinAudio : MonoBehaviour
{
    [Header("Çarpışma Sesi")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField][Range(0f, 1f)] private float volume = 0.7f;
    
    private AudioSource _audioSource;
    
    // YENİ VE ÖNEMLİ EKLENTİ: "static" kelimesi!
    // Bu sayede 10 labutun hepsi bu tek bir saati (değişkeni) ortak kullanır.
    private static float _globalLastPlayTime; 

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0.8f; 
        _audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. KONTROL: Yavaş çarpmalarda ses çıkarma
        if (collision.relativeVelocity.magnitude < 1.5f) return;

        // 2. KONTROL: BÜTÜN labutlar için global bekleme süresi! 
        // Eğer sahnede herhangi bir labut son 0.05 saniye içinde ses çıkardıysa, diğerleri sussun.
        if (Time.time - _globalLastPlayTime < 0.05f) return;

        if (hitSound != null)
        {
            _audioSource.pitch = Random.Range(0.85f, 1.2f);
            _audioSource.PlayOneShot(hitSound, volume);
            
            // Saati güncelle ki diğer labutlar sussun
            _globalLastPlayTime = Time.time;
        }
    }
}