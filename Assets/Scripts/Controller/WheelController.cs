using UnityEngine;

public class WheelController : MonoBehaviour
{
    [Header("Tekerlek Gruplar�")]
    [SerializeField] private Transform[] allWheels;
    [SerializeField] private Transform[] frontWheels;

    [Header("Ayarlar")]
    [SerializeField] private float wheelRadius = 0.4f;
    [SerializeField] private float maxSteerAngle = 30f;

    // YEN�: Direksiyonun sa�a sola d�nerkenki yumu�akl�k/gecikme h�z�
    [SerializeField] private float steerSmoothSpeed = 20f;

    public enum Axis { X, Y, Z }

    [Header("Eksen Ayarlar�")]
    public Axis rollAxis = Axis.Z;
    public Axis steerAxis = Axis.Y;

    [Header("Y�n D�zeltmeleri")]
    public bool tersYuvarlanma = true;
    public bool tersDireksiyon = false;

    private VehicleController _vehicle;
    private Rigidbody _rb;
    private float _rotationAngle;

    // YEN�: Tekerle�in anl�k olarak bulundu�u a��y� haf�zada tutar
    private float _currentSteerAngle;

    private Quaternion[] _baseRotationsAll;

    void Start()
    {
        _vehicle = GetComponentInParent<VehicleController>();
        _rb = _vehicle.GetComponent<Rigidbody>();

        _baseRotationsAll = new Quaternion[allWheels.Length];
        for (int i = 0; i < allWheels.Length; i++)
        {
            _baseRotationsAll[i] = allWheels[i].localRotation;
        }
    }

    void Update()
    {
        if (_vehicle == null) return;

        // 1. HIZ VE YUVARLANMA
        float speed = _vehicle.CurrentSpeed;
        float forwardDot = Vector3.Dot(_vehicle.transform.forward, _rb.linearVelocity);
        if (forwardDot < -0.1f) speed = -speed;

        float rotationStep = (speed * Time.deltaTime) / (2 * Mathf.PI * wheelRadius) * 360f;
        if (tersYuvarlanma) rotationStep = -rotationStep;

        _rotationAngle += rotationStep;

        // 2. D�REKS�YON (YUMU�ATILMI�)
        float steerInput = _vehicle.CurrentSteering;
        float targetSteerAngle = steerInput * maxSteerAngle;
        if (tersDireksiyon) targetSteerAngle = -targetSteerAngle;

        // S�H�RL� KOD: Tekerlek an�nda 'targetSteerAngle' olmak yerine, ona do�ru yumu�ak�a s�z�l�r!
        _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, targetSteerAngle, Time.deltaTime * steerSmoothSpeed);

        Vector3 rollVec = GetAxisVector(rollAxis);
        Vector3 steerVec = GetAxisVector(steerAxis);

        // 3. TEKERLEKLERE UYGULA
        for (int i = 0; i < allWheels.Length; i++)
        {
            Transform wheel = allWheels[i];
            Quaternion baseRot = _baseRotationsAll[i];

            Quaternion rollRot = Quaternion.AngleAxis(_rotationAngle, rollVec);
            Quaternion steerRot = Quaternion.identity;

            for (int j = 0; j < frontWheels.Length; j++)
            {
                if (wheel == frontWheels[j])
                {
                    // Art�k sert olan "targetSteerAngle"� de�il, yumu�at�lm�� olan "_currentSteerAngle"� uyguluyoruz
                    steerRot = Quaternion.AngleAxis(_currentSteerAngle, steerVec);
                    break;
                }
            }

            wheel.localRotation = baseRot * steerRot * rollRot;
        }
    }

    private Vector3 GetAxisVector(Axis axis)
    {
        if (axis == Axis.X) return Vector3.right;
        if (axis == Axis.Y) return Vector3.up;
        return Vector3.forward;
    }
}