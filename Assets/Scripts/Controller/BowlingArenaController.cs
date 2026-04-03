using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

// Yine Baba'dan (BaseArenaController) miras alıyoruz.
public class BowlingArenaController : BaseArenaController
{
    [Header("--- BOWLING ÖZEL AYARLAR ---")]
    [SerializeField] private Transform labutsParent; // Labutların klasörü

    [Header("--- BOWLING ÖZEL UI ---")]
    [SerializeField] private TextMeshProUGUI countdownText; 
    [SerializeField] private GameObject[] resultUIs; 

    [Header("--- BOWLING ÖZEL SESLER ---")]
    [SerializeField] private AudioClip strikeSound;      
    [SerializeField] private AudioClip failSound;        
    [SerializeField] private AudioClip tickSound;        
    
    private bool _isStrikeTriggered = false;
    private bool _isCheckingResult = false; 
    private float _canCheckStrikeTime = 0f; 

    // O bahsettiğimiz Labutların amelelik değişkenleri :)
    private Transform[] _pins;
    private Vector3[] _pinStartPos;
    private Quaternion[] _pinStartRot;
    private Rigidbody[] _pinRbs;
    private Vector3[] _pinTopDirections; 

    private Tween _checkResultTween;

    protected override void Start()
    {
        base.Start(); // Babam asansörü gizlesin

        // Ben de kendi UI'larımı gizleyeyim
        if (resultUIs != null)
        {
            foreach (var ui in resultUIs)
            {
                if (ui != null) ui.SetActive(false);
            }
        }
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        // Labutların ilk pozisyonlarını, açılarını ve ağırlıklarını hafızaya kazıdığımız kısım
        if (labutsParent != null)
        {
            int pinCount = labutsParent.childCount;
            _pins = new Transform[pinCount];
            _pinStartPos = new Vector3[pinCount];
            _pinStartRot = new Quaternion[pinCount];
            _pinRbs = new Rigidbody[pinCount];
            _pinTopDirections = new Vector3[pinCount]; 

            for (int i = 0; i < pinCount; i++)
            {
                _pins[i] = labutsParent.GetChild(i);
                _pinStartPos[i] = _pins[i].localPosition; 
                _pinStartRot[i] = _pins[i].localRotation;
                _pinRbs[i] = _pins[i].GetComponent<Rigidbody>();
                _pinTopDirections[i] = Quaternion.Inverse(_pins[i].rotation) * Vector3.up;
            }
        }
    }

    protected override void Update()
    {
        base.Update(); // Babam R ve F tuşlarını dinlemeye devam etsin

        // Eğer oyun açıksa ve atış yapıldıysa ben sürekli Strike oldu mu diye labutları kontrol edeyim
        if (isArenaActive && !_isStrikeTriggered && Time.time >= _canCheckStrikeTime)
        {
            CheckForStrike();
        }
    }

    // --- BABAMIN "ZORUNLU" TUTTUĞU METOTLAR ---
    // Babam: "Oyuna girilince arenanızı tazeleyin." dediği için labutları dikiyorum.
    protected override void ResetGameSpecifics()
    {
        ResetPinsInstantly();
    }

    // Babam: "Oyundan çıkılınca arkayı temizleyin." dediği için UI'ları ve sayaçları kapatıyorum.
    protected override void CloseGameSpecifics()
    {
        _checkResultTween?.Kill(); // Kalan beklemeleri iptal et
        StopAllCoroutines(); // Geri sayımı durdur
        
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (resultUIs != null)
        {
            foreach (var ui in resultUIs)
            {
                if (ui != null)
                {
                    ui.transform.DOKill();
                    ui.SetActive(false);
                }
            }
        }
    }

    // === SADECE BANA (BOWLING'E) AİT ÖZEL SİSTEMLER ===
    public void RegisterThrow()
    {
        if (!isArenaActive || _isStrikeTriggered || currentThrows <= 0 || _isCheckingResult) return;

        currentThrows--;
        UpdateBaseUI(); // Babam kalan atış UI'ını silsin

        if (currentThrows <= 0)
        {
            _isCheckingResult = true; 
            
            // Eğer 3 atış bittiyse ve hala strike olmadıysa, 6 saniye bekle ve sonra Try Again (Tekrar Dene) göster
            _checkResultTween = DOVirtual.DelayedCall(6f, () => 
            {
                if (!_isStrikeTriggered)
                {
                    ShowTryAgainAndReset();
                }
            });
        }
    }

    private void CheckForStrike()
    {
        if (_pins == null || _pins.Length == 0) return;

        int fallenCount = 0;

        // Her labutun tepe noktasının dik (Up) vektörüyle olan açısını (Angle) ölç
        for (int i = 0; i < _pins.Length; i++)
        {
            Vector3 currentTopDirection = _pins[i].rotation * _pinTopDirections[i];
            if (Vector3.Angle(currentTopDirection, Vector3.up) > 35f) // 35 derece devrildiyse düşmüş sayılır
            {
                fallenCount++;
            }
        }

        if (fallenCount == _pins.Length)
        {
            TriggerStrike();
        }
    }

    private void TriggerStrike()
    {
        _isStrikeTriggered = true;
        
        // Babamın (Base) audioSource değişkeniyle kendi Strike sesimi çalıyorum
        if (strikeSound != null) audioSource.PlayOneShot(strikeSound, volume);

        if (throwsLeftText != null) throwsLeftText.gameObject.SetActive(false);

        if (resultUIs != null && resultUIs.Length > 0 && resultUIs[0] != null)
        {
            GameObject strikeUI = resultUIs[0];

            strikeUI.SetActive(true);
            strikeUI.transform.localScale = Vector3.zero;
            strikeUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            StartCoroutine(CountdownAndResetRoutine(strikeUI));
        }
    }

    private void ShowTryAgainAndReset()
    {
        if (failSound != null) audioSource.PlayOneShot(failSound, volume);

        if (resultUIs != null && resultUIs.Length > 1 && resultUIs[1] != null)
        {
            GameObject tryAgainUI = resultUIs[1];

            tryAgainUI.SetActive(true);
            tryAgainUI.transform.localScale = Vector3.zero;
            tryAgainUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            StartCoroutine(CountdownAndResetRoutine(tryAgainUI));
        }
        else
        {
            ResetPinsInstantly();
        }
    }

    // 3-2-1 geri sayım rutini
    private IEnumerator CountdownAndResetRoutine(GameObject activeUI)
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (tickSound != null) audioSource.PlayOneShot(tickSound, volume * 0.5f); 

            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.transform.localScale = Vector3.one * 1.5f;
                countdownText.transform.DOScale(Vector3.one, 0.3f);
            }
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (activeUI != null)
        {
            activeUI.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => 
            {
                activeUI.SetActive(false);
                ResetPinsInstantly(); 
            });
        }
        else
        {
            ResetPinsInstantly();
        }
    }

    private void ResetPinsInstantly()
    {
        _checkResultTween?.Kill();
        
        _isStrikeTriggered = false;
        _isCheckingResult = false;
        _canCheckStrikeTime = Time.time + 0.5f; 

        currentThrows = maxThrows;
        UpdateBaseUI();
        
        StopAllCoroutines();
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (resultUIs != null)
        {
            foreach (var ui in resultUIs)
            {
                if (ui != null)
                {
                    ui.transform.DOKill();
                    ui.SetActive(false);
                }
            }
        }

        if (_pins == null) return;

        // Labutları fizikten koparıp tam santimi santimine eski yerlerine teleport ediyoruz
        for (int i = 0; i < _pins.Length; i++)
        {
            if (_pinRbs[i] != null)
            {
                _pinRbs[i].linearVelocity = Vector3.zero;
                _pinRbs[i].angularVelocity = Vector3.zero;
                _pinRbs[i].isKinematic = true; 

                _pins[i].localPosition = _pinStartPos[i];
                _pins[i].localRotation = _pinStartRot[i];

                _pinRbs[i].isKinematic = false; 
            }
        }
    }
}