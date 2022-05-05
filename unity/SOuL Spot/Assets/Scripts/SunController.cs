using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

public class SunController : MonoBehaviour
{
    // Designed to be called from the Sun object which is the parent of LED objects

    // Server stuff taken from https://www.codeproject.com/Articles/10649/An-Introduction-to-Socket-Programming-in-NET-using


    Material[] leds;
    bool continuousUpdates;

    private Color color;
    private float Intensity;

    static TcpListener listener;
    const int LIMIT = 1; //number of concurrent clients
    static Thread thread;
    static BinaryReader br;
    static BinaryWriter bw;

    int numPixels;
    byte[] ledBuffer;

    bool firstNonzeroBytes;

    // Start is called before the first frame update
    void Start()
    {        
        Debug.Log("Num Children: " + gameObject.transform.childCount);


        firstNonzeroBytes = false;

        numPixels = 600;
        ledBuffer = new byte[3 * numPixels];
        leds = new Material[numPixels];
        Transform sun = gameObject.transform;
        for (int i = 0; i < numPixels; i++) {
            ledBuffer[i] = (byte) (i % 128);
            GameObject led = sun.GetChild(i).gameObject;
            Material mat = led.GetComponent<Renderer>().material;
            mat.EnableKeyword("_EMISSION");
            leds[i] = mat;
            // Debug.Log(i + ": (" + led.transform.position[0] + ", " + led.transform.position[1] + ", " + led.transform.position[2] + ")");
        }
        ledBuffer[0] = 0xff;
        ledBuffer[1] = 0xfe;
        ledBuffer[2] = 0xfd;

    // Console.WriteLine("Starting..");
    // Debug.Log("Starting..");
        // GameObject go = GameObject.Find("MyCreatedGO0");
        // Renderer rend = go.GetComponent<Renderer> ();
        // Shader shader = Shader.Find("Standard");
        // Material material = rend.material;
        //     material.EnableKeyword("_RECEIVE_SHADOWS_OFF");
        //     material.DisableKeyword("_ADDITIONAL_LIGHT_SHADOWS");
        //     material.DisableKeyword("_SHADOWS_SOFT");
        //     material.EnableKeyword("_EMISSION");
        //     material.SetColor("_EmissionColor", Color.green);
        // goMaterial = material;
    // Debug.Log("End..");

        continuousUpdates = true;
        DoServer();
        // AsynchronousSocketListener.StartListening();

    }

    // Update is called once per frame
    void Update()
    {
        // for (int i = 0; i < numPixels; ++i) {
        //     Debug.Log("Up bytes: " + (int)superBytes[i]);
        // }
        // Debug.Log("g---");

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        } else if (Input.GetKeyDown(KeyCode.G)) {
            // Debug.Log("Hit G for Go Server");
            // Debug.Log("Calling DoServer()");
            // DoServer();
            // Debug.Log("Called DoServer()");
        } else if (Input.GetKeyDown(KeyCode.L)) {
            continuousUpdates = !continuousUpdates;
            Debug.Log("Hit L for Listen - toggle to continuousUpdates=" + continuousUpdates);
        } else if (Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("Hit R for Random - Set all LEDs to random values");
            var rand = new System.Random();
            for (int i = 0; i < numPixels; i++) {
                Color color = new Color(
                    (float)rand.NextDouble(),
                    (float)rand.NextDouble(),
                    (float)rand.NextDouble()
                );
                // Debug.Log("Color: " + i + " (" + color[0] + "," + color[1] + "," + color[2] + ")");
                leds[i].SetColor("_Color", color);
                leds[i].SetColor("_EmissionColor", color);
            }
        }

        if (continuousUpdates) {
            bool doPrint = false;
            if ((ledBuffer[0] != 0xff) && (ledBuffer[1] != 0xfe) && (ledBuffer[2] != 0xfd)) {
                doPrint = true;
            }
            // if (doPrint) {
            //     for (int i = 0; i < 3 * numPixels; i++) {
            //         Debug.Log(i + ": " + ledBuffer[i]);
            //     }
            // }
            for (int i = 0; i < numPixels; i++) {
                Color color = new Color(
                    (float)ledBuffer[3 * i + 0] / 255,
                    (float)ledBuffer[3 * i + 1] / 255,
                    (float)ledBuffer[3 * i + 2] / 255
                    );
                // Debug.Log("ak! " + i + " = " + 3*((int) i / 3) + 0 + " " + 3*((int) i / 3) + 1l + " " + 3*((int) i / 3) + 2);
                if (doPrint) {
                    GameObject led = gameObject.transform.GetChild(i).gameObject;
                    // Debug.Log(i + ": (" 
                    //     + led.transform.position[0].ToString("f4") + ", " + led.transform.position[1].ToString("f4") + ", " + led.transform.position[2].ToString("f4") + ")" 
                    //     + " -- ("
                    //     + ledBuffer[3*((int) i / 3) + 0] + ", " + ledBuffer[3*((int) i / 3) + 1] + ", " + ledBuffer[3*((int) i / 3) + 2] + ")");
                }
                
                leds[i].SetColor("_Color", color);
                leds[i].SetColor("_EmissionColor", color);
            }
        }
    }
    void OnDestroy() {
        Debug.Log("OnDestroy called");
        if (br != null) {
            br.Close();
        }
        if (bw != null) {
            bw.Close();
        }
        AsynchronousSocketListener.FinishNow();
        Debug.Log("pre join, thread is: " + thread);
        thread.Join();
        Debug.Log("post join, thread is: " + thread);
        // thread.Interrupt();
        Debug.Log("OnDestroy called done");
    }

    private void DoServer(){
        bool async = true;
        if (async) {
            int port = 12508;
            thread = new Thread(unused => AsynchronousSocketListener.StartListening(port, ledBuffer));
            thread.IsBackground = true;
            thread.Start(); 
        } else {
            int port = 12508;
            // string ipstr = "127.0.0.1";
            // IPAddress iP = new System.Net.IPAddress.Parse(ipstr);
            // listener = new TcpListener(iP, port);
            listener = new TcpListener(port);
            listener.Start();
            Debug.Log("Server mounted, listening to port " + port);
            for(int i = 0;i < LIMIT;i++){
                thread = new Thread(unused => Service(ledBuffer, numPixels));
                // thread = new Thread(new ParameterizedThreadStart(Service));
                // Thread t = new Thread(new ParameterizedThreadStart(Service));
                // Thread t = new Thread(new ThreadStart(Service));
                thread.Start();
            }
        }
    }

    public static void Service(byte[] superBytes3, int numPixels){
        byte[] superBytes2 = (byte[]) superBytes3;
        Debug.Log("We got this");

        bool useBinaryReader = true;
        // byte[] buffer = new byte[3];

        while(true){
            Debug.Log("Accepting socket");
            Socket soc = listener.AcceptSocket();
            //soc.SetSocketOption(SocketOptionLevel.Socket,
            //        SocketOptionName.ReceiveTimeout,10000);
            Debug.Log("Connected:" + soc.RemoteEndPoint.ToString());
            try{
                Stream s = new NetworkStream(soc); 

                br = new BinaryReader(s);
                bw = new BinaryWriter(s);
                //sw.WriteLine("{0} Employees available", 
                //      ConfigurationSettings.AppSettings.Count);
                while(true){
                    if (useBinaryReader) {
                        // Debug.Log("Reading2");
                        int numRead = br.Read(superBytes2, 0, 3 * numPixels);
                        // Debug.Log("Got2");
                        // Debug.Log("Read: " + numRead);
                        if ((numRead == -1) || (numRead == 0))
                        {
                            break;
                        }
                        // Debug.Log("Bytes2:");
                        // for (int i = 0; i < numRead; ++i) {
                        //     Debug.Log((int)superBytes2[i]);
                        // }
                        // Debug.Log("Writing");
                        byte[] good = {0xff};
                        bw.Write(numRead);
                        // Debug.Log("Wrote");
                    } else {
                        StreamReader sr = new StreamReader(s);
                        StreamWriter sw = new StreamWriter(s);
                        sw.AutoFlush = true; // enable automatic flushing
                        string name = sr.ReadLine();
                        Debug.Log("name: " + name);
                        if(name == "" || name == null) break;
                        //string job = 
                        //    ConfigurationSettings.AppSettings[name];
                        //if(job == null) job = "No such employee";

                        System.Threading.Thread.Sleep(1);
                        Debug.Log("Writing response");
                        sw.WriteLine(name);
                        System.Threading.Thread.Sleep(1);
                        Debug.Log("Done writing repsonse");
                    }
                }
                Debug.Log("Done");
                s.Close();
            }catch(Exception e){
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }
            Console.WriteLine("Disconnected: {0}", soc.RemoteEndPoint);
            soc.Close();
        }
        Debug.Log("Exiting Service()");
    }
}

