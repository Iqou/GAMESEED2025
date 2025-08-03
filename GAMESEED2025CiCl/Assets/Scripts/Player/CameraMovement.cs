using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 9, -5);

    [Range(0.01f, 1f)] public float smoothSpeed = 0.125f;

    // --- Zoom Settings ---
    [Header("Shop Zoom Settings")]
    public float zoomFieldOfView = 30f;
    public float zoomDuration = 0.5f;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    // --- State Tracking ---
    private float originalFieldOfView;
    private bool isZoomed = false;
    private Coroutine zoomCoroutine;

    void Awake()
    {
        cam = GetComponent<Camera>();
        originalFieldOfView = cam.fieldOfView;
    }

    void LateUpdate()
    {
        // Only follow the player if we are not zoomed in on a target
        if (player != null && !isZoomed)
        {
            Vector3 targetPosition = player.transform.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothSpeed);
        }
    }

    // --- Public Methods to Control Zooming ---

    public void ZoomToTarget(Transform target)
    {
        if (isZoomed) return;

        isZoomed = true;

        // The target for the camera is the shop's position plus the original camera offset
        Vector3 zoomTargetPosition = target.position + offset;

        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(DoZoom(zoomTargetPosition, zoomFieldOfView));
    }

    public void ResetZoom()
    {
        if (!isZoomed) return;

        // The target position is the player's current position plus the offset
        Vector3 targetPosition = player.transform.position + offset;

        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(DoZoom(targetPosition, originalFieldOfView, () => 
        {
            isZoomed = false; // Set isZoomed to false only after the zoom-out is complete
        }));
    }

    // --- Coroutine for Smooth Transition ---

    private IEnumerator DoZoom(Vector3 targetPosition, float targetFov, System.Action onComplete = null)
    {
        Vector3 startPosition = transform.position;
        float startFov = cam.fieldOfView;
        float time = 0;

        while (time < zoomDuration)
        {
            time += Time.deltaTime;
            float t = time / zoomDuration;

            // Smoothly interpolate position and field of view
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cam.fieldOfView = Mathf.Lerp(startFov, targetFov, t);

            yield return null;
        }

        // Ensure final values are set
        transform.position = targetPosition;
        cam.fieldOfView = targetFov;

        onComplete?.Invoke();
    }
}
