using UnityEngine;

public class TriggerPourDetector : MonoBehaviour
{
    public Transform origin = null;
    public GameObject streamPrefab;

    private bool isPouring = false;
    private SauceStream currentStream = null;


    public void StartPour()
    {
        if(!isPouring && origin != null && streamPrefab != null)
        {
            isPouring = true;
            currentStream = CreateStream();
            currentStream.Begin();
        }
    }

    public void StopPour()
    {
        if (isPouring)
        {
            isPouring = false;
            if(currentStream != null)
            {
                currentStream.End();
                currentStream = null;
            }
        }
    }

    private SauceStream CreateStream()
    {
        GameObject streamObject = Instantiate(streamPrefab, origin.position, Quaternion.identity, transform);
        return streamObject.GetComponent<SauceStream>();
    }
}
