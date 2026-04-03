using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;

// "abstract" kelimesi bunun tek başına kullanılamayacak bir "Şablon" olduğunu belirtir.
// Yani Unity'de bir objenin üzerine "BaseArenaController" sürükleyip bırakamazsın.
public abstract class BaseArenaController : MonoBehaviour
{
    // --- ERİŞİM BELİRLEYİCİLER (protected vs private) ---
    // "private" yazsaydık, sadece bu kod bunları görebilirdi. 
    // "protected" yazdığımız için, bu koddan MİRAS ALAN çocuklar (Curling/Bowling) da bunları kullanabilir!

    [Header("--- TEMEL ARENA AYARLARI ---")]
    [SerializeField] protected Collider startTrigger; // Arenaya girilen görünmez kutu
    [SerializeField] protected Transform arenaContent; // Yerden çıkacak olan asıl arena
    [SerializeField] protected GameObject startObject; // R'ye basınca gizlenecek kutu
    
    [SerializeField] protected float yOffset = -5f; // Arena ne kadar derinden çıkacak?
    [SerializeField] protected float animDuration = 1.5f; // Çıkma animasyonu kaç saniye sürecek?
    [SerializeField] protected int maxThrows = 3; // Varsayılan atış hakkı

    [Header("--- TEMEL UI AYARLARI ---")]
    [SerializeField] protected GameObject mainPanel; // Kalan atış vs. gösteren ana arayüz
    [SerializeField] protected GameObject startPromptUI; // [R] PLAY yazısı
    [SerializeField] protected GameObject exitPromptUI; // [F] EXIT yazısı
    [SerializeField] protected TextMeshProUGUI throwsLeftText;

    [Header("--- TEMEL SES AYARLARI ---")]
    [SerializeField] protected AudioClip arenaOpenSound;
    [SerializeField] protected AudioClip arenaCloseSound;
    [SerializeField][Range(0f, 1f)] protected float volume = 0.8f;

    protected AudioSource audioSource;
    protected bool isArenaActive = false;
    protected bool isPlayerInStartZone = false;
    protected int currentThrows;

    protected Vector3 arenaUpPosition;
    protected Vector3 arenaDownPosition;

    // Dışarıdaki kodların (mesela Snowball) arenanın açık olup olmadığını öğrenmesi için kapı
    public bool IsArenaActive => isArenaActive; 

    // --- VIRTUAL (SANAL) METOTLAR ---
    // Bir metodun başına "virtual" yazarsan çocuklara şunu dersin: 
    // "Benim burada temel bir işleyişim var ama isterseniz siz kendi kodunuzda bunu ezip (override) üzerine eklemeler yapabilirsiniz!"
    
    protected virtual void Awake()
    {
        // Ses çaları yaratıp ayarlıyoruz
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    protected virtual void Start()
    {
        // Oyun başlarken arenayı yerin dibine saklama işlemi
        if (arenaContent != null)
        {
            arenaUpPosition = arenaContent.position;
            arenaDownPosition = arenaUpPosition + new Vector3(0, yOffset, 0);
            arenaContent.position = arenaDownPosition;
            arenaContent.gameObject.SetActive(false);
        }

        // Bütün UI'ları başlangıçta gizle
        if (mainPanel != null) mainPanel.SetActive(false);
        if (startPromptUI != null) startPromptUI.SetActive(false);
        if (exitPromptUI != null) exitPromptUI.SetActive(false);
        if (throwsLeftText != null) throwsLeftText.gameObject.SetActive(false);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isArenaActive) return;

        // Arabayla girilirse R yazısını zıplatarak aç
        if (other.TryGetComponent(out VehicleController vehicle) && startPromptUI != null)
        {
            isPlayerInStartZone = true;
            startPromptUI.SetActive(true);
            startPromptUI.transform.DOKill();
            startPromptUI.transform.localScale = Vector3.zero;
            startPromptUI.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        // Arabayla çıkılırsa R yazısını küçülterek kapat
        if (!isArenaActive && other.TryGetComponent(out VehicleController vehicle) && startPromptUI != null)
        {
            isPlayerInStartZone = false;
            startPromptUI.transform.DOKill();
            startPromptUI.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => startPromptUI.SetActive(false));
        }
    }

    protected virtual void Update()
    {
        // R Tuşu: Ortak giriş sistemi
        if (!isArenaActive && isPlayerInStartZone && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            OpenArena();
            if(startObject != null) startObject.GetComponent<MeshRenderer>().enabled = false; 
        }

        // F Tuşu: Ortak çıkış sistemi
        if (isArenaActive && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            CloseArena();
            if(startObject != null) startObject.GetComponent<MeshRenderer>().enabled = true; 
        }
    }

    protected virtual void OpenArena()
    {
        isArenaActive = true;
        isPlayerInStartZone = false;

        // Sesleri ve UI'ları aç
        if (arenaOpenSound != null) audioSource.PlayOneShot(arenaOpenSound, volume);
        if (mainPanel != null) mainPanel.SetActive(true);
        if (startPromptUI != null) startPromptUI.SetActive(false);
        if (exitPromptUI != null)
        {
            exitPromptUI.SetActive(true);
            exitPromptUI.transform.DOKill();
            exitPromptUI.transform.localScale = Vector3.zero;
            exitPromptUI.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
        if (startTrigger != null) startTrigger.enabled = false;

        currentThrows = maxThrows;
        UpdateBaseUI();

        // DİKKAT: Baba sınıf burada çocuğa dönüp diyor ki: "Ben kendi işlerimi bitirdim, şimdi sen kendi oyununa özel (Labut dizme, kamera sıfırlama) işlerini yap."
        ResetGameSpecifics(); 

        // Arenayı yerden asansör gibi çıkar
        if (arenaContent != null)
        {
            arenaContent.gameObject.SetActive(true);
            arenaContent.DOMove(arenaUpPosition, animDuration).SetEase(Ease.OutBack);
        }
    }

    protected virtual void CloseArena()
    {
        isArenaActive = false;
        
        if (arenaCloseSound != null) audioSource.PlayOneShot(arenaCloseSound, volume);

        if (exitPromptUI != null)
        {
            exitPromptUI.transform.DOKill();
            exitPromptUI.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => exitPromptUI.SetActive(false));
        }

        if (throwsLeftText != null) throwsLeftText.gameObject.SetActive(false);

        // Çocuğun (Curling/Bowling) kapanış işlemlerini çağır (Örn: Geri sayımı iptal et)
        CloseGameSpecifics();

        // Arenayı yerin altına göm
        if (arenaContent != null)
        {
            arenaContent.DOMove(arenaDownPosition, animDuration).SetEase(Ease.InBack).OnComplete(() => 
            {
                arenaContent.gameObject.SetActive(false);
                if (mainPanel != null) mainPanel.SetActive(false);
                if (startTrigger != null) startTrigger.enabled = true;
            });
        }
    }

    protected virtual void UpdateBaseUI()
    {
        if (throwsLeftText != null)
        {
            throwsLeftText.gameObject.SetActive(true);
            throwsLeftText.text = $"Throws Left: {currentThrows}";
        }
    }

    // --- ABSTRACT (ZORUNLU) METOTLAR ---
    // Başına "abstract" koyarsan, bu sınıftan miras alan HER ÇOCUK bu metodu yazmak ZORUNDADIR.
    // Yoksa Unity hata verir. Bu sayede unutmanı engelleriz.
    protected abstract void ResetGameSpecifics();
    protected abstract void CloseGameSpecifics();
}