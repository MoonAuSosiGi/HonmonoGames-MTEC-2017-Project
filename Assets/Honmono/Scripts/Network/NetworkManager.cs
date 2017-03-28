using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class NetworkManager : Singletone<NetworkManager> {

    // -- 기본 json 태그 --------------------------------------------------------------------------------------//
    public const string MSGTYPE = "MsgType";
    public const string USERNAME = "UserName";
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

    public string SERVER_URL
    {
        get { return m_serverURL; }
        set { m_serverURL = value; }
    }
 
    
    // -- 메시지 큐 -------------------------------------------------------------------------------------------//
    private Queue<string> m_socketMessages = new Queue<string>();
    private Queue<string> m_socketMoveMessage = new Queue<string>();
    //-- 옵저버 패턴 [채팅] -------------------------------------------------------------------------------------------//

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
    private void SendNetworkChatMessage()
    {
        if (m_socketMessages.Count <= 0)
            return;

        JSONObject obj = new JSONObject(m_socketMessages.Dequeue());

        MessageEvent e = new MessageEvent(obj.GetField(MSGTYPE).str, obj.GetField(USERNAME).str, obj.GetField(MSG));

        foreach(NetworkMessageEventListenrer listener in m_socketListener)
        {
            listener.ReceiveNetworkMessage(e);
        }

    }
    // -- 옵저버 패턴 [무브] -----------------------------------------------------------------------------------//
    List<NetworkMoveEventListener> m_moveEventList = new List<NetworkMoveEventListener>();
    public interface NetworkMoveEventListener
    {
        void ReceiveMoveEvent(string json);
    }

    public void AddNetworkMoveEventListener(NetworkMoveEventListener l)
    {
        m_moveEventList.Add(l);
    }

    void SendMoveNetworkMessage()
    {
        if (m_socketMoveMessage.Count <= 0)
            return;

        string json = m_socketMoveMessage.Dequeue();

        //{"Users":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}


        
        foreach (NetworkMoveEventListener l in m_moveEventList)
        {
            l.ReceiveMoveEvent(json);
        }
    }

    // ---------------------------------------------------------------------------------------------------------//

    void Start()
    {
      //  SetupWebSocket();
    }

    void Update()
    {
        SendNetworkChatMessage();
        SendMoveNetworkMessage();
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
        m_socket.SendChatMessage(json);
    }

    public void SendMoveMessage(string json)
    {
        m_socket.SendMoveMessage(json);
    }


    // 큐에 메시지를 담는다.
    public void PushChatMessage(string json)
    {
        this.m_socketMessages.Enqueue(json);
    }

    // 무브 이벤트 정보만 담는다.
    public void PushMoveMessage(string json)
    {
        this.m_socketMoveMessage.Enqueue(json);   
    }
}
