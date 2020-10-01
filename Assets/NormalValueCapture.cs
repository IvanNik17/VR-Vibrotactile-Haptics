using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class NormalValueCapture : MonoBehaviour {

    private float angle;
    public int anountOfRays = 10;
    int lengthOfLineRenderer = 2;
    public GameObject penModel;
    bool tempOldRayHit;
    InfoPackage oldInfoPackage, newInfoPackage;
    public SteamVR_Input_Sources thisHand;

    public SteamVR_Action_Vibration vib;

    public SteamVR_Action_Boolean touch;

    public bool istoutching = false;

    [Range (10, 350)]
    public float freqencyHigh = 20f;

    [Range (0, 1)]
    public float amplitudeHigh = 0.2f;
    public float durationHigh = 0.075f;
    [Range (10, 350)]
    public float freqencyLow = 10;

    [Range (0, 1)]
    public float amplitudeLow = 0.1f;
    public float durationLow = 0.025f;

    public float minMovement = 0.002f;

    public float multiplyer = 1f;

    Mesh penMesh;

    Vector3[] meshVerticesWC;
    Vector3[] meshNormals;

    public struct InfoPackage {
        public Vector3 pos;
        public Vector3 rayHitPoint;
        public float rayHitDis;
        public Vector3 hitNormal;
    }

    void Start () {
        touch.AddOnStateDownListener (TouchDown, thisHand);
        touch.AddOnStateUpListener (TouchUp, thisHand);

        penMesh = penModel.GetComponent<MeshFilter> ().mesh;
        penMesh.RecalculateNormals ();
        meshVerticesWC = penMesh.vertices;
        meshNormals = penMesh.normals;

        /*
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = lengthOfLineRenderer;
         */

    }

    public void TouchUp (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        istoutching = false;
    }
    public void TouchDown (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        istoutching = true;
    }

    void Update () {
        /* 
                LineRenderer lineRenderer = GetComponent<LineRenderer>();
                Vector3[] points = new Vector3[lengthOfLineRenderer];
                float t = Time.time;
                for (int i = 0; i < lengthOfLineRenderer; i++)
                {
                    points[i] = new Vector3(penModel.transform.position.x + (i * 0.5f) / lengthOfLineRenderer, penModel.transform.position.y, penModel.transform.position.z);
                }
                lineRenderer.SetPositions(points);
        */
        RaycastHit hit = new RaycastHit ();
        bool hitSomething = false;
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        //Debug.DrawRay(penModel.transform.position, penModel.transform.up * 30, Color.red, Time.deltaTime);
        //RaycastHit[] allrayHits;
        //allrayHits = Physics.SphereCastAll(penModel.transform.position, 0.01f, penModel.transform.up, 0.101f, layerMask);
        //Debug.Log(allrayHits.Length);

        Debug.DrawRay (penModel.transform.position, penModel.transform.up * 0.101f, Color.black);
        if (Physics.Raycast (penModel.transform.position, penModel.transform.up, out hit, 0.101f, layerMask)) {

            hitSomething = true;
        } else {
            for (int i = 0; i < meshVerticesWC.Length; i++) {
                Debug.DrawRay (penModel.transform.TransformPoint (meshVerticesWC[i]), penModel.transform.TransformDirection (meshNormals[i]) * 0.01f, Color.red);
                if (Physics.Raycast (penModel.transform.TransformPoint (meshVerticesWC[i]), penModel.transform.TransformDirection (meshNormals[i]), out hit, 0.01f, layerMask)) {
                    hitSomething = true;
                    break;

                }

            }
        }

        if (tempOldRayHit == false) {

            //if (Physics.Raycast(penModel.transform.position, penModel.transform.up, out hit, 0.101f, layerMask))
            if (hitSomething) {
                Debug.Log ("Did Hit");
                
                    oldInfoPackage = new InfoPackage { pos = penModel.transform.position, rayHitPoint = hit.point, rayHitDis = hit.distance, hitNormal = hit.normal };
                    tempOldRayHit = true;
            }
        } else {

            //if (Physics.Raycast(penModel.transform.position, penModel.transform.up, out hit, 0.101f, layerMask))
            if (hitSomething) {
                Debug.Log (hit.transform.parent.name);
                newInfoPackage = new InfoPackage { pos = penModel.transform.position, rayHitPoint = hit.point, rayHitDis = hit.distance, hitNormal = hit.normal };

                float fromOldtoNewDis = Vector3.Distance (oldInfoPackage.rayHitPoint, newInfoPackage.rayHitPoint);
                float fromOldOrigintoOldHit = oldInfoPackage.rayHitDis;
                float fromOldOrigintoNewHit = Vector3.Distance (oldInfoPackage.pos, newInfoPackage.rayHitPoint);
                angle = Mathf.Acos ((fromOldOrigintoNewHit * fromOldOrigintoNewHit + fromOldOrigintoOldHit * fromOldOrigintoOldHit - fromOldtoNewDis * fromOldtoNewDis) / (2.0f * fromOldOrigintoNewHit * fromOldOrigintoOldHit)) * Mathf.Rad2Deg;
                for (int i = 0; i < anountOfRays; i++) {
                    //for use later for faning out the ray from new ray pos towards old ray hit pos
                }

                float angleXNormals = Mathf.Max (0, Vector3.Dot (newInfoPackage.hitNormal, oldInfoPackage.hitNormal));
                //Debug.Log(fromOldtoNewDis);
                if (angleXNormals <= 0.9f && istoutching && fromOldtoNewDis >= minMovement) {
                    amplitudeHigh = Mathf.Max (minMovement, ((minMovement / fromOldtoNewDis)));
                    vib.Execute (0, durationHigh, freqencyHigh, amplitudeHigh, thisHand);
                    Debug.DrawRay (oldInfoPackage.rayHitPoint, oldInfoPackage.hitNormal, Color.blue, 0.5f);
                    Debug.DrawRay (newInfoPackage.rayHitPoint, newInfoPackage.hitNormal, Color.green, 0.5f);
                } else if (istoutching && fromOldtoNewDis >= minMovement) {
                    amplitudeLow = (minMovement / fromOldtoNewDis) * ((1f-angleXNormals)* multiplyer);
                    //Debug.Log(((minMovement/fromOldtoNewDis))*0.1f);
                    vib.Execute (0, Time.deltaTime * 0.9f, freqencyLow, amplitudeLow, thisHand);
                }
                Debug.DrawRay (penModel.transform.position, penModel.transform.up * hit.distance, Color.red, Time.deltaTime);
            } else {
                Debug.DrawRay (penModel.transform.position, penModel.transform.up * 30, Color.red, Time.deltaTime);
            }

            oldInfoPackage = newInfoPackage;
        }
    }
}