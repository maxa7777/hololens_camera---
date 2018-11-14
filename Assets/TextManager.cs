using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour {
    public string recvtext;
    private TextMesh targettext;
    GameObject refObj,refObj2;
    BoundingManager b;
    SendImage t1;
    USTrackingTcpClient t2;
    // Use this for initialization
    void Start () {

        
        refObj = GameObject.Find("Plane");      
        t2 = refObj.GetComponent<USTrackingTcpClient>();
        targettext = GetComponent<TextMesh>();
    }
	
	// Update is called once per frame
	void Update () {
        //targettext.text=t2.joutai;
        //this.targettext.text =t1.recvtext;
        targettext.text ="";
        List<recvdata> cpylist = new List<recvdata>(t2.recvlist);
        foreach (recvdata d in cpylist)
        {
            targettext.text += d.label+"\n";
        }
        
	}
}
