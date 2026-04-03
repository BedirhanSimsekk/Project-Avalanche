using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AutoResetObject : MonoBehaviour
{
    [Header("Sıfırlama Ayarları")]
    [Tooltip("Obje devrilip hareketsiz kaldıktan KAÇ SANİYE sonra yerine dönsün?")]
    public float resetDelay = 4f;
    
    [Tooltip("Yerine dönerkenki süzülme hızı (Sihirli bir efekt verir)")]
    public float returnSpeed = 5f;

    private Vector3 _startPos;
    private Quaternion _startRot;
    private Rigidbody _rb;
    
    private float _idleTimer = 0f;
    private bool _isReturning = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        // Oyun başladığı an objenin ilk durduğu yeri ve açıyı hafızaya alıyoruz
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

    void Update()
    {
        // EĞER OBJE ŞU AN YERİNE DÖNÜYORSA:
        if (_isReturning)
        {
            // Fizik kurallarını kapatıp objeyi yumuşakça (Lerp) eski yerine doğru çekiyoruz
            transform.position = Vector3.Lerp(transform.position, _startPos, Time.deltaTime * returnSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, _startRot, Time.deltaTime * returnSpeed);

            // Eğer eski yerine santimetrelerce yaklaştıysa, tam yerine oturt ve işlemi bitir
            if (Vector3.Distance(transform.position, _startPos) < 0.05f && Quaternion.Angle(transform.rotation, _startRot) < 2f)
            {
                transform.position = _startPos;
                transform.rotation = _startRot;
                
                _isReturning = false;
                _rb.isKinematic = false; // Fiziği ve yerçekimini geri aç
            }
            return; // Dönüş bitene kadar aşağıdaki kodları okuma
        }

        // --- NORMAL DURUM KONTROLÜ ---
        
        // Obje şu an hareket ediyor mu? (Araba veya top çarptıysa hız 0.1'den büyük olur)
        bool isMoving = _rb.linearVelocity.magnitude > 0.1f || _rb.angularVelocity.magnitude > 0.1f;
        
        // Obje ilk doğduğu yerden uzaklaşmış veya devrilmiş mi?
        bool isDisplaced = Vector3.Distance(transform.position, _startPos) > 0.1f || Quaternion.Angle(transform.rotation, _startRot) > 5f;

        // EĞER obje devrilmişse AMA şu an hareket etmiyorsa (yerde duruyorsa)
        if (!isMoving && isDisplaced)
        {
            _idleTimer += Time.deltaTime; // Kronometreyi çalıştır

            if (_idleTimer >= resetDelay)
            {
                StartReturn(); // Süre doldu, eve dönüş başlasın!
            }
        }
        else
        {
            // Eğer objeye biri tekrar çarparsa veya obje hala yuvarlanıyorsa kronometreyi sıfırla
            _idleTimer = 0f;
        }
    }

    private void StartReturn()
    {
        _isReturning = true;
        _idleTimer = 0f;
        
        // Önce hızları sıfırlıyoruz (Hala fizikselken)
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        // Sonra fiziği kapatıyoruz
        _rb.isKinematic = true; 
    }
}