using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class HistoryManager : MonoBehaviour {
    // Start is called before the first frame update
    public Object textPrefab;
    private TMP_InputField _inputField;
    private ScrollRect _rect;
    private Scrollbar _scrollbar;

    private ServerHandler server;
    void Start() {
        //_textPrefab = AssetDatabase.LoadAssetAtPath<Object>("Assets/HistoryEntry.prefab");
        
        //listen to the Text Input
        var input = GameObject.Find("User Input");
        if (input == null) {
            print("Failed to get the object 'User Input'");
            return;
        }
        _inputField = input.GetComponent<TMP_InputField>();
        if (_inputField == null) {
            print("Failed to get 'User Input''s Text Input component");
            return;
        }
        _inputField.onSubmit.AddListener(AddTextEntrySend);

        var scrollObj = GameObject.Find("Scrollbar Vertical");
        if (scrollObj == null) {
            print("Failed to get the scrollbar");
            return;
        }
        //_scrollbar = scrollObj.GetComponent<Scrollbar>();
        //_scrollbar.on;
        var serverObj = GameObject.Find("Server");
        if (serverObj == null) {
            print("Failed to get the server gameobject");
            return;
        }

        server = serverObj.GetComponent<ServerHandler>();
    }

    // Update is called once per frame
    void Update() {

    }

    private void ParseString(string text){
       //This function generates a byte array that will be sent to the server. 
    }
    
    private void AddTextEntrySend(string text)
    {
        
        if (text.Equals("")) {
            return;
        }

        server.Send(text);
        if (text == "quit") {
            server.tryQuit = true;
            //server.tryReceive = false;
        }
        if (text == "join") {
            server.tryReceive = true;
            server.tryQuit = false;
        }
        text = text.Insert(0, "> ");
        AddTextEntry(text);
        //highlight text box
        EventSystem.current.SetSelectedGameObject(_inputField.gameObject,null);
        _inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        _inputField.text = "";
    }
    public void AddTextEntry(string text) {
        //var obj = PrefabUtility.InstantiatePrefab(textPrefab) as GameObject;
        var obj = Instantiate(textPrefab) as GameObject;
        if (obj == null) {
            print("Failed to create prefab");
            return;
        }
        var textComp = obj.GetComponent<TextMeshProUGUI>();
        if (textComp == null) {
            print("Object doesn't have this component");
            return;
        }
        textComp.text = text;
        //get text height
        
        //become child
        obj.transform.SetParent(transform.GetChild(0).GetChild(0).transform,false);
    }
}