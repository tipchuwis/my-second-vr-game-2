using System.Collections;
using UnityEngine;

public class ShakeToppingSpawner : MonoBehaviour
{
    public Transform origin;
    public GameObject sprinklePrefab;

    public float minTiltAngle = 95f;
    public float minShakeDistance = 0.015f;
    public float spawnCooldown = 0.1f;
    public float spreadRadius = 0.035f;

    public float fallSpeed = 1.4f;

    private Vector3 lastPosition;
    private float cooldownTimer = 0f;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (origin == null || sprinklePrefab == null) return;

        cooldownTimer -= Time.deltaTime;

        float movementDelta = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        bool isTilted = Vector3.Angle(origin.up, Vector3.up) > minTiltAngle;
        bool isShaking = movementDelta > minShakeDistance;

        if(isTilted && isShaking && cooldownTimer <= 0f)
        {
            SpawnSprinkleOnSurface();
            cooldownTimer = spawnCooldown;
        }

    }

    private void SpawnSprinkleOnSurface()
    {

        Vector2 randomCircle = Random.insideUnitCircle * spreadRadius;
        Vector3 rayStart = origin.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if(Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2.0f))
        {
            Quaternion finalRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Vector3 finalPos = hit.point + hit.normal * 0.002f;

            GameObject sprinkle = Instantiate(sprinklePrefab, rayStart, Random.rotation);
            StartCoroutine(FallToSurface(sprinkle.transform, finalPos, finalRot, hit.collider.transform));
        }
    }

    private IEnumerator FallToSurface(Transform sprinkle, Vector3 targetPos, Quaternion targetRot, Transform parentSurface)
    {
        while (sprinkle != null && Vector3.Distance(sprinkle.position, targetPos) > 0.003f)
        {
            sprinkle.position = Vector3.MoveTowards(sprinkle.position, targetPos, fallSpeed * Time.deltaTime);
            sprinkle.Rotate(Vector3.one * (360f * Time.deltaTime));
            yield return null;
        }
        if(sprinkle != null)
        {
            sprinkle.position = targetPos;
            sprinkle.rotation = targetRot;
            sprinkle.SetParent(parentSurface);
        }
    }
}
