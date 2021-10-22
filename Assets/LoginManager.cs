using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoginManager : MonoBehaviour {
    [SerializeField] public GameObject server;

    private ServerHandler _serverMan;
    // Start is called before the first frame update
    void Start()
    {
        if (server) {
            _serverMan = server.GetComponent<ServerHandler>();
        }
        else {
            print("HUGE ERROR! Failed to get sever. Please set in the Unity Editor!");
        }

        var input = GetComponent<TMP_InputField>();
        input.onSubmit.AddListener(OnSubmit);
        EventSystem.current.SetSelectedGameObject(gameObject,null);
        input.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private void OnSubmit(string str) {
        print($"On Login Submit: {str}");
        //_serverMan.tryConnect = true;
        _serverMan.connectAs = str;
        //transform.parent.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
