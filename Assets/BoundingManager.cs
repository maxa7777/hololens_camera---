using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingManager : MonoBehaviour {
    GameObject refObj;
    USTrackingTcpClient t2;
    public Vector3 Outpos;
    public float[] xy;
    public float[] center;
    private GameObject c1, c2, c3, c4;
    private int width,height;
	// Use this for initialization
    //試しに追記　更新テスト
    //Cloneもしてみる
	void Start () {
        refObj = GameObject.Find("Plane");
        t2 = refObj.GetComponent<USTrackingTcpClient>();
        width = t2.width;
        height = t2.height;
        xy = new float[2];
        center = new float[3];
        c1 = transform.GetChild(1).gameObject;
        c2 = transform.GetChild(2).gameObject;
        c3 = transform.GetChild(3).gameObject;
        c4 = transform.GetChild(4).gameObject;
        
    }
	
	// Update is called once per frame
	void Update () {
        //List<recvdata> cpylist = new List<recvdata>(t2.recvlist);
        
        //if(cpylist!= null)
        {
            //matPointToWorldPoint(cpylist[0].label,cpylist[0].pos[0], cpylist[0].pos[1]);
            this.transform.position = Outpos;
            transform.LookAt(Camera.main.transform);
            //子オブジェクト参照して4つのCubeの座標変更　Cube1,2,3,4
            Vector3 temp;
            
           
            Vector3 pos = new Vector3();
            temp = new Vector3(center[0]+xy[0]/2, center[1]+xy[1]/2, 0);     
            matPointToWorldPoint(ref pos, temp.x, temp.y);
            c1.transform.position = pos;
            temp = new Vector3(center[0] + xy[0]/2, center[1] - xy[1]/2, 0);
            matPointToWorldPoint(ref pos, temp.x, temp.y);
            c2.transform.position = pos;
            temp = new Vector3(center[0] - xy[0]/2, center[1] - xy[1]/2, 0);
            matPointToWorldPoint(ref pos, temp.x, temp.y);
            c3.transform.position = pos;
            temp = new Vector3(center[0] - xy[0]/2, center[1] + xy[1]/2, 0);
            matPointToWorldPoint(ref pos, temp.x, temp.y);
            c4.transform.position = pos;

        }
	}
    //bool matPointToWorldPoint(string label,float x,float y)
    //{
    //    // 画像座標を正規化，Y軸の向きを反転し，ビューポート座標を求める．
    //    float viewportPointX = (float)(x / (float)t2.width);
    //    float viewportPointY = (float)(1.0 - y / (float)t2.height);
    //    Vector3 viewportPoint = new Vector3(viewportPointX, viewportPointY);

    //    // Rayを使って，Z軸も含んだワールド座標を求める．
    //    Ray ray = Camera.main.ViewportPointToRay(viewportPoint);
    //    RaycastHit hitInfo;
    //    //Debug.Log(this.transform.position + "\n" +viewportPoint);
    //    //Debug.Log(label+"  "+ viewportPoint);
    //    if (Physics.Raycast(ray, out hitInfo))
    //    {
    //        Outpos = hitInfo.point;
    //        return true;
    //    }
    //    //Outpos = new Vector3();
    //    Outpos = viewportPoint;
    //    Outpos.z = 5;
    //    return false;
    //}
    bool matPointToWorldPoint(ref Vector3 Outpos, float x, float y)
    {
        // 画像座標を正規化，Y軸の向きを反転し，ビューポート座標を求める．
        float viewportPointX = (float)(x / (float)width);
        float viewportPointY = (float)(1.0 - y / (float)height);
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
