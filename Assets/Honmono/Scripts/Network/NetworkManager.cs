using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using WebSocketSharp;

public class NetworkManager : Singletone<NetworkManager>
{

    // --  사용자 정의  --------------------------------------------------------------------------------------//
    public const string MSGTYPE = "msgType";
    public const string USERNAME = "UserName";
    public const string TARGETNAME = "targetName";
    public const string MSG = "msg";

    public const string DIR = "Dir";

    // order
    public const string CHAT = "chat";
    public const string CREATE = "create";
    public const string REMOVE = "remove";
    public const string CH_ORIGINUSER = "ch_OriginUser";
    public const string CH_ORIGINUSER_REQ = "ch_OriginUser_req";
    public const string ANIMATION = "anmation";

    //create order
    public const string CREATE_TARGET = "create_target";


    // hero list
    [SerializeField]
    private List<Hero> m_userList = new List<Hero>();
    public List<GameObject> m_enemyList = new List<GameObject>();
    private List<string> m_userNameList = new List<string>(); // 플레이어 제외하고는 최대 3명 
    private bool m_firstCheck = false;    
    // -- 기본 정보 -------------------------------------------------------------------------------------------//

    private string m_serverURL = "localhost:8090";

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
    private Queue<string> m_socketEnemyMoveMessage = new Queue<string>();
    //-- 옵저버 패턴 [채팅] -------------------------------------------------------------------------------------------//

    public class MessageEvent
    {
        public string msgType = "";
        public string user = "";
        public string targetName = "";
        public JSONObject msg = null;

        public MessageEvent(string type, string user, string targetName, JSONObject msg)
        {
            this.msgType = type; this.user = user; this.msg = (msg);
            this.targetName = targetName;
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

        MessageEvent e = new MessageEvent(obj.GetField(MSGTYPE).str, obj.GetField(USERNAME).str, obj.GetField(TARGETNAME).str, obj.GetField(MSG));

        foreach (NetworkMessageEventListenrer listener in m_socketListener)
        {
            listener.ReceiveNetworkMessage(e);
        }

    }
    // -- 옵저버 패턴 [무브] -----------------------------------------------------------------------------------//
    List<NetworkMoveEventListener> m_moveEventList = new List<NetworkMoveEventListener>();
    public interface NetworkMoveEventListener
    {
        void ReceiveMoveEvent(JSONObject json);
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
        JSONObject obj = new JSONObject(json);

        if(obj.GetField("Users") == null)
        {
            MDebug.Log("JSON " + obj);
            // 임시코드 

            string t = "hero_robo";
            string a = "hero";

            MDebug.Log(t == a);
            if (obj.GetField("Client ID").i <= 1)
            {
                NetworkManager.Instance().SendNetworkMessage(JSONMessageTool.ToJsonOrderChange(GameManager.Instance().PLAYER.USER_NAME, 0));
                
            }
            else if(obj.GetField("Client ID").i > 1)
            {
                MDebug.Log("전~~~송~~!!!요청");
                NetworkManager.Instance().SendNetworkMessage(JSONMessageTool.ToJsonOrderRequest(NetworkOrderController.ORDER_NAME, NetworkOrderController.ORDER_SPACE));
            }
            return;
        }

        JSONObject users = obj.GetField("Users");
        //{"Users":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}

        

        //변경
        int userCount = 0;
        //user
        //최대 접속자는 4명 그 중에 Hero
        
        foreach (Hero user in m_userList)
        {
            //켜져있다면 체크해야함
            if (user.gameObject.activeSelf)
            {
                bool check = false;
                for (int j = 0; j < users.Count; j++)
                {
                    string name = users[j].GetField(USERNAME).str;
                    
                    if(name.IndexOf("robo") >= 0)
                        continue;
                    if (name == GameManager.Instance().PLAYER.USER_NAME)
                        continue;
                    if (user.USERNAME == name)
                    {
                        check = true;
                        break;
                    }
                }

                if (!check)
                {
                    // UserName List 에서 삭제 
                    m_userNameList.Remove(user.USERNAME);
                    user.gameObject.SetActive(false);
                }

            }
            else
            {
                // 꺼져있다면 추가해야 하므로 체크 로직 
                for (int j = 0; j < users.Count; j++)
                {
                    bool check = false;
                    string name = users[j].GetField(USERNAME).str;
                    
                    if (name.IndexOf("robo") >= 0)
                        continue;
                    if (name == GameManager.Instance().PLAYER.USER_NAME)
                        continue;

                    for (int k = 0; k < m_userNameList.Count; k++)
                    {
                        if (m_userNameList[k] == name)
                        {
                            check = true;
                            break;
                        }
                    }

                    if (!check)
                    {
                        
                        m_userNameList.Add(name);
                        user.gameObject.SetActive(true);
                        user.USERNAME = name;
                        break;
                    }
                }
            }


        }

       

        foreach (NetworkMoveEventListener l in m_moveEventList)
        {
            l.ReceiveMoveEvent(obj);
        }

    }
    // ---------------------------------------------------------------------------------------------------------//

    // -- 옵저버 패턴 [적 무브] -----------------------------------------------------------------------------------//
    List<NetworkMoveEventListener> m_enemyMoveEventList = new List<NetworkMoveEventListener>();


    public void AddNetworkEnemyMoveEventListener(NetworkMoveEventListener l)
    {
        m_enemyMoveEventList.Add(l);
    }
    public void RemoveNetworkEnemyMoveEventListener(NetworkMoveEventListener l)
    {
        m_enemyMoveEventList.Remove(l);
    }

    void SendEnemyMoveNetworkMessage()
    {
        if (m_socketEnemyMoveMessage.Count <= 0)
            return;

        string json = m_socketEnemyMoveMessage.Dequeue();
        JSONObject obj = new JSONObject(json);
        JSONObject users = obj.GetField("Enemies");

        foreach (NetworkMoveEventListener l in m_enemyMoveEventList)
        {
            l.ReceiveMoveEvent(obj);
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
        SendEnemyMoveNetworkMessage();
    }

    //-- REST ------------------------------------------------------------------------------------------------//
    public void Login(string id, string pwd, GameObject target, string targetFunc)
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

    public void SendEnemyMoveMessage(string json)
    {
        m_socket.SendEnemyMoveMessage(json);
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

    // 적 무브 이벤트 정보만 담는다.
    public void PushEnemyMoveMessage(string json)
    {
        this.m_socketEnemyMoveMessage.Enqueue(json);
    }
}
