
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBoxManager : MonoBehaviour {
    GameObject box1, box2, box3,label1,label2,label3,refObj;
    USTrackingTcpClient t2;
    BoundingManager b1, b2, b3;
    LabelManager l1,l2,l3;
    List<BoundingManager> boxlist;
    List<LabelManager> labellist;
    List<recvdata> cpylist;
    // Use this for initialization
    void Start () {
        boxlist = new List<BoundingManager>();
        labellist = new List<LabelManager>();
        //子オブジェクト参照出来たらもっとスマートにかけるかも
        box1 = GameObject.Find("BoundingBox1");
        box2 = GameObject.Find("BoundingBox2");
        box3 = GameObject.Find("BoundingBox3");
        boxlist.Add(box1.GetComponent<BoundingManager>());
        boxlist.Add(box2.GetComponent<BoundingManager>());
        boxlist.Add(box3.GetComponent<BoundingManager>());     
        label1 = GameObject.Find("Label1");
        label2 = GameObject.Find("Label2");
        label3 = GameObject.Find("Label3");
        labellist.Add(label1.GetComponent<LabelManager>());
        labellist.Add(label2.GetComponent<LabelManager>());
        labellist.Add(label3.GetComponent<LabelManager>());
        refObj = GameObject.Find("Plane");
        t2 = refObj.GetComponent<USTrackingTcpClient>();
    }
	
	// Update is called once per frame
	void Update () {
      cpylist = new List<recvdata>(t2.recvlist);
        int ind = 0;
        foreach (recvdata d in cpylist)
        {
            if (ind > 2) break;
            Vector3 Outpos=new Vector3();
            if (d != null)
            {
                matPointToWorldPoint(ref Outpos,d.pos[0],d.pos[1]);
                boxlist[ind].Outpos = Outpos;
                boxlist[ind].center[0] = d.pos[0];
                boxlist[ind].center[1] = d.pos[1];
                boxlist[ind].xy[0] = d.pos[2];
                boxlist[ind].xy[1] = d.pos[3];
                labellist[ind].label = d.label;
                ind++;
            }
            
        }
    }
    bool matPointToWorldPoint(ref Vector3 Outpos,float x, float y)
    {
        // 画像座標を正規化，Y軸の向きを反転し，ビューポート座標を求める．
        float viewportPointX = (float)(x / (float)t2.width);
        float viewportPointY = (float)(1.0 - y / (float)t2.height);
        Vector3 viewportPoint = new Vector3(viewportPointX, viewportPointY);

        // Rayを使って，Z軸も含んだワールド座標を求める．
        Ray ray = Camera.main.ViewportPointToRay(viewportPoint);
        RaycastHit hitInfo;
        //Debug.Log(this.transform.position + "\n" +viewportPoint);
        //Debug.Log(label+"  "+ viewportPoint);
        if (Physics.Raycast(ray, out hitInfo))
        {
            Outpos = hitInfo.point;
            return true;
        }
        //Outpos = new Vector3();
        Outpos = viewportPoint;
        Outpos.z = 5;
        return false;
    }
}
