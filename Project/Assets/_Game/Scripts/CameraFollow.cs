using UnityEngine;

// La cámara sigue al jugador en horizontal, sin salirse de los bordes del nivel.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 100f;

    // LateUpdate se ejecuta después de que el jugador se movió
    private void LateUpdate()
    {
        if (target == null) return;

        float x = Mathf.Clamp(target.position.x, minX, maxX);
        Vector3 goal = new Vector3(x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, smoothSpeed * Time.deltaTime);
    }
}
