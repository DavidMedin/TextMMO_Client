using System;
using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    async void Start()
    {
        _Client = new TcpClient(AddressFamily.InterNetworkV6);
        await _Client.ConnectAsync("localhost", 8080);
        print("Got connection");
        _stream = _Client.GetStream();
        //start receiving
        Receive();
       
        //find HistoryManager
        var history = GameObject.Find("History");
        man = history.GetComponent<HistoryManager>();
        if (man == null)
        {
            print("Couldn't find the HistoryManager component");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void Receive()
    {
        Byte[] data = new Byte[256];
        int read;
        try {
            read = await _stream.ReadAsync(data, 0, 256);
        }
        catch (AggregateException ae) {
            ae.Handle((e) => {

            });
        }
        print("cancel request: "+cancelToken.IsCancellationRequested);
        print("canceled: " + tsk.IsCanceled);
        String message = Encoding.ASCII.GetString(data, 0, read);
        //print(message);
        man.AddTextEntry(message);
        Receive();
    }

    public void Send(string message) {
        Byte[] msg = Encoding.ASCII.GetBytes(message);
        _stream.Write(msg,0,msg.Length);
    }
}
