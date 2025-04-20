using UnityEngine;
using System.Collections;

public class CameraGyroParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float xMultiplier = 0.05f;
    public float yMultiplier = 0.03f;
    public float smoothTime = 0.1f;
    public Vector2 xClamp = new Vector2(-0.5f, 0.5f);
    public Vector2 yClamp = new Vector2(-0.3f, 0.3f);

    [Header("Gyro Calibration")]
    public bool autoCalibrate = true;
    public float calibrationTime = 1f;
    private Quaternion _gyroOffset;
    private bool _isCalibrated;

    [Header("Mouse Simulation (Editor Only)")]
    public bool enableMouseSimulation = true;
    public float mouseSensitivity = 2f;
    private Vector3 _mouseInput;

    [Header("Animation Blend")]
    [Range(0, 1)] public float gyroInfluence = 0.7f;

    private Vector3 _originalPosition;
    private Vector3 _targetPosition;
    private Vector3 _velocity;
    private Animator _animator;
    private bool _gyroEnabled;

    private void Start()
    {
        _originalPosition = transform.localPosition;
        _animator = GetComponent<Animator>();
        
        // Инициализация гироскопа
        _gyroEnabled = SystemInfo.supportsGyroscope;
        if (_gyroEnabled)
        {
            Input.gyro.enabled = true;
            if (autoCalibrate) StartCoroutine(CalibrateGyro());
        }
        else Debug.LogWarning("Гироскоп не поддерживается");
    }

    private IEnumerator CalibrateGyro()
    {
        yield return new WaitForSeconds(0.5f);
        _gyroOffset = Quaternion.Inverse(Input.gyro.attitude);
        _isCalibrated = true;
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (enableMouseSimulation && !_gyroEnabled)
        {
            SimulateWithMouse();
            ApplyParallaxEffect(_mouseInput);
            return;
        }
#endif

        if (!_gyroEnabled || !_isCalibrated) return;

        // Получаем данные гироскопа
        Quaternion rot = _gyroOffset * Input.gyro.attitude;
        Vector3 angles = rot.eulerAngles;
        
        Vector3 gyroInput = new Vector3(
            angles.x > 180 ? angles.x - 360 : angles.x,
            angles.y > 180 ? angles.y - 360 : angles.y,
            0);

        ApplyParallaxEffect(gyroInput);
    }

    private void ApplyParallaxEffect(Vector3 input)
    {
        // Базовая позиция (из аниматора или оригинальная)
        Vector3 basePosition = (_animator != null && _animator.enabled) 
            ? transform.localPosition 
            : _originalPosition;

        // Целевая позиция с ограничениями
        _targetPosition = basePosition + new Vector3(
            Mathf.Clamp(input.x * xMultiplier, xClamp.x, xClamp.y),
            Mathf.Clamp(input.y * yMultiplier, yClamp.x, yClamp.y),
            0);

        // Смешивание с анимацией
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            Vector3.Lerp(basePosition, _targetPosition, gyroInfluence),
            ref _velocity,
            smoothTime);
    }

#if UNITY_EDITOR
    private void SimulateWithMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        _mouseInput = new Vector3(
            (mousePos.x / Screen.width - 0.5f) * mouseSensitivity,
            (mousePos.y / Screen.height - 0.5f) * mouseSensitivity,
            0);
    }
#endif

    public void ManualCalibrate()
    {
        if (_gyroEnabled)
        {
            _gyroOffset = Quaternion.Inverse(Input.gyro.attitude);
            _isCalibrated = true;
            Debug.Log("Гироскоп откалиброван!");
        }
    }

    private void OnDisable()
    {
        transform.localPosition = _originalPosition;
    }
}