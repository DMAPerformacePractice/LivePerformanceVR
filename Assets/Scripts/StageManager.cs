using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Light stageLight;

    private bool lightsDimming = false;

    public static bool userPerforming = false;

    [SerializeField] private float dimTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartLightsDimming();
        }
    }

    public void StartLightsDimming()
    {
        if (lightsDimming == false)
        {
            StartCoroutine(DimLights());
        }
    }

    private IEnumerator DimLights()
    {
        lightsDimming = true;

        var t = 0;

        while (t < 100) {
            stageLight.intensity = Mathf.Lerp(1, 0.5f, t);

            Debug.Log(stageLight.intensity);

            t += 1;
        }

        Debug.Log("Dimming Lights");

        yield return null;

        Debug.Log("Lights Dimmed");
    }
}
