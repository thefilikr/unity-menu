using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Layouts;

[RequireComponent(typeof(Camera))]
public class GyroParallax : MonoBehaviour
{
    [Header("Основные настройки")]
    [Range(0.01f, 1f)] public float sensitivity = 0.15f;
    [Range(0.01f, 10.0f)] public float smoothTime = 0.1f;
    public Vector2 maxOffset = new Vector2(1.5f, 1f);

    [Header("Слои паралакса")]
    public Transform[] parallaxLayers;
    public float[] layerMultipliers = { 0.3f, 0.7f, 1.2f };

    [Header("Дополнительные настройки")]
    public bool autoCalibrate = true;
    public bool invertX = false;
    public bool invertY = true;

    private Vector3[] initialLayerPositions;
    private Vector3[] currentLayerOffsets;
    private Vector3 velocity;
    private Quaternion baseAttitude;
    private bool sensorsReady = false;
    private Vector3 lastTargetPosition;

    void Start()
    {
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            sensorsReady = true;
            if (autoCalibrate) CalibrateGyro();
        }
        else
        {
            Debug.LogWarning("AttitudeSensor не поддерживается на этом устройстве.");
        }

        if (parallaxLayers != null)
        {
            initialLayerPositions = new Vector3[parallaxLayers.Length];
            currentLayerOffsets = new Vector3[parallaxLayers.Length];
            
            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                if (parallaxLayers[i] != null)
                {
                    initialLayerPositions[i] = parallaxLayers[i].localPosition;
                    currentLayerOffsets[i] = Vector3.zero;
                }
            }
        }
    }

    void Update()
    {
        Vector3 movement = CalculateMovement();
        Vector3 targetPosition = ApplyLimits(movement);
        lastTargetPosition = targetPosition;

        ApplyParallaxEffect(targetPosition);
    }

    private Vector3 CalculateMovement()
    {
        if (sensorsReady && AttitudeSensor.current != null)
        {
            Quaternion gyroAttitude = AttitudeSensor.current.attitude.ReadValue();
            Quaternion relativeRotation = Quaternion.Inverse(baseAttitude) * gyroAttitude;
            Vector3 euler = relativeRotation.eulerAngles;

            euler.x = NormalizeAngle(euler.x);
            euler.y = NormalizeAngle(euler.y);

            float x = euler.y * sensitivity * (invertX ? -1 : 1);
            float y = euler.x * sensitivity * (invertY ? -1 : 1);

            return new Vector3(x, y, 0);
        }

#if UNITY_EDITOR
        if (!Application.isMobilePlatform)
        {
            float mouseX = Mouse.current.delta.x.ReadValue() * sensitivity * 0.01f;
            float mouseY = Mouse.current.delta.y.ReadValue() * sensitivity * 0.01f;
            return new Vector3(
                mouseX * (invertX ? -1 : 1),
                mouseY * (invertY ? -1 : 1),
                0);
        }
#endif
        return Vector3.zero;
    }

    private Vector3 ApplyLimits(Vector3 movement)
    {
        movement.x = Mathf.Clamp(movement.x, -maxOffset.x, maxOffset.x);
        movement.y = Mathf.Clamp(movement.y, -maxOffset.y, maxOffset.y);
        return movement;
    }

    private void ApplyParallaxEffect(Vector3 movement)
    {
        if (parallaxLayers == null || layerMultipliers == null || initialLayerPositions == null)
            return;

        int layers = Mathf.Min(parallaxLayers.Length, layerMultipliers.Length, initialLayerPositions.Length);

        for (int i = 0; i < layers; i++)
        {
            if (parallaxLayers[i] != null)
            {
                // Обновляем текущее смещение с учетом множителя слоя
                currentLayerOffsets[i] = movement * layerMultipliers[i];
                
                // Применяем смещение к начальной позиции
                parallaxLayers[i].localPosition = initialLayerPositions[i] + currentLayerOffsets[i];
            }
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    public void CalibrateGyro()
    {
        if (AttitudeSensor.current != null)
        {
            baseAttitude = AttitudeSensor.current.attitude.ReadValue();
            Debug.Log("Гироскоп откалиброван (Input System)");
        }
    }

    private void OnGUI()
    {
        GUIStyle overlayStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Screen.width / 50,
            normal = { textColor = Color.white },
            padding = new RectOffset(10, 10, 10, 10)
        };

        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        bgTex.Apply();
        GUI.DrawTexture(new Rect(10, 10, 500, 250), bgTex);

        GUILayout.BeginArea(new Rect(20, 20, 480, 230));
        
        GUILayout.Label($"🧭 Гироскоп: {(AttitudeSensor.current != null ? "Доступен" : "Нет")}", overlayStyle);
        GUILayout.Label($"🧭 Sensors Ready: {(sensorsReady)}", overlayStyle);

        if (AttitudeSensor.current != null)
        {
            Quaternion attitude = AttitudeSensor.current.attitude.ReadValue();
            GUILayout.Label($"Attitude: {attitude.eulerAngles}", overlayStyle);
            GUILayout.Label($"Base Attitude: {baseAttitude.eulerAngles}", overlayStyle);
        }

        GUILayout.Label($"📦 Смещение: {lastTargetPosition}", overlayStyle);

        if (parallaxLayers != null)
        {
            for (int i = 0; i < Mathf.Min(parallaxLayers.Length, 3); i++)
            {
                if (parallaxLayers[i] != null)
                    GUILayout.Label($"Слой {i}: {parallaxLayers[i].localPosition}", overlayStyle);
            }
        }

        GUILayout.EndArea();
    }
}