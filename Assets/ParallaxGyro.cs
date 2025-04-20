using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class ParallaxGyro : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float xMultiplier = 0.1f;
    public float yMultiplier = 0.1f;
    public Vector2 xClamp = new Vector2(-1f, 1f);
    public Vector2 yClamp = new Vector2(-1f, 1f);
    public float smoothTime = 0.1f;
    
    [Header("Orientation Settings")]
    public ScreenOrientation gameOrientation = ScreenOrientation.LandscapeLeft;
    public bool invertX = false;
    public bool invertY = false;
    
    [Header("Calibration")]
    public bool autoCalibrate = true;
    public float calibrationTime = 1f;
    
    [Header("Editor Simulation")]
    public bool enableMouseSimulation = true;
    public float mouseSensitivity = 1f;

    private Vector3 _originalPosition;
    private Quaternion _gyroOffset;
    private bool _isCalibrated;
    private Vector3 _velocity;
    
    private void Start()
    {
        _originalPosition = transform.localPosition;
        Screen.orientation = gameOrientation;
        
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            
            if (autoCalibrate)
                StartCoroutine(CalibrateGyro());
        }
        else
        {
            Debug.LogWarning("Gyroscope not available");
            _isCalibrated = true; // Allow mouse simulation
        }
    }

    private IEnumerator CalibrateGyro()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Get initial rotation and adjust for landscape
        _gyroOffset = Quaternion.Inverse(GetAdjustedGyroRotation());
        _isCalibrated = true;
    }

    private Quaternion GetAdjustedGyroRotation()
    {
        Quaternion rotation = Input.gyro.attitude;
        
        // Adjust rotation based on screen orientation
        switch (gameOrientation)
        {
            case ScreenOrientation.LandscapeLeft:
                return new Quaternion(-rotation.y, rotation.x, -rotation.z, rotation.w);
            
            case ScreenOrientation.LandscapeRight:
                return new Quaternion(rotation.y, -rotation.x, -rotation.z, rotation.w);
            
            case ScreenOrientation.Portrait:
            default:
                return new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        }
    }

    private void Update()
    {
        if (!_isCalibrated) return;

#if UNITY_EDITOR
        if (enableMouseSimulation)
        {
            SimulateWithMouse();
            return;
        }
#endif

        if (!Input.gyro.enabled) return;

        Quaternion rot = _gyroOffset * GetAdjustedGyroRotation();
        Vector3 angles = rot.eulerAngles;
        
        float x = angles.x > 180 ? angles.x - 360 : angles.x;
        float y = angles.y > 180 ? angles.y - 360 : angles.y;
        
        ApplyMovement(
            invertX ? -x : x,
            invertY ? -y : y
        );
    }

    private void ApplyMovement(float xInput, float yInput)
    {
        Vector3 target = _originalPosition;
        target.x += Mathf.Clamp(xInput * xMultiplier, xClamp.x, xClamp.y);
        target.y += Mathf.Clamp(yInput * yMultiplier, yClamp.x, yClamp.y);
        
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            target,
            ref _velocity,
            smoothTime
        );
    }

#if UNITY_EDITOR
    private void SimulateWithMouse()
    {
        Vector2 normalizedMousePos = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height);
        
        Vector2 centeredMousePos = normalizedMousePos - new Vector2(0.5f, 0.5f);
        
        ApplyMovement(
            centeredMousePos.x * mouseSensitivity * 100f,
            centeredMousePos.y * mouseSensitivity * 100f
        );
    }
#endif

    public void Recalibrate()
    {
        if (Input.gyro.enabled)
        {
            _gyroOffset = Quaternion.Inverse(GetAdjustedGyroRotation());
        }
    }
}