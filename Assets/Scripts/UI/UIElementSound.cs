using UnityEngine;
using UnityEngine.EventSystems; // Fare olaylarý için

// Herhangi bir UI objesine eklendiðinde "Üzerine Gelme" ve "Týklama" olaylarýný otomatik algýlar
public class UIElementSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Ses Ayarlarý")]
    [SerializeField] private AudioClip hoverSound;   // Üzerine gelince çýkacak ses
    [SerializeField] private AudioClip clickSound;   // Týklayýnca çýkacak ses (Ýsteðe baðlý)
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource _audioSource;

    private void Awake()
    {
        // Kod hangi objeye atýlýrsa atýlsýn, ona otomatik bir hoparlör ekler
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    // 1. FARE ÜZERÝNE GELDÝÐÝNDE ÇALIÞIR
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            _audioSource.PlayOneShot(hoverSound, volume);
        }
    }

    // 2. FAREYLE TIKLANDIÐINDA ÇALIÞIR
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            _audioSource.PlayOneShot(clickSound, volume);
        }
    }
}