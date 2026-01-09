using UnityEngine;

public class Camerafix : MonoBehaviour
{
    private float defaultAspect = 9f / 16f;
    private float defaultSize;
    private bool isTablet;

    void Start()
    {
        defaultSize = Camera.main.orthographicSize;

        isTablet = IsTabletDevice();

        if (!isTablet)
        {
            AdjustCamera();
        }
    }

    void AdjustCamera()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        Camera.main.orthographicSize = defaultSize * (defaultAspect / currentAspect);
    }

    bool IsTabletDevice()
    {
        float screenRatio = (float)Screen.height / Screen.width;

        Debug.Log(screenRatio);

        if (screenRatio > 1.3f && screenRatio < 1.45f)
        {
            return true;
        }
        else if (screenRatio > 1.55f && screenRatio < 1.7f)
        {
            return true;
        }

        return false;
    }
}
