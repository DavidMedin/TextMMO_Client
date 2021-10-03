using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ServerHandler : MonoBehaviour {
    private TcpClient _client;
    private NetworkStream _stream;

    private byte[] receivedData = new byte[256];
    private bool _tryConnect = true;
    private bool _tryReceive = false;
    public void Start() {
    }

    public void Update() {
        if (_tryConnect)
            Connect();
        if (_tryReceive)
            Receive();
    }

    private async void Receive() {
        _tryReceive = false;
        try {
            int read = await _stream.ReadAsync(receivedData, 0, 256);
            string msg = Encoding.ASCII.GetString(receivedData, 0, read);
            print(msg);
            _tryReceive = _tryReceive = true;
        }
        catch(IOException e) {
            print("caught exception -> " +  e.Message);
            _stream.Close();
            _client.Close();
            _tryConnect = true;
        }

    }

    private async void Connect() {
        print("connecting");
        _tryConnect = false;
        if (_client is
        { Connected: true }) {
            print("Client already connected");
            return;
        }
        _client = new TcpClient(AddressFamily.InterNetworkV6);
        try {
            //bool available;
            //do {
            //    available = true;
            //    IPGlobalProperties globProps = IPGlobalProperties.GetIPGlobalProperties();
            //    TcpConnectionInformation[] tcpConnArr = globProps.GetActiveTcpConnections();
            //    foreach (var tcpConn in tcpConnArr) {
            //        if (tcpConn.LocalEndPoint.Port == 8080) {
            //            available = false;
            //            break;
            //        }
            //    }
            //} while (available == false);

            await _client.ConnectAsync("127.0.0.1", 8080);
            _stream = _client.GetStream();
            
            //start receiving
        }
        catch(Exception e) {
            print("failed to connect: "+e.Message );
            _tryConnect = true;
            return;
        }
        print("connected");
        _tryReceive = true;
    }
}
