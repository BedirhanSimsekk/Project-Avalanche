using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Engine Settings")]
    [SerializeField] private float baseSpeed = 15f;
    [SerializeField] private float turnSpeed = 200f;
    [SerializeField] private float turboMultiplier = 2f;
    [SerializeField] private float accelerationRate = 5f;

    [Header("Physics & Stability")]
    [SerializeField] private Transform centerOfMass;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private bool _isTurboActive;

    // Kaps�lleme: D��ar�dan sadece okunabilir. SnowballController bu veriyi �ekecek.
    public float CurrentSpeed => _rb.linearVelocity.magnitude;
    public float CurrentSteering => _moveInput.x;
    public bool IsTurboActive => _isTurboActive;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Arac�n devrilmesini �nlemek i�in a��rl�k merkezini a�a�� �ekiyoruz
        if (centerOfMass != null)
        {
            _rb.automaticCenterOfMass = false; // Unity 6 i�in manuel atamay� zorla
            _rb.centerOfMass = centerOfMass.localPosition;
        }
    }

    // New Input System Event'leri
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnTurbo(InputAction.CallbackContext context)
    {
        if (context.started) _isTurboActive = true;
        if (context.canceled) _isTurboActive = false;
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
    }

    private void HandleMotor()
    {
        float currentSpeedLimit = baseSpeed * (_isTurboActive ? turboMultiplier : 1f);

        // Sadece ileri/geri ekseninde hedef h�z hesaplama
        Vector3 targetVelocity = transform.forward * (_moveInput.y * currentSpeedLimit);

        // Yer�ekimi (Y ekseni) h�z�n� koru ki ara� havada as�l� kalmas�n
        targetVelocity.y = _rb.linearVelocity.y;

        // Arcade s�r�� hissi i�in h�z� yumu�ak�a uygula
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * accelerationRate);
    }

    private void HandleSteering()
    {
        // Ara� sadece hareket halindeyken d�nebilir (Fizik ger�ek�ili�i)
        if (Mathf.Abs(_moveInput.y) > 0.1f)
        {
            // Geri giderken d�n�� y�n�n� tersine �evir
            float turnMultiplier = _moveInput.y > 0 ? 1f : -1f;
            float turn = _moveInput.x * turnSpeed * turnMultiplier * Time.fixedDeltaTime;

            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            _rb.MoveRotation(_rb.rotation * turnRotation);
        }
    }
}