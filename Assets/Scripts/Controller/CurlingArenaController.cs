using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; 
using DG.Tweening;
using TMPro;

// "BaseArenaController"dan miras alıyoruz (Inheritance).
// Yani Baba'nın "protected" olan tüm değişkenleri ve "virtual" olan tüm metotları artık bizim emrimizde!
public class CurlingArenaController : BaseArenaController
{
    [Header("--- CURLING ÖZEL AYARLAR ---")]
    [SerializeField] private Transform targetCenter; 
    [SerializeField] private float maxScoreDistance = 10f; 
    [SerializeField] private CinemachineCamera topDownCamera; 

    [Header("--- CURLING ÖZEL UI ---")]
    [SerializeField] private GameObject restartPromptUI; 
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("--- CURLING ÖZEL SESLER ---")]
    [SerializeField] private AudioClip scoreSound;
    [SerializeField] private AudioClip missSound;  
    [SerializeField] private AudioClip tickSound; 

    private bool _isWaitingForRestart = false; 
    private int _totalScore = 0;

    // Sahnede sadece 1 tane olduğu için kendini dışarıya (Snowball.cs'ye) tanıtıyor
    public static CurlingArenaController Instance;
    public bool IsWaitingForRestart => _isWaitingForRestart;

    // --- OVERRIDE (ÜZERİNE YAZMA) ---
    // Babamın "virtual" Awake metodunu eziyorum (override). 
    protected override void Awake()
    {
        // "base.Awake()" diyerek önce Babamın Awake işlerini (ses sistemi kurma) yapmasını sağlıyorum...
        base.Awake(); 
        
        // ...sonra da kendi özel işimi (Instance ataması) yapıyorum!
        Instance = this;
    }

    protected override void Start()
    {
        base.Start(); // Baba arenayı gizlesin.
        
        // Ben de curling'e özel yazıları gizleyeyim.
        if (restartPromptUI != null) restartPromptUI.SetActive(false); 
        if (scoreText != null) scoreText.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update(); // Baba R ve F tuşlarını kontrol etsin.

        // Ben de Curling oyununda olan T (Tekrar) tuşunu kontrol edeyim.
        if (isArenaActive && _isWaitingForRestart && Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            RestartGameInstantly();
        }
    }

    // --- BABAMIN "ZORUNLU" TUTTUĞU METOTLAR ---
    // Babam bana "Oyuna girildiğinde senin özel olarak sıfırlaman gereken bir şey var mı?" diye soruyor.
    protected override void ResetGameSpecifics()
    {
        _isWaitingForRestart = false;
        _totalScore = 0;
        
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = $"Total Score: {_totalScore}";
        }
    }

    // Babam bana "Oyundan çıkıldığında özel olarak kapatman gereken bir şey var mı?" diye soruyor.
    protected override void CloseGameSpecifics()
    {
        _isWaitingForRestart = false; 
        
        if (restartPromptUI != null)
        {
            restartPromptUI.transform.DOKill();
            restartPromptUI.SetActive(false);
        }
        
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        
        ResetCamera(); // Çıkarken kameram havada kaldıysa arabaya zorla geri döndür
    }

    // === SADECE BANA (CURLING'E) AİT ÖZEL SİSTEMLER ===
    // (Buraları zaten biliyorsun, kamerayı ayarlayıp puan hesapladığımız yerler)
    public void SetCameraTarget(Transform snowballTransform)
    {
        if (topDownCamera != null)
        {
            topDownCamera.Follow = snowballTransform;
            topDownCamera.LookAt = snowballTransform;
            topDownCamera.Priority = 20; 
        }
    }

    public void ResetCamera()
    {
        if (topDownCamera != null)
        {
            topDownCamera.Priority = 0; 
        }
    }

    public void RegisterExplosion(Vector3 explosionPosition)
    {
        // "isArenaActive" veya "currentThrows" gibi değişkenleri benim tanımlamamama rağmen
        // Baba sınıfından bana miras kaldığı için direkt kullanabiliyorum!
        if (!isArenaActive || currentThrows <= 0 || _isWaitingForRestart) return;

        float distanceToCenter = Vector3.Distance(targetCenter.position, explosionPosition);
        int pointsEarned = 0;

        if (distanceToCenter <= maxScoreDistance)
        {
            float scoreRatio = 1f - (distanceToCenter / maxScoreDistance);
            pointsEarned = Mathf.RoundToInt(scoreRatio * 100f);
        }

        _totalScore += pointsEarned;
        currentThrows--;

        // Ses çalarken Babamın "audioSource" ve "volume" değişkenlerini kullanıyorum
        if (pointsEarned > 50 && scoreSound != null) audioSource.PlayOneShot(scoreSound, volume); 
        else if (pointsEarned > 0 && scoreSound != null) audioSource.PlayOneShot(scoreSound, volume * 0.5f); 
        else if (missSound != null) audioSource.PlayOneShot(missSound, volume); 

        UpdateBaseUI(); // Babamın Kalan atış UI'ını güncellemesini emrediyorum
        UpdateCurlingUI(); // Kendi puan UI'ımı güncelliyorum

        if (currentThrows <= 0)
        {
            _isWaitingForRestart = true;
            
            if (restartPromptUI != null)
            {
                restartPromptUI.SetActive(true);
                restartPromptUI.transform.localScale = Vector3.zero;
                restartPromptUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }
        }
    }

    private void RestartGameInstantly()
    {
        if (tickSound != null) audioSource.PlayOneShot(tickSound, volume); 

        if (restartPromptUI != null)
        {
            restartPromptUI.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => restartPromptUI.SetActive(false));
        }

        currentThrows = maxThrows; 
        UpdateBaseUI();
        ResetGameSpecifics();
    }

    private void UpdateCurlingUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Total Score: {_totalScore}";
            scoreText.transform.DOKill(true);
            scoreText.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 10, 1f); 
        }
    }
}