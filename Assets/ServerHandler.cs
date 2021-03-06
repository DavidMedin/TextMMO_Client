using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

class OnlyFirst {
    private bool _locked = false;
    public void Lock(Action onFirst) {
        if (!_locked) {
            onFirst();
            _locked = true;
        }
    }

    public void Unlock() {
        _locked = false;
    }
}

public enum MsgHeader
{
    msg=1,
    usr_err,
    login
}
public class ServerHandler : MonoBehaviour {
    private TcpClient _client;
    private NetworkStream _stream;

    private byte[] receivedData = new byte[256];
    private bool _tryConnect = true;

    private string _connectAs;
    public string connectAs {
        get =>  _connectAs;
        set {
            _connectAs = value;
            if (_client.Connected) {
                Send(_connectAs);
            }
        }
    }
    
    //Message Handlers
    private Action<string> _msgHndl;

    public bool tryReceive = false;
    public bool tryQuit = false;

    //private bool _loggedIn = false;
    
    [SerializeField] public string address = "138.247.108.215";

    private HistoryManager _man;
    public GameObject loginMan;
    private OnlyFirst _msgLock = new OnlyFirst();
    public void Start() {
        var manObj = GameObject.Find("History");
        _man = manObj.GetComponent<HistoryManager>();
    }

    public void Update() {
        if (_tryConnect)
            Connect();
        if (tryReceive)
            Receive();
    }

    private async void Receive() {
        tryReceive = false;
        try {
            int read = await _stream.ReadAsync(receivedData, 0, 256);
            if (read == 0) {
                throw new IOException("read was zero");
            }
            string msg = Encoding.ASCII.GetString(receivedData, 0, read);
            if(!tryQuit)
                tryReceive = true;
            if(_man != null){
                print($"Found message ({(int)msg[0]}, {msg.Substring(1)}) L: {read}");
                switch ((MsgHeader)msg[0])
                {
                    case MsgHeader.msg:
                    {
                        //_man.AddTextEntry(msg.Substring(1));
                        if (_msgHndl != null)
                            _msgHndl(msg.Substring(1));
                        break;
                    }
                    case MsgHeader.usr_err:
                    {
                        _man.AddTextEntry($"<color=red>{msg.Substring(1)}</color>");
                        break;
                    }
                    case MsgHeader.login:
                    {
                        //_loggedIn = true;
                        loginMan.SetActive(false);
                        _msgHndl = _man.AddTextEntry;
                        break;
                    }
                    default:
                    {
                        print($"Oh no, recieved unkown header from Server! {msg[0]}");
                        break;
                    }
                }
            }
        }
        catch(IOException e) {
            print("caught exception -> " +  e.Message);
            _stream.Close();
            _client.Close();
            _tryConnect = true;
        }

    }

    public async void Send(string message) {
        if (_client is {Connected : true}) {
            try {
                await _stream.WriteAsync(Encoding.ASCII.GetBytes(message), 0, message.Length);
                print("sent item");
            }
            catch (Exception e) {
                print("Failed to send: " + e.Message);
            }

        }
    }

    public void OnApplicationQuit(){
        if(_client.Connected)
            Send("quit");
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
            await _client.ConnectAsync(address, 8080);
            _stream = _client.GetStream();
            //start receiving
            _msgLock.Unlock();
            if(_man != null){_man.AddTextEntry("<color=green>Connected</color>");} 
        }
        catch(Exception e) {
            print("failed to connect: "+e.Message );
            _msgLock.Lock(() => {
                if(_man != null) _man.AddTextEntry("<color=red>Disconnected</color>"); 
            });
            _tryConnect = true;
            return;
        }
        print("connected");
        if (connectAs != null) {
            Send(connectAs);
        }
        tryReceive = true;
    }
}
