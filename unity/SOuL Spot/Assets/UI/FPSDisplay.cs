using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour {
    // From: https://www.youtube.com/watch?v=xOCScMQIxrU
    public TextMeshProUGUI FpsText;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    void Update() {
        time += Time.deltaTime;

        frameCount += 1;

        if (time >= pollingTime) {
            float frameRate = frameCount / time;
            FpsText.text = frameRate.ToString("f3") + " fps";

            time -= pollingTime;
            frameCount = 0;
        }    
    }
}
