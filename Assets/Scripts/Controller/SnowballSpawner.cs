using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool; // YEN�: Havuz k�t�phanesi

public class SnowballSpawner : MonoBehaviour
{
    [SerializeField] private Snowball snowballPrefab;
    [SerializeField] private Transform spawnPoint;

    private VehicleController _vehicle;
    private Snowball _currentSnowball;

    // YEN�: Unity'nin yerle�ik Nesne Havuzu
    private ObjectPool<Snowball> _snowballPool;

    private void Awake()
    {
        _vehicle = GetComponent<VehicleController>();

        // Havuzu �n�a Ediyoruz
        _snowballPool = new ObjectPool<Snowball>(
            createFunc: CreateSnowball,             // 1. Havuzda eleman kalmad�ysa nas�l yenisini yaratacak?
            actionOnGet: OnGetSnowball,             // 2. Havuzdan bir obje �ekilince ne yapacak?
            actionOnRelease: OnReleaseSnowball,     // 3. Obje havuza geri d�n�nce ne yapacak?
            actionOnDestroy: OnDestroySnowball,     // 4. Havuz �ok dolarsa (limit a��l�rsa) ne yapacak?
            collectionCheck: false,                 // G�venlik kontrol� (Performans i�in false yap�yoruz)
            defaultCapacity: 5,                     // Oyuna ba�larken kenarda haz�r 5 tane beklet
            maxSize: 15                             // Havuzda maksimum 15 tane birikebilir
        );
    }

    // Input System üzerinden tetiklenecek event
    public void OnSnowballAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // --- YENİ EKLENEN GÜVENLİK KONTROLÜ ---
            // Eğer sahnede Curling arenası açıksa VE oyuncunun atış hakları bitip T'ye basması bekleniyorsa...
            if (CurlingArenaController.Instance != null && 
                CurlingArenaController.Instance.IsArenaActive && 
                CurlingArenaController.Instance.IsWaitingForRestart)
            {
                Debug.Log("<color=yellow>Atış hakkın bitti! Önce T'ye basıp sıfırlamalısın.</color>");
                return; // Aşağıdaki kodları okuma, işlemi iptal et!
            }
            // --------------------------------------

            if (_currentSnowball == null)
            {
                SpawnSnowball();
            }
            else
            {
                ThrowSnowball();
            }
        }
    }

    private void SpawnSnowball()
    {
        // Havuzdan 1 tane uyan�k kar topu iste
        _currentSnowball = _snowballPool.Get();

        // Kar topuna "S�f�rlan ve araban�n arkas�na ge�" emrini ver
        _currentSnowball.Initialize(_vehicle, spawnPoint, _snowballPool);
    }

    private void ThrowSnowball()
    {
        _currentSnowball.Release();
        _currentSnowball = null;

        // YENİ EKLENEN SATIR: Top fırlatıldığı an bowling arenasına "1 atış kullanıldı" mesajı gönder!
        FindAnyObjectByType<BowlingArenaController>()?.RegisterThrow();
    }

    // --- HAVUZ KURALLARI (Arka Planda �al��an Sistemler) ---

    private Snowball CreateSnowball()
    {
        // Sadece havuz ilk kez dolarken veya yetersiz kal�rsa Instantiate �al���r.
        return Instantiate(snowballPrefab);
    }

    private void OnGetSnowball(Snowball snowball)
    {
        // Havuzdan �ekilen kar topunu g�r�n�r yap
        snowball.gameObject.SetActive(true);
    }

    private void OnReleaseSnowball(Snowball snowball)
    {
        // Havuza d�nen kar topunu tamamen gizle ve dinlenmeye al
        snowball.gameObject.SetActive(false);
    }

    private void OnDestroySnowball(Snowball snowball)
    {
        // E�er havuz kapasitesi (15) a��l�rsa, fazlal�klar� ger�ekten bellekten sil
        Destroy(snowball.gameObject);
    }
}