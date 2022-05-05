using System;  
using System.IO;
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Threading;  

using UnityEngine;

// State object for reading client data asynchronously  
public class StateObject {
    // Size of receive buffer.  
    public int BufferSize = -1;
    // public const int BufferSize = 1024;

    // Receive buffer.  
    public byte[] buffer = null;
    // public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public BinaryReader br = null; // = new BinaryReader();
    // public StringBuilder sb = new StringBuilder();

    // Client socket.
    public Socket workSocket = null;

    public StateObject(byte[] inBytes) {
        BufferSize = inBytes.Length;
        buffer = inBytes;
    }
    // StateObject(int numLeds) {
    //     BufferSize = 3 * numLeds;
    //     buffer = new byte[BufferSize];
    // }

    // ~StateObject() {
    //     buffer = null;
    // }
}  
  
public class MyThingyqq {
    public Socket listener;
    public byte[] myBytes;
    
    public MyThingyqq(Socket inListener, byte[] inBytes) {
        listener = inListener;
        myBytes = inBytes;
    }

    ~MyThingyqq() {
        listener = null;
        myBytes = null;
    }
}

public class AsynchronousSocketListener {
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    static bool loopyLoop = true;

    // Debugging
    const string directory = "Assets/Exports";
    const string filename = "debug.txt";
    public static StreamWriter writer = new StreamWriter(directory + "/" + filename);

    public AsynchronousSocketListener() {
    }

    ~AsynchronousSocketListener() {
        writer.Close();
        writer = null;
    }

    public static void debug_log(string log) {
        // Debug.Log(log);
        // if (writer != null) {
        //     writer.WriteLine(log);
        //     writer.Flush();
        // }
    }

    public static void StartListening(int port, byte[] superBytes) {
        debug_log("StartListening");

        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        // IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
        IPAddress ipAddress = IPAddress.Loopback; // ipHostInfo.AddressList[0];  
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);  

        debug_log("Start socket");
        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,  
            SocketType.Stream, ProtocolType.Tcp );  

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try {  
           debug_log("Start bind");
            listener.Bind(localEndPoint);
           debug_log("Start listen");
            listener.Listen(100);  
  
            bool allDoneResult = false;
            while (loopyLoop) {  
                debug_log("top of loopyLoop");
                // Set the event to nonsignaled state.  
                allDone.Reset();  

                // Start an asynchronous socket to listen for connections.  
                MyThingyqq mt = new MyThingyqq(listener, superBytes);
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),  
                    mt ); 
                    // listener ); 

                // Wait until a connection is made before continuing.
                debug_log("waiting for allDone ");  
                allDoneResult = allDone.WaitOne(-1);
                while (!allDoneResult) {
                    // debug_log("Waiting 100 milliseconds");
                    // Thread.Sleep(100);
                    debug_log("baaaaaa!!!!");
                    allDoneResult = allDone.WaitOne(100);
                }  
                debug_log("wait for allDone.WaitOne() done");
            }  
        } catch (Exception e) {  
            debug_log("StartListening error:" + e.ToString());  
        }  
  
        debug_log("\nPress ENTER to continue (NOT)...");  
        // Console.Read(); 
        debug_log("Closing socket");
        listener.Close();
        debug_log("Closing socket, done closed");
  
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        debug_log("AcceptCallback start");
        // Signal the main thread to continue.  
        allDone.Set();  
        debug_log("AcceptCallback start 2");
  
        // Get the socket that handles the client request.  
        MyThingyqq mt = (MyThingyqq) ar.AsyncState;
        Socket listener = mt.listener;
        // Socket listener = (Socket) ar.AsyncState;  
        debug_log("AcceptCallback start 2a listener: " + listener);
        debug_log("AcceptCallback start 2a .Connected: " + listener.Connected);
        debug_log("AcceptCallback start 2a .IsBound: " + listener.IsBound);

        if (!loopyLoop) {
            debug_log("AcceptCallback loopyLoop==false, aborting");
            return;
        }
        Socket handler;
        try
        {
            handler = listener.EndAccept(ar);  
            // Socket handler = listener.EndAccept(ar);  
        }
        catch (Exception e)
        {
            debug_log("AcceptCallback error: " + e.ToString());  
            return;
        }  
        debug_log("AcceptCallback start 3");
  
        // Create the state object.  
        StateObject state = new StateObject(mt.myBytes);  
        state.workSocket = handler;  
        debug_log("AcceptCallback start 4");
        handler.BeginReceive( state.buffer, 0, state.BufferSize, 0,  
        // handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,  
            new AsyncCallback(ReadCallback), state);  
        debug_log("AcceptCallback is done");
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        debug_log("ReadCallback");
        String content = String.Empty;  
  
        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject) ar.AsyncState;  
        Socket handler = state.workSocket;
        Stream s = new NetworkStream(handler); 
        BinaryReader br = new BinaryReader(s);

        // Read data from the client socket.
        debug_log("pre handler.EndReceive");
        int bytesRead = handler.EndReceive(ar);  
        debug_log("post handler.EndReceive1: " + bytesRead);
  
        if (bytesRead > 0) {
            debug_log("*** in my bytes read");
            Send(handler, bytesRead);
                debug_log("receiving more");
                if (loopyLoop) {
                    debug_log("yep, loopy looping");
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, state.BufferSize, 0,  
                    // handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
                        new AsyncCallback(ReadCallback), state);  
                debug_log("done doing another BeginReceive");
                } else {
                    handler.Close();
                }
                
                debug_log("always receiving more 2");
        }  
        if (false && bytesRead > 0) {  
        debug_log("post handler.EndReceive2: " + bytesRead);
            // There  might be more data, so store the data received so far.  
            // state.sb.Append(Encoding.ASCII.GetString(  
            //     state.buffer, 0, bytesRead));
            
        debug_log("post handler.EndReceive3: " + bytesRead);
  
            // Check for end-of-file tag. If it is not there, read
            // more data.  
        debug_log("post handler.EndReceive4: " + bytesRead);
        // debug_log("post handler.EndReceive content: " + state.sb.ToString());
        //     content = state.sb.ToString();  
            if (content.IndexOf("<EOF>") > -1) {  
                debug_log("Inside MEX MEX content.IndexOf yo!");
                // All the data has been read from the
                // client. Display it on the console.  
                debug_log("Read " + content.Length + " bytes from socket. \n Data : " + content);  
                // Echo the data back to the client.  
                Send(handler, content);  
            } else {  
                debug_log("Inside else content.IndexOf");
                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, state.BufferSize, 0,  
                // handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
                new AsyncCallback(ReadCallback), state);  
                debug_log("Inside else content.IndexOf end");
            }  
        }  
        debug_log("ReadCallback is done");
    }

    private static void Send(Socket handler, int numBytesReceived)
    {
        debug_log("### In send int");
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = BitConverter.GetBytes(numBytesReceived);  
        debug_log("### In send int 2 + " + byteData.Length);
  
        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,  
            new AsyncCallback(SendCallback), handler);  
        debug_log("### In send int 3");
    }

    private static void Send(Socket handler, String data)
    {
        debug_log("### In send");
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);  
        debug_log("### In send 2");
  
        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,  
            new AsyncCallback(SendCallback), handler);  
        debug_log("### In send 3");
    }

    public static void FinishNow() {
        debug_log("#### Finish now!");
        loopyLoop = false;
        allDone.Set();

    }
    private static void SendCallback(IAsyncResult ar)
    {
        debug_log("### In send callback");
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket) ar.AsyncState;  
  
            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);  
            debug_log("Sent " + bytesSent + " bytes to client.");  
  
            // handler.Shutdown(SocketShutdown.Both);  
            // handler.Close();  
  
        }
        catch (Exception e)
        {
            debug_log("SendCallback error: " + e.ToString());  
        }  
        debug_log("## SendCallback is done");
    }
}