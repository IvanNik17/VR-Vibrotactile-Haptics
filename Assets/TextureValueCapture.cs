using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;

public class TextureValueCapture : MonoBehaviour
{
    private Texture2D mTexture = null;
    private Vector3 lastPosition;

    private RaycastHit oldHit;

    float currValueOnFrame;

    public List<float> currValueOnFrame_list;

    GameObject currObj;

    Dictionary<GameObject, Vector2> tagedObj = new Dictionary<GameObject, Vector2>();

    Dictionary<GameObject, RaycastHit> savedHits = new Dictionary<GameObject, RaycastHit>();

    public SteamVR_Input_Sources thisHand;


    public SteamVR_Action_Vibration vib;

    public bool debug = false;

    public GameObject raycasterObj;

    public int numLineChecks = 10;
    public float maxVelocity = 2.0f;
    public int rayFilterSize = 1;

    public bool useBeginBumps = true;
    public float highTresh = 0.8f;
    public float lowTresh = 0.2f;

    public float midTresh = 0.5f;

    public float durationHigh = 0.075f;
    [Range(10, 350)]
    public float freqencyHigh = 20f;

    [Range(0, 1)]
    public float amplitudeHigh = 0.2f;
    public float durationLow = 0.015f;
    [Range(10, 350)]
    public float freqencyLow = 320f;
    [Range(0, 1)]
    public float amplitudeLow = 0.9f;

    public float durationNon = 0.015f;
    [Range(10, 350)]
    public float freqencyNon = 320f;
    [Range(0, 1)]
    public float amplitudeNon = 0.9f;

    private int currTresh = 0;

    public struct savedInfoPackage
    {
        public GameObject obj;
        public List<float> valueList;
        public float distanceMovedSinceUpdate;


    }

    public void AddNewModel(GameObject obj)
    {
        if (!tagedObj.ContainsKey(obj))
        {
            tagedObj.Add(obj, GetMinMaxTextureValues(obj));
        }
    }

    public Vector2 GetMinMaxTextureValues(GameObject GObj)
    {
        Vector2 minMaxValue = new Vector2(99999f, 0f);
        mTexture = GObj.GetComponent<Renderer>().material.GetTexture("_ParallaxMap") as Texture2D;
        Color colorData = Color.black;

        for (int i = 0; i < mTexture.width; i++)
        {
            for (int j = 0; j < mTexture.height; j++)
            {
                colorData = mTexture.GetPixel(i, j);
                float intensityVal = (colorData.r + colorData.g + colorData.b) / 3;

                if (intensityVal > minMaxValue[1])
                {
                    minMaxValue[1] = intensityVal;
                }

                if (intensityVal < minMaxValue[0])
                {
                    minMaxValue[0] = intensityVal;
                }
            }
        }
        return minMaxValue;
    }

    public savedInfoPackage MakePackage()
    {
        float tempDistance = Vector3.Distance(raycasterObj.transform.position, lastPosition) / Time.deltaTime;
        lastPosition = raycasterObj.transform.position;
        Ray ray = new Ray(raycasterObj.transform.position, raycasterObj.transform.forward);
        RaycastHit hit;
        List<float> currValue = new List<float>();
        float dis = 10000.1f;
        GameObject tempObj = null;
        Debug.DrawRay(raycasterObj.transform.position, raycasterObj.transform.forward, Color.red, Time.deltaTime);


        foreach (KeyValuePair<GameObject, Vector2> entry in tagedObj)
        {
            if (entry.Key.GetComponent<Collider>().Raycast(ray, out hit, 10000.0f) && hit.distance <= dis)
            {
                if (savedHits[entry.Key].transform != null)
                {
                    Texture2D tempTexture = hit.transform.gameObject.GetComponent<Renderer>().material.GetTexture("_ParallaxMap") as Texture2D;
                    dis = hit.distance;
                    tempObj = hit.transform.gameObject;
                    int hitX = (int)(hit.textureCoord.x * tempTexture.width);
                    int hitY = (int)(hit.textureCoord.y * tempTexture.height);

                    int oldhitX = (int)(savedHits[entry.Key].textureCoord.x * tempTexture.width);
                    int oldhitY = (int)(savedHits[entry.Key].textureCoord.y * tempTexture.height);

                    float deltaXDistance = hitX - oldhitX;
                    float deltaYDistance = hitY - oldhitY;
                    Vector2 tempVector = tagedObj[hit.transform.gameObject];

                    for (int k = 0; k < numLineChecks; k++)
                    {
                        float currValue_temp = 0f;
                        for (int i = 0; i < rayFilterSize; i++)
                        {
                            for (int j = 0; j < rayFilterSize; j++)
                            {
                                Color colorData = tempTexture.GetPixelBilinear((oldhitX + (deltaXDistance * ((k + 1) / (float)numLineChecks)) + i - 1) /
                                (float)tempTexture.width, (oldhitY + (deltaYDistance * ((k + 1) / (float)numLineChecks)) + j - 1) / (float)tempTexture.height);
                                float tempValue = (colorData.r + colorData.g + colorData.b) / 3;
                                currValue_temp = (tempValue - tempVector[0]) / (tempVector[1] - tempVector[0]);
                            }
                        }
                        currValue_temp /= rayFilterSize;
                        currValue.Add(currValue_temp);
                    }
                }
            }
            savedHits[entry.Key] = hit;
        }







        return new savedInfoPackage
        {
            obj = tempObj,
            valueList = currValue,
            distanceMovedSinceUpdate = (tempDistance < maxVelocity) ? tempDistance : maxVelocity
        };
    }

    public void maxDistInList(List<float> inputsList, out float outMinDist, out float outMaxDist)
    {
        outMaxDist = 0;
        outMinDist = 9;

        for (int i = 0; i < inputsList.Count; i++)
        {
            for (int j = 0; j < inputsList.Count; j++)
            {
                float tempDist = Mathf.Abs(inputsList[i] - inputsList[j]);
                if (tempDist > outMaxDist)
                {
                    outMaxDist = tempDist;
                }
                if (tempDist < outMinDist)
                {
                    outMinDist = tempDist;
                }

            }
        }



    }


    void Start()
    {


        GameObject[] foundObj = FindObjectsOfType<GameObject>();

        lastPosition = transform.position;

        foreach (GameObject x in foundObj)
        {
            if (x.tag == "feedback")
            {
                tagedObj.Add(x, GetMinMaxTextureValues(x));
                savedHits.Add(x, new RaycastHit());
                if (debug) Debug.Log("Adding " + x.name + " to the dictionary");
            }
        }
    }

    void Update()
    {


        savedInfoPackage currentSave = MakePackage();




        currValueOnFrame_list = currentSave.valueList;
        currObj = currentSave.obj;

        float minDist;
        float maxDist;
        maxDistInList(currValueOnFrame_list, out minDist, out maxDist);

        //List<float> allDistances = new List<float>();
        // float maxDist = maxDistInList(currValueOnFrame_list, out allDistances);

        currValueOnFrame = maxDist > Mathf.Abs(minDist) ? maxDist : minDist;
        int count = 0;
        foreach (var x in currValueOnFrame_list)
        {

            if (Mathf.Abs(Mathf.Abs(x) - Mathf.Abs((currValueOnFrame))) < 0.00000000001f)
            {
                count++;
            }
        }
/*
        int tempNum = currTresh;
        if (currObj != null && SteamVR_Input._default.inActions.TouchPad.GetState(thisHand))
        {
            if (useBeginBumps)
            {
                if (currTresh == 0)
                {
                    if (currValueOnFrame >= highTresh)
                    {
                        if (debug) Debug.Log("High");
                        vib.Execute(0, durationHigh, freqencyHigh, amplitudeHigh, thisHand);
                        currTresh = 1;
                    }
                    else if (currValueOnFrame <= lowTresh)
                    {
                        if (debug) Debug.Log("Low");
                        vib.Execute(0, durationLow, freqencyLow, amplitudeLow, thisHand);
                        currTresh = -1;
                    }
                }
                else if (currTresh == 1)
                {
                    if (currValueOnFrame <= lowTresh)
                    {
                        if (debug) Debug.Log("Low");
                        vib.Execute(0, durationLow, freqencyLow, amplitudeLow, thisHand);
                        currTresh = -1;
                    }
                    else if (currValueOnFrame <= midTresh)
                    {
                        currTresh = 0;
                    }
                }
                else if (currTresh == -1)
                {
                    if (currValueOnFrame >= highTresh)
                    {
                        if (debug) Debug.Log("High");
                        vib.Execute(0, durationHigh, freqencyHigh, amplitudeHigh, thisHand);
                        currTresh = 1;
                    }
                    else if (currValueOnFrame >= midTresh)
                    {
                        currTresh = 0;
                    }
                }
            }
            //Debug.Log(currentSave.distanceMovedSinceUpdate);
            /*
            if (tempNum == currTresh && currentSave.distanceMovedSinceUpdate >= 0.1f)
            {
                float tempVal = -99;
                float totalVal = 0;
                foreach (float x in currValueOnFrame_list)
                {
                    if (tempVal == -99)
                    {
                        tempVal = x;
                    }
                    else
                    {
                        //if (debug) Debug.Log((x - tempVal));
                        totalVal += (x - tempVal);
                        tempVal = x;
                    }

                }
                vib.Execute(0, durationNon, freqencyNon, totalVal / (currValueOnFrame_list.Count - 1), thisHand);

                if (debug) Debug.Log(totalVal / currValueOnFrame_list.Count + "            " + totalVal);
            }
             *





            //if (debug) Debug.Log("CurrTresh: " + currTresh);
        }
*/



    }
}