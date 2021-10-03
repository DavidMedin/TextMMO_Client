using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ServerHandler : MonoBehaviour
{
    private TcpClient _Client;
    private NetworkStream _stream;
    private HistoryManager man;
    private bool _tryConnect;
    private byte[] data;

    private List<string> messages = new List<string>();
    // Start is called before the first frame update
    void Start() {
        data = new byte[256];
        _tryConnect = true;
        Connect();
        //find HistoryManager
        var history = GameObject.Find("History");
        man = history.GetComponent<HistoryManager>();
        if (man == null)
        {
            print("Couldn't find the HistoryManager component");
        }
    }

    async void Connect() {
        try {
            _Client = new TcpClient(AddressFamily.InterNetworkV6);
            await _Client.ConnectAsync("localhost", 8080);
        }
        catch (SocketException se) {
            print("trying again: " + se.Message);
            return;
        }
        print("Got connection");
        _stream = _Client.GetStream();
        //start receiving
        _stream.BeginRead(data, 0, 256, Receive, _stream);
        _tryConnect = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (_tryConnect == true) {
            Connect();
        }
        else {
            lock (messages) {
                foreach (var msg in messages) {
                    man.AddTextEntry(msg);
                }

                messages.Clear();
            }
            
        }
    }

    public void Receive(IAsyncResult ar) {
        NetworkStream str = ar.AsyncState as NetworkStream;
        //byte[] buff = new byte[256];
        int read = 0;
        try {
            read = str.EndRead(ar);
        }
        catch (IOException ioe) {
            print("socket disconneced: " + ioe.Message);
            Connect();
            return;
        }
        string msg;
        if (read == 0) {
            print("done, disconnecting");
            return;
        }

        msg = Encoding.ASCII.GetString(data, 0, read);
        //print(msg);
        lock (messages) {
            messages.Add(msg);
        }
        _stream.BeginRead(data, 0, 256, Receive, str);
    }

    public void Send(string message) {
        Byte[] msg = Encoding.ASCII.GetBytes(message);
        _stream.Write(msg,0,msg.Length);
    }
}
