using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class NetworkManager : Singletone<NetworkManager> {

    // -- 기본 json 태그 --------------------------------------------------------------------------------------//
    public const string MSGTYPE = "MsgType";
    public const string USER = "User";
    public const string MSG = "Msg";

    // 상태값 
    public const string CHAT = "chat";
    public const string MOVE = "move";
    public const string DIR = "dir";

    // -- 기본 정보 -------------------------------------------------------------------------------------------//

    private string m_serverURL = "localhost:8080";

    [SerializeField]
    private RestController m_rest = null;
    [SerializeField]
    private WebSocketController m_socket = null;
 
    
    // -- 메시지 큐 -------------------------------------------------------------------------------------------//
    private Queue<string> m_socketMessages = new Queue<string>();
    //-- 옵저버 패턴-------------------------------------------------------------------------------------------//

    public class MessageEvent
    {
        public string msgType = "";
        public string user = "";
        public JSONObject msg = null;

        public MessageEvent(string type, string user, JSONObject msg)
        {
            this.msgType = type; this.user = user; this.msg = msg;
        }
    }

    public interface NetworkMessageEventListenrer
    {
        void ReceiveNetworkMessage(MessageEvent e);
    }

    private List<NetworkMessageEventListenrer> m_socketListener = new List<NetworkMessageEventListenrer>();

    // 이벤트 리스너 등록
    public void AddNetworkMessageEventListener(NetworkMessageEventListenrer listener)
    {
        m_socketListener.Add(listener);
    }
    // 이벤트 리스너 삭제
    public void RemoveNetworkMessageEventListener(NetworkMessageEventListenrer listener)
    {
        m_socketListener.Remove(listener);
    }

    // 메시지 일괄 전송
    private void SendNetworkMessage()
    {
        if (m_socketMessages.Count <= 0)
            return;

        JSONObject obj = new JSONObject(m_socketMessages.Dequeue());

        MessageEvent e = null; 
        //// 이전 서버 제어용
        //if (!obj.HasField(MSGTYPE))
        //{
        //    e = new MessageEvent(MOVE, obj.GetField("UserName").str, obj);
        //}
        //else
            e = new MessageEvent(obj.GetField(MSGTYPE).str, obj.GetField(USER).str, obj.GetField(MSG));

        foreach(NetworkMessageEventListenrer listener in m_socketListener)
        {
            listener.ReceiveNetworkMessage(e);
        }

    }

    void Start()
    {
        SetupWebSocket();
    }

    void Update()
    {
        SendNetworkMessage();
    }
    
    //-- REST ------------------------------------------------------------------------------------------------//
    public void Login(string id,string pwd, GameObject target, string targetFunc)
    {
        if (m_rest != null)
            m_rest.Login(id, pwd, target, targetFunc);
    }

    // WebSocket ---------------------------------------------------------------------------------------------//
    public void SetupWebSocket()
    {
        m_socket.SetupSocket(m_serverURL);
    }
    public void SendNetworkMessage(string json)
    {
        m_socket.SendNetworkMessage(json);
    }

    // 큐에 메시지를 담는다.
    public void PushSocketMessage(string json)
    {
        // 무브 테스트용ㅇ
        //JSONObject j = new JSONObject(json);
        //JSONObject users = j.GetField("Users");

        //for(int i = 0; i < users.Count; i++)
        //this.m_socketMessages.Enqueue(users[i].ToString());


        this.m_socketMessages.Enqueue(json);
    }
}
