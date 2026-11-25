using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SpoonTiltPourDetector : MonoBehaviour
{
    public enum SpoonContentType { LiquidStream, SolidToppings  }

    public SpoonContentType contentType = SpoonContentType.LiquidStream;

    public Transform origin = null;
    public GameObject contentPrefab;
    public GameObject liquidVisual;

    public float pourThreshold = 85f;
    public float emptyDuration = 1.2f;
    public string refillTag = "BatterPot";

    public int toppingsPerScoop = 4;
    public float toppingFallSpeed = 1.4f;

    private bool hasContent = true;
    private bool isPouring = false;
    private float currentFill = 1f;
    private SauceStream currentStream = null;
    private Vector3 initialVisualScale;
    private float toppingTimer = 0f;

    private void Start()
    {
        if(liquidVisual != null)
        {
            initialVisualScale = liquidVisual.transform.localScale;
            liquidVisual.SetActive(hasContent);
        }    
    }

    private void Update()
    {
        Debug.Log(Vector3.Angle(origin.up, Vector3.up));
        if (origin == null || contentPrefab == null) return;

        if (!hasContent)
        {
            if (isPouring) StopPour();
            return;
        }

        bool pourCheck = Vector3.Angle(origin.up, Vector3.up) > pourThreshold;

        if (isPouring != pourCheck)
        {
            if (pourCheck) StartPour();
            else StopPour();
        }

        if (isPouring)
        {
            currentFill -= Time.deltaTime / emptyDuration;

            if (contentType == SpoonContentType.SolidToppings)
            {
                toppingTimer -= Time.deltaTime;
                float interval = emptyDuration / Mathf.Max(1, toppingsPerScoop);
                if(toppingTimer <= 0)
                {
                    SpawnSolidTopping();
                    toppingTimer = interval;
                }
            }

            if(liquidVisual != null)
            {
                float scaleFactor = Mathf.Clamp01(currentFill);
                liquidVisual.transform.localScale = new Vector3(
                    initialVisualScale.x * Mathf.Lerp(0.4f, 1f, scaleFactor),
                    initialVisualScale.y * scaleFactor,
                    initialVisualScale.z * Mathf.Lerp(0.4f, 1f, scaleFactor)
                );
            }

            if(currentFill <= 0f)
            {
                hasContent = false;
                if(liquidVisual != null) liquidVisual.SetActive(false);
                StopPour();
            }
        }
    }

    private void StartPour()
    {
        isPouring = true;
        toppingTimer = 0f;

        if(contentType == SpoonContentType.LiquidStream)
        {
            GameObject streamObj = Instantiate(contentPrefab, origin.position, Quaternion.identity, transform);
            currentStream = streamObj.GetComponent<SauceStream>();
            if (currentStream != null) currentStream.Begin();
        }
    }

    private void StopPour()
    {
        isPouring = false;
        if(currentStream != null)
        {
            currentStream.End();
            currentStream = null;
        }
    }

    private void SpawnSolidTopping()
    {
        Vector2 randomCircle = Random.insideUnitCircle * 0.025f;
        Vector3 rayStart = origin.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if(Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2.0f))
        {
            Quaternion finalRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Vector3 finalPos = hit.point + hit.normal * 0.002f;

            GameObject piece = Instantiate(contentPrefab, rayStart, Random.rotation);
            StartCoroutine(FallToSurface(piece.transform, finalPos, finalRot, hit.collider.transform));
        }
    }

    private IEnumerator FallToSurface(Transform piece, Vector3 targetPos, Quaternion targetRot, Transform parentSurface)
    {
        while (piece != null && Vector3.Distance(piece.position, targetPos) > 0.003f)
        {
            piece.position = Vector3.MoveTowards(piece.position, targetPos, toppingFallSpeed * Time.deltaTime);
            piece.Rotate(Vector3.one * (360f * Time.deltaTime));
            yield return null;
        }

        if(piece != null)
        {
            piece.position = targetPos;
            piece.rotation = targetRot;
            piece.SetParent(parentSurface);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(refillTag))
        {
            hasContent = true;
            currentFill = 1f;
            if(liquidVisual != null)
            {
                liquidVisual.transform.localScale = initialVisualScale;
                liquidVisual.SetActive(true);
            }
        }
    }
}
