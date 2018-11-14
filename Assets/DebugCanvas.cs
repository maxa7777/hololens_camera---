using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvas : MonoBehaviour {
   private int frameCount;
   private float prevTime;
    private Text targettext;
    private GameObject refObj;
    USTrackingTcpClient d;
	// Use this for initialization
	void Start () {
        frameCount = 0;
        prevTime = 0.0f;
        targettext = this.GetComponent<Text>();
        refObj = GameObject.Find("Plane");
        d = refObj.GetComponent<USTrackingTcpClient>();
	}
	
	// Update is called once per frame
	void Update () {
        ++frameCount;
        float time = Time.realtimeSinceStartup - prevTime;

        if (time >= 0.5f)
        {
            prevTime = Time.realtimeSinceStartup;
            float fps =(frameCount / time);
            //Debug.Log("frameCount="+frameCount+" fps="+fps);
            this.targettext.text = (fps).ToString()+"\n gettime="+d.GetpixelTime+"\n settime="+d.SetpixelTime;
            d.wrcount = 0;
            
            frameCount = 0;
        }
    }
}
