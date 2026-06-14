using UnityEngine;
using UnityEngine.Splines;

public class SplineTrap : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float speed = 0.2f; // 0-1 arası hız
    [Range(0f, 1f)]
    public float offset;
    [SerializeField] private float turnSpeed = 2f;
    private float progress;

    void Start()
    {
        progress = offset;
    }

    void Update()
    {
        if (splineContainer == null) return;

        progress += speed * Time.deltaTime;
        if (progress > 1f)
            progress -= 1f;

        Vector3 position = splineContainer.EvaluatePosition(progress);
        Vector3 tangent = splineContainer.EvaluateTangent(progress);

        transform.position = position;

        Quaternion splineRotation = Quaternion.LookRotation(tangent);

        // Sürekli Y dönüşü
        Quaternion spinRotation = Quaternion.Euler(0f, Time.time * turnSpeed, 0f);

        transform.rotation = splineRotation * spinRotation;
    }
}