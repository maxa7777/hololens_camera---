//#define USEPHOTOCAPTURE

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Linq;
using UnityEngine.VR.WSA.WebCam;

#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif

[Serializable]
public class recvdata
{

    public string label;
    public float prob;
    public float[] pos = new float[4];
    public int[] time = new int[4];
    //public static recvdata CreateFromJSON(string jsonstring)
    //{
    //    return JsonUtility.FromJson<recvdata>(jsonstring);
    //}
}
[Serializable]
public class rootdata
{
    public List<recvdata> re;
}


public class USTrackingTcpClient : MonoBehaviour
{

   public int width = 416; //640 512
   public int height = 416;//360 288
    public int fps = 60;
    public string num = null;
    public Texture2D texture;
    WebCamTexture webcamTexture;
    public Color32[] colors = null;
    public Color32[] tempcolors = null;
    public List<recvdata> recvlist;
    public int wrcount;
    private int exccount;
    public float GetpixelTime, SetpixelTime;

    Texture2D targetTexture = null;
    PhotoCapture photoCaptureObject = null;
    IEnumerator Init()
    {
        while (true)
        {
            if (webcamTexture.width > 16 && webcamTexture.height > 16)
            {
                colors = new Color32[webcamTexture.width * webcamTexture.height];
                tempcolors = new Color32[webcamTexture.width * webcamTexture.height];
                texture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false,true);

                break;
            }
            yield return null;
        }
    }

#if !UNITY_EDITOR
    private bool _useUWP = true;
    private Windows.Networking.Sockets.StreamSocket socket;
    private Task exchangeTask;
#endif

#if UNITY_EDITOR
    private bool _useUWP = false;
    System.Net.Sockets.TcpClient client;
    System.Net.Sockets.NetworkStream stream;
    private Thread exchangeThread;
#endif


    private StreamWriter writer;
    private StreamReader reader;
    public string recvtext;

    public void Connect(string host, string port)
    {
        if (_useUWP)
        {
            ConnectUWP(host, port);
        }
        else
        {
            ConnectUnity(host, port);
        }
    }



#if UNITY_EDITOR
    private void ConnectUWP(string host, string port)
#else
    private async void ConnectUWP(string host, string port)
#endif
    {
#if UNITY_EDITOR
        errorStatus = "UWP TCP client used in Unity!";
#else
        try
        {
            if (exchangeTask != null) StopExchange();
        
            socket = new Windows.Networking.Sockets.StreamSocket();
            Windows.Networking.HostName serverHost = new Windows.Networking.HostName(host);
            await socket.ConnectAsync(serverHost, port);
        
            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(streamOut) { AutoFlush = true };
        
            Stream streamIn = socket.InputStream.AsStreamForRead();
            reader = new StreamReader(streamIn);

            RestartExchange();
            successStatus = "Connected!";
        }
        catch (Exception e)
        {
            errorStatus = e.ToString();
        }
#endif
    }

    private void ConnectUnity(string host, string port)
    {
#if !UNITY_EDITOR
        errorStatus = "Unity TCP client used in UWP!";
#else
        try
        {
            if (exchangeThread != null) StopExchange();

            client = new System.Net.Sockets.TcpClient(host, Int32.Parse(port));
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            RestartExchange();
            successStatus = "Connected!";
        }
        catch (Exception e)
        {
            errorStatus = e.ToString();
        }
#endif
    }

    private bool exchanging = false;
    private bool exchangeStopRequested = false;
    private string lastPacket = null;

    private string errorStatus = null;
    private string warningStatus = null;
    private string successStatus = null;
    private string unknownStatus = null;
    private string strdata = null;


    public void RestartExchange()
    {
#if UNITY_EDITOR
        if (exchangeThread != null) StopExchange();
        exchangeStopRequested = false;
        exchangeThread = new System.Threading.Thread(ExchangePackets);
        exchangeThread.Start();
#else
        if (exchangeTask != null) StopExchange();
        exchangeStopRequested = false;
        exchangeTask = Task.Run(() => ExchangePackets());
        
#endif
    }
    char[] temp = new char[416 * 416];
    public void Start()
    {
        //Connect巻数にHostとPortを指定して渡してやる
        Connect("10.40.1.175", "3333");
        recvtext = "first";
        wrcount = 0;
        exccount = 0;
        strdata = new string(temp);
        GetpixelTime = 0;
        SetpixelTime = 0;


#if USEPHOTOCAPTURE
        PhotoCapture.CreateAsync(false,OnPhotoCaptureCreated);
#else

        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(devices[0].name, this.width, this.height, this.fps);
        webcamTexture.Play();
        StartCoroutine(Init());
        num = webcamTexture.deviceName;
        //Debug.Log("in start");
#endif
    }
    private bool sended = false;
    private bool captured = false;
    public void Update()
    {


        if (colors != null)
        {

            //現在時刻取得
            //float time = Time.realtimeSinceStartup;
            if (captured == false)
            {
                webcamTexture.GetPixels32(colors);
                
                captured = true;
            }
            
            webcamTexture.GetPixels32(colors);
            texture.SetPixels32(colors);
            strdata = Texture2DToBase64(texture);
            ////Color32toBase64();
            //byte[] tempbyte = Convert.FromBase64String(strdata);
            //Color32[] testcolors = new Color32[width * height];
            //for (int i = 0; i < tempbyte.Length / 4; i++)
            //{
            //    testcolors[i].b = tempbyte[4 * i];
            //    testcolors[i].g = tempbyte[4 * i + 1];
            //    testcolors[i].r = tempbyte[4 * i + 2];
            //    testcolors[i].a = tempbyte[4 * i + 3];
            //}
            //texture.SetPixels32(testcolors);

            texture.Apply();
            GetComponent<Renderer>().material.mainTexture = texture;
        }

        if (lastPacket != null)
        {
           
        }

        if (errorStatus != null)
        {
            //StatusTextManager.SetError(errorStatus);
            Debug.Log(errorStatus);
            errorStatus = null;
        }
        if (warningStatus != null)
        {
            //StatusTextManager.SetWarning(warningStatus);
            Debug.Log(warningStatus);
            warningStatus = null;
        }
        if (successStatus != null)
        {
            //StatusTextManager.SetSuccess(successStatus);
            Debug.Log(successStatus);
            successStatus = null;
        }
        if (unknownStatus != null)
        {
            //StatusTextManager.SetUnknown(unknownStatus);
            Debug.Log(unknownStatus);
            unknownStatus = null;
        }
        //photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
#if USEPHOTOCAPTURE
        //PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
#endif
    }

    public void ExchangePackets()
    {
        //ここで延々ループ回っているからSleepなりで遅らせるといいかも 一番いいのはThreadの設定で調節
        while (!exchangeStopRequested)
        {
          
            if (writer == null || reader == null) continue;
            exchanging = true;
            
#if UNITY_EDITOR
            //strdata = Color32toBase64();
            //strdata = Texture2DToBase64(texture);
#else
            //strdata = Texture2DToBase64(texture);
           strdata=Color32toBase64();
#endif
            if (strdata != null&&sended==false)
                {
                    writer.Write(strdata + "\n");
                    }
                else
                    writer.Write("X\n");

            
            wrcount++;
            string received = null;

#if UNITY_EDITOR
            byte[] bytes = new byte[client.SendBufferSize];
            int recvint = 0;
            while (true)
            {
                
                recvint = stream.Read(bytes, 0, client.SendBufferSize);
                received += Encoding.UTF8.GetString(bytes, 0, recvint);
                //received += Encoding.ASCII.GetString(bytes, 0, recvint);
                if (received.EndsWith("\n")) break;
                Debug.Log("in whle");
            }
#else
            received = reader.ReadLine();
#endif

            lastPacket = received;
            
            recvtext = received;
            exchanging = false;
            //Debug.Log("log=" + recvtext);
            //文字列分割
            string[] separate = { "]]," };
            //string[] words = recvtext.Split(separate, System.StringSplitOptions.RemoveEmptyEntries);
            string[] words = recvtext.Split(separate, System.StringSplitOptions.RemoveEmptyEntries);
            char[] removechar = new char[] { '[', ']', '\"' };
            recvlist = new List<recvdata>();
            if (words.Length > 10)
                Debug.Log("stop");
            if (recvlist != null) recvlist.Clear();

            foreach (var word in words)
            {

                recvdata recv = new recvdata();
#if UNITY_EDITOR
                string removeword = String.Copy(word);
#else
                string removeword=new string(word.ToCharArray());
#endif

                foreach (char c in removechar)
                {
                    removeword = removeword.Replace(c.ToString(), "");
                }
                if (word == "" || word == "[]" || removeword == "\n" || word == "") continue;


                //Debug.Log(word.Length + " " + System.Runtime.InteropServices.Marshal.SizeOf<recvdatatest>());
                //if (word.Length < System.Runtime.InteropServices.Marshal.SizeOf<recvdatatest>()) continue;

                string[] datas = removeword.Split(',');
                recv.label = datas[0];
                recv.prob = Convert.ToSingle(datas[1]);
                recv.pos[0] = Convert.ToSingle(datas[2]);
                recv.pos[1] = Convert.ToSingle(datas[3]);
                recv.pos[2] = Convert.ToSingle(datas[4]);
                recv.pos[3] = Convert.ToSingle(datas[5]);

                recvlist.Add(recv);
            }
            //byte[] tempbyte=Convert.FromBase64String(strdata);
            //Color32[] colors = new Color32[width * height];
            //for (int i = 0; i < tempbyte.Length / 4; i++)
            //{
            //    colors[i].b = tempbyte[4 * i];
            //    colors[i].g = tempbyte[4 * i + 1];
            //    colors[i].r = tempbyte[4 * i + 2];
            //    colors[i].a = tempbyte[4 * i + 3];
            //}
            //Debug.Log(colors.Length);
#if UNITY_EDITOR
            System.Threading.Thread.Sleep(8);          
#endif
        }
    }

#if !UNITY_EDITOR
    private async void Delay(int ms)
    {
        await Task.Delay(ms);
    }
#endif

    public void StopExchange()
    {
        exchangeStopRequested = true;

#if UNITY_EDITOR
        if (exchangeThread != null)
        {
            exchangeThread.Abort();
            stream.Close();
            client.Close();
            writer.Close();
            reader.Close();

            stream = null;
            exchangeThread = null;
        }
#else
        if (exchangeTask != null) {
            exchangeTask.Wait();
            socket.Dispose();
            writer.Dispose();
            reader.Dispose();

            socket = null;
            exchangeTask = null;
        }
#endif
        writer = null;
        reader = null;
    }

    public void OnDestroy()
    {
        StopExchange();
    }

    public static string Texture2DToBase64(Texture2D texture)
    {

        //EncodeToJPG()がかなり重い（60msec）これを代替する
        byte[] imageData = texture.EncodeToJPG();

        
        return Convert.ToBase64String(imageData);
    }
    //ColorをByteに変換しても変わらず重い
  public string Color32toBase64()
    {
    
        byte[] colorbytes = new byte[width * height * 4];
        //float time = Time.realtimeSinceStartup;
        for(int i = 0; i < colors.Length; i++)
        {
            colorbytes[4 * i] = colors[i].b;
            colorbytes[4 * i + 1] = colors[i].g;

            colorbytes[4 * i+2] = colors[i].r;
            colorbytes[4 * i + 3] = colors[i].a;
        }
        //Encodetime = Time.realtimeSinceStartup - time;
        return Convert.ToBase64String(colorbytes);
    }
    public Color32[] Base64toColor32()
    {
        Color32[] ret= new Color32[width * height];
        Debug.Log(strdata.Length);
        byte[] temp = System.Convert.FromBase64String(strdata);
        for(int i = 0; i < strdata.Length / 4; i++)
        {
            ret[i].b = temp[4 * i];
            ret[i].g = temp[4 * i+1];
            ret[i].r = temp[4 * i+2];
            ret[i].a = temp[4 * i+3];
        }


        return ret;
    }
    public static Texture2D Base64ToTexture2D(string encodedData)
    {
        byte[] imageData = Convert.FromBase64String(encodedData);

        int width, height;
        GetImageSize(imageData, out width, out height);

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(imageData);

        return texture;
    }

    private static void GetImageSize(byte[] imageData, out int width, out int height)
    {
        width = ReadInt(imageData, 3 + 15);
        height = ReadInt(imageData, 3 + 15 + 2 + 2);
    }

    private static int ReadInt(byte[] imageData, int offset)
    {
        return (imageData[offset] << 8) | imageData[offset + 1];
    }

#if USEPHOTOCAPTURE
    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;
        Debug.Log("width=" + cameraResolution.width + " height=" + cameraResolution.height);
        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
           
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }
    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        //if (result.success)
        //{
        //    List<byte> imageBufferList = new List<byte>();
        //    // Copy the raw IMFMediaBuffer data into our empty byte list.
        //    photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

        //    // In this example, we captured the image using the BGRA32 format.
        //    // So our stride will be 4 since we have a byte for each rgba channel.
        //    // The raw image data will also be flipped so we access our pixel data
        //    // in the reverse order.
        //    int stride = 4;
        //    float denominator = 1.0f / 255.0f;
        //    List<Color> colorArray = new List<Color>();
        //    for (int i = imageBufferList.Count - 1; i >= 0; i -= stride)
        //    {
        //        float a = (int)(imageBufferList[i - 0]) * denominator;
        //        float r = (int)(imageBufferList[i - 1]) * denominator;
        //        float g = (int)(imageBufferList[i - 2]) * denominator;
        //        float b = (int)(imageBufferList[i - 3]) * denominator;

        //        colorArray.Add(new Color(r, g, b, a));
        //    }
        //    // Now we could do something with the array such as texture.SetPixels() or run image processing on the list
        //}
        //photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        Debug.Log("OnCapturedPhotoToMemory");

        if (result.success)
        {
            Debug.Log("OnCapturedPhotoToMemory: success");
            // 使用するTexture2Dを作成し、正しい解像度を設定する
            // Create our Texture2D for use and set the correct resolution
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            // 画像データをターゲットテクスチャにコピーする
            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            // テクスチャをマテリアルに適用する
            this.GetComponent<Renderer>().material.mainTexture = targetTexture;
        }
       
    }
    //終了処理
    private void OnApplicationQuit()
    {
        // クリーンアップ
        // Clean up
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }
#endif
}