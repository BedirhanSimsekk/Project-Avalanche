using UnityEngine;
using System;
using ProjectAvalanche.Data;

public class StationTrigger : MonoBehaviour
{
    public static event Action<CVSection> OnStationUnlocked;
    public static void TriggerEvent(CVSection section) => OnStationUnlocked?.Invoke(section);

    [Header("Station Settings")]
    [SerializeField] private CVSection sectionType;
    [SerializeField] private float requiredSnowballSize = 3f;

    [Header("Links")]
    [SerializeField] private GameObject solidGlass;
    [SerializeField] private GameObject checkpointObject;

    [Header("Effects")]
    [SerializeField] private ParticleSystem shatterVFXPrefab;

    private bool _isUnlocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isUnlocked) return;

        if (other.TryGetComponent(out Snowball snowball))
        {
            if (snowball.transform.localScale.x >= requiredSnowballSize)
            {
                UnlockStation();
                Destroy(snowball.gameObject);
            }
            else
            {
                Debug.Log($"<color=yellow>Yetersiz Boyut!</color>");
            }
        }
    }

    private void UnlockStation()
    {
        _isUnlocked = true;

        if (solidGlass != null) solidGlass.SetActive(false);

        // YENÝ KOD: Cam kýrýlma efekti havuzdan çaðrýlýyor (3 saniye süreyle)
        if (shatterVFXPrefab != null)
        {
            VFXManager.Instance.PlayVFX(shatterVFXPrefab, transform.position, transform.rotation, Vector3.one, 3f);
        }

        if (checkpointObject != null)
        {
            checkpointObject.SetActive(true);

            var zone = checkpointObject.GetComponentInChildren<CVZone>();

            if (zone != null)
            {
                zone.Setup(sectionType);
            }
            else
            {
                Debug.LogWarning("DÝKKAT: CVZone kodu soru iþaretinde bulunamadý!");
            }
        }
    }
}