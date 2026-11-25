using UnityEngine;

//Code provided by: https://www.youtube.com/watch?v=uL9RGgfA-Ck

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public Material skybox;
    public float cycleDuration = 30f;

    [Header("Day")]
    public Color dayZenith = new Color(0.53f, 0.81f, 0.98f);
    public Color dayHorizon = new Color(0.73f, 0.89f, 1f);
    public float dayIntenstity = 1f;

    [Header("Night")]
    public Color nightZenith = new Color(0.01f, 0.01f, 0.07f);
    public Color nightHorizon = new Color(0.01f, 0.01f, 0.05f);
    public float nightIntenstity = 0.05f;

    private float timer;
    private void Update()
    {
        timer = (timer+Time.deltaTime) % cycleDuration;

        float angle = (timer / cycleDuration) * 360f;
        //float t = Mathf.InverseLerp(dayIntenstity, nightIntenstity, t);
        //float t = Mathf.Clamp01(angle / 180f);
        float t = Mathf.PingPong(angle / 180f, 1f);

        sun.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        sun.intensity = Mathf.Lerp(dayIntenstity, nightIntenstity, t);

        if(skybox != null)
        {
            skybox.SetColor("_ZenithColor", Color.Lerp(dayZenith, nightZenith, t));
            skybox.SetColor("_HorizonColor", Color.Lerp(dayHorizon, nightHorizon, t));
            skybox.SetFloat("_AtmosphereThickness", Mathf.Lerp(0.5f, 1f, t));
            skybox.SetFloat("_EnableStars", Mathf.Lerp(0f, 1f, t));
        }
    }
}
