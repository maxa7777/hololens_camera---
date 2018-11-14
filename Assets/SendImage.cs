#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

#else
using System;
using System.IO;
using Windows.ApplicationModel.Background;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;

using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif
using UnityEngine.Assertions;


public class SendImage : MonoBehaviour {
    int width = 416;
    int height = 416;
    public int fps = 30;
    public string num = null;
    public Texture2D texture;
    WebCamTexture webcamTexture;
    public Color32[] colors = null;
    IEnumerator Init()
    {
        while (true)
        {
            if (webcamTexture.width > 16 && webcamTexture.height > 16)
            {
                colors = new Color32[webcamTexture.width * webcamTexture.height];
                texture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32, false);
                GetComponent<Renderer>().material.mainTexture = texture;
                break;
            }
            yield return null;
        }
    }

#if UNITY_EDITOR
    public string recvtext;
    void Start(){
        recvtext = "start at unity";
     WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(devices[0].name, this.width, this.height, this.fps);
        webcamTexture.Play();
        StartCoroutine(Init());
        num =webcamTexture.deviceName;
    }

    void Update()
    {
        //webcamera
        if (colors != null)
        {
            webcamTexture.GetPixels32(colors);
            texture.SetPixels32(colors);
            texture.Apply();
        }
    }
#else

    public string recvtext;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(devices[0].name, this.width, this.height, this.fps);
        webcamTexture.Play();
        StartCoroutine(Init());
        num = webcamTexture.deviceName;
        recvtext = "network Start";
    }

    async void Update()
    {
        //webcamera
        if (colors != null)
        {
            webcamTexture.GetPixels32(colors);
            texture.SetPixels32(colors);
            texture.Apply();
        }

        try
        {
            // Create the StreamSocket and establish a connection to the echo server.
            using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
            {
                // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                var hostName = new Windows.Networking.HostName("localhost");
                
                //this.clientListBox.Items.Add("client is trying to connect...");

                await streamSocket.ConnectAsync(hostName, "8052");

                //this.clientListBox.Items.Add("client connected");

                // Send a request to the echo server.
                string request = "Hello, World!";
                using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(request);
                        //byte[] data_jpg = texture.EncodeToJPG();
                        
                        //await streamWriter.WriteLineAsync(System.Text.Encoding.GetEncoding("utf-8").GetString(data_jpg));

                        await streamWriter.FlushAsync();
                    }
                }

                //this.clientListBox.Items.Add(string.Format("client sent the request: \"{0}\"", request));

                // Read data from the echo server.

                string response;
                using (Stream inputStream =streamSocket.InputStream.AsStreamForRead())
                {
                    inputStream.ReadTimeout = 100;
                    using (StreamReader streamReader = new StreamReader(inputStream))
                    {

                        response = await streamReader.ReadLineAsync();
                        
                        recvtext = response;
                    }
                }

                
                //this.clientListBox.Items.Add(string.Format("client received the response: \"{0}\" ", response));
            }

            //this.clientListBox.Items.Add("client closed its socket");
        }
        catch (Exception ex)
        {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
            //this.clientListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            //recvtext = webErrorStatus.ToString()+ex.Message;
            Debug.Log("loop");
        }

        
    }

    //private async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
    //{
    //    string request;
    //    using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
    //    {
    //        request = await streamReader.ReadLineAsync();
    //    }

    //    //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));

    //    // Echo the request back as the response.
    //    using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
    //    {
    //        using (var streamWriter = new StreamWriter(outputStream))
    //        {
    //            await streamWriter.WriteLineAsync(request);
    //            await streamWriter.FlushAsync();
    //        }
    //    }

    //    //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server sent back the response: \"{0}\"", request)));

    //    sender.Dispose();

    //    //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("server closed its socket"));
    //}
#endif
}
