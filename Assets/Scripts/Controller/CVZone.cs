using UnityEngine;
using ProjectAvalanche.Data;

public class CVZone : MonoBehaviour
{
    private CVSection _sectionType;

    // YENÝ: UIManager'ý hafýzada tutacaðýmýz deðiþken
    private UIManager _uiManager;

    private void Start()
    {
        // YENÝ: UIManager'ý oyun baþladýðýnda sadece 1 KERE bul ve hafýzaya al.
        // Artýk her araba çýktýðýnda sahnede arama yapýlmayacak.
        _uiManager = FindFirstObjectByType<UIManager>();
    }

    public void Setup(CVSection type)
    {
        _sectionType = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        // YENÝ: TryGetComponent kullanmak, GetComponent != null demekten çok daha performanslýdýr.
        if (other.TryGetComponent(out VehicleController vehicle))
        {
            StationTrigger.TriggerEvent(_sectionType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out VehicleController vehicle))
        {
            // Hafýzadaki (cache) UI Manager'ý direkt kullan
            if (_uiManager != null)
            {
                _uiManager.ClosePanel();
            }
        }
    }
}