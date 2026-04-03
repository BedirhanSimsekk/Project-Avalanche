using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // Saniyede kaç derece dönecek

    [Header("Float Settings")]
    public float floatSpeed = 2f;      // Dalgalanma hýzý
    public float floatAmplitude = 0.3f; // Ne kadar yükseðe çýkýp ineceði

    private Vector3 _startPosition;

    void Start()
    {
        // Oyun baþladýðý an, objenin haritada konulduðu o ilk yeri hafýzaya al
        _startPosition = transform.position;
    }

    void Update()
    {
        // 1. Kendi etrafýnda fýrýl fýrýl dön (Y ekseninde)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // 2. Olduðu yerde yukarý aþaðý süzül (Sinüs dalgasý ile)
        float newY = _startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // Yeni pozisyonu uygula (X ve Z sabit kalýyor, sadece Y deðiþiyor)
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}