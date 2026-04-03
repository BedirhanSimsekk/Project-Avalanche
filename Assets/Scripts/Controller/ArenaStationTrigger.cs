using UnityEngine;
using ProjectAvalanche.Data; // CVSection enum'ýna ulaþmak için gerekli

public class ArenaStationTrigger : MonoBehaviour
{
    [Header("Ýstasyon Bilgisi")]
    [Tooltip("Bu soru iþaretine girince hangi CV bölümü açýlacak?")]
    [SerializeField] private CVSection sectionType;

    private UIManager _uiManager;

    private void Start()
    {
        // Oyuncu alandan çýkýnca paneli kapatabilmek için UIManager'ý bulup hafýzaya alýyoruz
        _uiManager = FindFirstObjectByType<UIManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Eðer bu görünmez alana giren þey bizim Arabamýz ise...
        if (other.TryGetComponent(out VehicleController vehicle))
        {
            // Mevcut sistemini kullanarak UI'a "Seçili paneli aç" komutunu gönderiyoruz
            StationTrigger.TriggerEvent(sectionType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Arabamýz bu alandan uzaklaþýrsa...
        if (other.TryGetComponent(out VehicleController vehicle))
        {
            // Açýk olan paneli otomatik kapat
            if (_uiManager != null)
            {
                _uiManager.ClosePanel();
            }
        }
    }
}