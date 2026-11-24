using System.Collections;
using UnityEngine;

public class SauceStream : MonoBehaviour
{

    public float pourSpeed = 0.8f;

    public GameObject stainPrefab;
    public float stainInterval = 0.12f;

    private LineRenderer lineRenderer = null;
    private ParticleSystem splashParticle = null;
    private Vector3 targetPosition = Vector3.zero;
    private Coroutine pourRoutine = null;
    private float stainTimer = 0f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticle = GetComponentInChildren<ParticleSystem>();
    }
    private void Start()
    {
        MoveToPosition(0, transform.position);
        MoveToPosition(1, transform.position);
    }

    public void Begin()
    {


        if(splashParticle != null )
            StartCoroutine(UpdateParticle());

        pourRoutine = StartCoroutine(BeginPour());
    }

    private IEnumerator BeginPour()
    {
        while (gameObject.activeSelf)
        {
            targetPosition = FindEndPoint();

            MoveToPosition(0, transform.position);
            AnimateToPosition(1, targetPosition, pourSpeed);

            yield return null;
        }
    }

    public void End()
    {
        if(pourRoutine != null)
            StopCoroutine(pourRoutine);

        pourRoutine = StartCoroutine(EndPour());
    }

    private IEnumerator EndPour()
    {
        while(!HasReachedPosition(0, targetPosition))
        {
            AnimateToPosition(0, targetPosition, pourSpeed * 1.25f);
            AnimateToPosition(1, targetPosition, pourSpeed);

            yield return null;
        }

        Destroy(gameObject);
    }

    private Vector3 FindEndPoint()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(ray, out RaycastHit hit, 2.0f))
        {
            if(HasReachedPosition(1, hit.point) && stainPrefab != null) 
            { 
                stainTimer -= Time.deltaTime;
                if( stainTimer <= 0f)
                {
                    SpawnStain(hit);
                    stainTimer = stainInterval;
                }
            }
            return hit.point;
        }

        return ray.GetPoint(2.0f);
    }

    private void SpawnStain(RaycastHit hit)
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
        GameObject stain = Instantiate(stainPrefab, hit.point + hit.normal * 0.002f, rot);
        stain.transform.SetParent(hit.collider.transform);
    }

    private void MoveToPosition(int index, Vector3 pos)
    {
        lineRenderer.SetPosition(index, pos);
    }

    private void AnimateToPosition(int index, Vector3 pos, float speed)
    {
        Vector3 currentPoint = lineRenderer.GetPosition(index);
        Vector3 newPosition = Vector3.MoveTowards(currentPoint, pos, Time.deltaTime * speed);
        lineRenderer.SetPosition(index, newPosition);
    }

    private bool HasReachedPosition(int index, Vector3 pos)
    {
        return Vector3.Distance(lineRenderer.GetPosition(index), pos) < 0.005f;
    }

    private IEnumerator UpdateParticle()
    {
        while (gameObject.activeSelf)
        {
            splashParticle.transform.position = targetPosition;
            bool isHitting = HasReachedPosition(1, targetPosition);
            splashParticle.gameObject.SetActive(isHitting);

            yield return null;
        }
    }

}
