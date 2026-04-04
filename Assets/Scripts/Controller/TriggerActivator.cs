using UnityEngine;

public class TriggerActivator : MonoBehaviour
{
    [Header("Açılmasını İstediğin Obje")]
    [SerializeField] private GameObject hedefObje; 

    [Header("Ayarlar")]
    [Tooltip("Eğer işaretlersen, arabayla bu tetikleyiciye çarptığında tetikleyicinin kendisi yok olur.")]
    [SerializeField] private bool tetikleyiciYokOlsun = true;
    private void OnTriggerEnter(Collider other)
    {
        // Çarpan şeyin bizim arabamız (VehicleController) olup olmadığını kontrol ediyoruz
        if (other.TryGetComponent(out VehicleController vehicle))
        {
            // 1. Hedef objeyi AKTİF ET!
            if (hedefObje != null)
            {
                hedefObje.SetActive(true);
            }

            // 2. İstersen çarptığımız bu görünmez kutuyu yok et ki bir daha çalışmasın
            if (tetikleyiciYokOlsun)
            {
                gameObject.SetActive(false); // Sahnede gizler (performans dostudur)
            }
        }
    }
}