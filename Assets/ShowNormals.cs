using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShowNormals : MonoBehaviour {
    Mesh mesh;
    float normalLength = 1;
    // Start is called before the first frame update
    void Start () {
        mesh = transform.GetComponent<MeshFilter> ().mesh;
    }

    // Update is called once per frame
    void Update () {
        for (int i = 0; i < mesh.vertices.Length; i++) {
            Vector3 norm = transform.TransformDirection (mesh.normals[i]);
            Vector3 vert = transform.TransformPoint (mesh.vertices[i]);
            Debug.DrawRay (vert, norm * normalLength, Color.red);
        }
    }
}