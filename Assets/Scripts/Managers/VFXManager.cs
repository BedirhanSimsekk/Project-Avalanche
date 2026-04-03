using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;

public class VFXManager : MonoBehaviour
{
    // Projedeki her kodun bu yöneticiye kolayca ulaþabilmesi için Singleton (Tekil) yapýsý
    public static VFXManager Instance { get; private set; }

    // Hangi efekte hangi havuzun ait olduðunu aklýnda tutan akýllý sözlük (Dictionary)
    private Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> _pools = new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();

    private void Awake()
    {
        // Sahnede sadece 1 tane VFXManager olduðundan emin ol
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Efekti oynatacak ana metod
    public void PlayVFX(ParticleSystem prefab, Vector3 position, Quaternion rotation, Vector3 scale, float lifeTime = 2f)
    {
        if (prefab == null) return;

        // Eðer bu efekt türü için (örneðin cam kýrýðý) daha önce havuz açýlmadýysa, hemen yarat
        if (!_pools.ContainsKey(prefab))
        {
            _pools[prefab] = new ObjectPool<ParticleSystem>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: (vfx) => vfx.gameObject.SetActive(true),
                actionOnRelease: (vfx) => vfx.gameObject.SetActive(false),
                actionOnDestroy: (vfx) => Destroy(vfx.gameObject),
                defaultCapacity: 5,
                maxSize: 20
            );
        }

        // Havuzdan 1 tane boþta/uyuyan efekt çek
        ParticleSystem activeVFX = _pools[prefab].Get();

        // Yerini, yönünü ve boyutunu gönderilen ayarlara göre hizala
        activeVFX.transform.position = position;
        activeVFX.transform.rotation = rotation;
        activeVFX.transform.localScale = scale;

        // Efekti patlat
        activeVFX.Play();

        // Belirlenen süre (lifeTime) bitince efekti yok etmek yerine havuza geri yolla
        StartCoroutine(ReturnToPoolAfterDelay(activeVFX, prefab, lifeTime));
    }

    private IEnumerator ReturnToPoolAfterDelay(ParticleSystem vfx, ParticleSystem prefabKey, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Eðer obje hala aktifse onu havuza iade et (dinlenmeye al)
        if (vfx.gameObject.activeInHierarchy)
        {
            _pools[prefabKey].Release(vfx);
        }
    }
}