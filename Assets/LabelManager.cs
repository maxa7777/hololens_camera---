using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelManager : MonoBehaviour {
    GameObject refObj;
    USTrackingTcpClient t2;
    public string label;
    // Use this for initialization
    void Start () {
        label = null;
    }
	
	// Update is called once per frame
	void Update () {
        if (label != null)
            this.GetComponent<TextMesh>().text = label;
        else
            this.GetComponent<TextMesh>().text = "null";
    }
}
