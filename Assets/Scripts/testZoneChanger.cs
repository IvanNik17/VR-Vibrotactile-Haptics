using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testZoneChanger : MonoBehaviour
{
    public GameObject testZone;
    public GameObject startZone;
    // Start is called before the first frame update
    void Start()
    {
        testZone.SetActive(false);
        startZone.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(startZone.activeSelf == false)
            {
                startZone.SetActive(true);
                testZone.SetActive(false);
            }
            else
            {
                startZone.SetActive(false);
                testZone.SetActive(true);

                float tempRandom = Random.Range(0,4)* 90;
                testZone.transform.eulerAngles = new Vector3(0f, tempRandom, 0f);
            }
        }
    }
}
