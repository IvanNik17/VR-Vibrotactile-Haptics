using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class haptic : MonoBehaviour
{
    public SteamVR_Action_Vibration vib;
    public SteamVR_Input_Sources thisHand;

    public float secondsFromNow;
    public float durationSeconds = 2f;
    [Range(0, 320f)]
    public float frequency = 320f;
    [Range(0, 1)]
    public float amplitude = 1f;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {

            //vib.Execute(secondsFromNow, durationSeconds, frequency, amplitude, thisHand);
            StartCoroutine(Vibrate());
        }

    }
     IEnumerator Vibrate() {

        while(true) {
            vib.Execute(secondsFromNow, durationSeconds, frequency, amplitude, thisHand);
            yield return new WaitForSeconds(durationSeconds);
        }
        
    }
}
