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
    public const string ORDERS = "orders";
    public const string ORDER = "order";
    public const string MSG = "msg";
    public const string CLIENT_ID = "clientid";
    public const string TIMESTAMP = "timestamp";
    public const string DIR = "Dir";
    public const string DIRVECTOR = "DirVector";

    // order
    public const string CHAT = "chat";
    public const string CREATE = "create";
    public const string REMOVE = "remove";
    public const string CH_ORIGINUSER = "ch_OriginUser";
    public const string CH_ORIGINUSER_REQ = "ch_OriginUser_req";
    public const string ANIMATION = "anmation";
    public const string STATE_CHANGE = "state_change";
    public const string SOCKET_OPEN = "socket_open";
    public const string GUN_ANGLE_CHANGE = "gun_angle_change";

    public const string BOSS_SCENE_MOVE = "boss_scene_move";

    //connect
    public const string USER_CONNECT = "user_connect";
    public const string USER_INFO_REQ = "userinfo_req";
    public const string USER_CHARACTER_CREATE = "user_char_create";
    public const string USER_LOGOUT = "user_logout";
    public const string USER_READY = "user_ready";
    public const string USER_INDEX = "user_index"; // ready
    public const string GAME_START = "gamestart";
    public const string READY_STATE = "ready_state";

    public const string STATUS_SPEED = "status_userspeed";
    public const string STATUS_POWER = "status_power";
    public const string STATUS_REPAIR = "status_repair";

    //충돌체크
    public const string CRASH = "crash";
    public const string CRASH_NAME1 = "crashname1";
    public const string CRASH_NAME2 = "crashname2";

    //place Change
    public const string PLACE_CHANGE = "place_change";
    //create order
    public const string CREATE_TARGET = "create_target";

    // robot order
    public const string ROBOT_DRIVER = "robot_driver";
    public const string ROBOT_GUNNER = "robot_gunner";


    // AI
    public const string AI = "ai"; // type
    public const string AI_C = "aiC";
    public const string AI_C_LASER = "aiClaser";
    public const string AI_D = "aiD";
    public const string AI_D_END = "aiDend";
    public const string AI_PATTERN_NAME = "ai_pattern_name";
    public const string AI_ANI_NAME = "ai_ani_name";
    public const string AI_ANI_INDEX = "ai_ani_index";
    public const string AI_ANI_LOOP = "ai_ani_loop";
    public const string AI_D_ROTATE = "ai_D_rotate";

    //ㄱㄱ
    public const string INTHE_STAR = "intherstar";


    public GameObject m_chatUI = null;

    // 우주에 있는 유저 리스트 ---------------------------------------------------------------//
    [SerializeField]
    private List<Hero> m_spaceUserList = new List<Hero>();

    // 로봇 안에 있는 유저 리스트------------------------------------------------------------//
    [SerializeField]
    private List<Hero> m_robotUserList = new List<Hero>();

    public List<GameObject> m_enemyList = new List<GameObject>();
    private List<string> m_userNameList = new List<string>(); // 플레이어 이름 리스트
    

    public List<string> USER_LIST
    {
        get { return m_userNameList; }
    }

    // -- 기본 정보 -------------------------------------------------------------------------------------------//

    private string m_serverURL = "localhost"; //"13.124.50.145:8090";

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
    private Queue<string> m_socketMoveMessage = new Queue<string>(1000);
    private Queue<string> m_socketEnemyMoveMessage = new Queue<string>();
    private Queue<string> m_socketOrders = new Queue<string>();
    //-- 옵저버 패턴 [채팅] -------------------------------------------------------------------------------------------//

    public class MessageEvent
    {
        public string msgType = "";
        public string user = "";
        public string targetName = "";
        public JSONObject msg = null;
        public JSONObject orders = null;

        public MessageEvent(string type, string user, string targetName, JSONObject orders, JSONObject msg)
        {
            this.msgType = type; this.user = user; this.msg = (msg);
            this.orders = orders;
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

        MessageEvent e = new 
            MessageEvent(obj.GetField(ORDERS)[0].GetField(ORDER).str,
            obj.GetField(CLIENT_ID).str,
            obj.GetField(ORDERS)[0].GetField(MSG).GetField(TARGETNAME).str,
            obj.GetField(ORDERS)[0], obj.GetField(ORDERS)[0].GetField(MSG));

        
        for (int i = m_socketListener.Count - 1; i >= 0; i--)
            m_socketListener[i].ReceiveNetworkMessage(e);

    }
    // ---------------------------------------------------------------------------------------------------------//
    // -- 옵저버 패턴 [오더] -----------------------------------------------------------------------------------//
    private List<NetworkMessageEventListenrer> m_orderList = new List<NetworkMessageEventListenrer>();

    // 이벤트 리스너 등록
    public void AddNetworkOrderMessageEventListener(NetworkMessageEventListenrer listener)
    {
        m_orderList.Add(listener);
    }
    // 이벤트 리스너 삭제
    public void RemoveNetworkOrderMessageEventListener(NetworkMessageEventListenrer listener)
    {
        m_orderList.Remove(listener);
    }

    // 커맨드 내용이 들어온다 ---------------------------------------------------!
    private void SendNetworkOrderMessage()
    {
        if (m_socketOrders.Count <= 0)
            return;

        JSONObject obj = new JSONObject(m_socketOrders.Dequeue());

        MessageEvent e = 
            new MessageEvent(obj.GetField(ORDERS)[0].GetField(ORDER).str, 
            obj.GetField(CLIENT_ID).str, 
            obj.GetField(ORDERS)[0].GetField(MSG).GetField(TARGETNAME).str, 
            obj.GetField(ORDERS)[0], obj.GetField(ORDERS)[0].GetField(MSG));
        
        for (int i = m_orderList.Count - 1; i >= 0; i--)
            m_orderList[i].ReceiveNetworkMessage(e);

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

    // 이동메시지를 쏜다 ------------------------------------------------------------!
    void SendMoveNetworkMessage()
    {
        if (m_socketMoveMessage.Count <= 0)
            return;


        string json = null;

        while(m_socketMoveMessage.Count > 0)
            json = m_socketMoveMessage.Dequeue();
        JSONObject obj = new JSONObject(json);

        if(obj.GetField("Users") == null)
        {
            MDebug.Log("Client Enter " + obj);
            // 임시코드 

            string t = "hero_robo";
            string a = "hero";
            GameManager.Instance().PLAYER.NETWORK_INDEX = (int)obj.GetField("Client ID").i;


            if (GameManager.Instance().PLAYER.NETWORK_INDEX <= 1)
            {
                NetworkOrderController.ORDER_NAME = GameManager.Instance().PLAYER.USER_NAME;
                NetworkOrderController.ORDER_SPACE = 0;

            }
            else if(GameManager.Instance().PLAYER.NETWORK_INDEX > 1)
            {
               // NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderRequest(NetworkOrderController.ORDER_NAME, NetworkOrderController.ORDER_SPACE));
            }
            return;
        }

        JSONObject users = obj.GetField("Users");
        //{"Users":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}



        //우주에 있는 유저놈들
        //로봇에 있는 유저놈들
        //행서에 있는 유저놈들
        //동일 로직?

        //확실한 것은 받아오는 리스트엔 모든 유저가 다 있다.
        // 이름 구분 방법 : 이름_space 이름_robot 이름_행성이름
      

        // 들어와있는 것들은 켠다.
        // 다만 위치를 판단해서 켜준다.

        foreach(Hero hero in m_spaceUserList)
        {
            for(int i = 0; i < users.Count; i ++)
            {
                if(users[i].GetField(USERNAME).str == hero.USERNAME)
                {

                    int area = (int)users[i].GetField("Z").f;
                    
                    if (area == (int)NetworkOrderController.AreaInfo.AREA_SPACE 
                        && m_userNameList.Contains(hero.USERNAME.Split('_')[0]))
                        hero.gameObject.SetActive(true);
                    else
                        hero.gameObject.SetActive(false);
                }
            }
        }

        //여긴 로봇 :: 아마 부하 없을거
        foreach (Hero hero in m_robotUserList)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].GetField(USERNAME).str == hero.USERNAME)
                {

                    int area = (int)users[i].GetField("Z").f;
                    if (area == (int)NetworkOrderController.AreaInfo.AREA_ROBOT
                        && m_userNameList.Contains(hero.USERNAME.Split('_')[0]))
                        hero.gameObject.SetActive(true);
                    else
                        hero.gameObject.SetActive(false);
                }
            }
        }
        

        for (int i = m_moveEventList.Count - 1; i >= 0; i--)
            m_moveEventList[i].ReceiveMoveEvent(obj);
    }

    public void GameStartUserSetup(string name)
    {
        m_userNameList.Add(name);
        m_robotUserList[0].USERNAME = name + "_robo";
        m_robotUserList[0].gameObject.SetActive(true);
    }

    public void LogOutUser(string name)
    {
        if(m_userNameList.Contains(name))
        {
            foreach(Hero user in m_spaceUserList)
            {
                if (user.USERNAME == name + "_space")
                {
                    user.USERNAME = "";
                    user.gameObject.SetActive(false);
                }
            }
            foreach (Hero user in m_robotUserList)
            {
                if (user.USERNAME == name + "_robo")
                {
                    user.USERNAME = "";
                    user.gameObject.SetActive(false);
                }
            }
        }
    }

    // 캐릭터 생성
    public void CreateUserCharacter(string name)
    {
        if(!m_userNameList.Contains(name))
        {
            //TODO 추후 여기서 name split 해서 캐릭터 생성 
            foreach(Hero user in m_robotUserList)
            {
                if(string.IsNullOrEmpty(user.USERNAME) || !m_userNameList.Contains(user.USERNAME.Split('_')[0]))
                {
                    if(!user.m_isMe)
                    {
                        m_userNameList.Add(name);
                        user.USERNAME = name + "_robo";
                        user.gameObject.SetActive(true);
                        return;
                    }

                }
            }
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
        string json = null;

        while(m_socketEnemyMoveMessage.Count > 0)
            json = m_socketEnemyMoveMessage.Dequeue();
        JSONObject obj = new JSONObject(json);
        JSONObject users = obj.GetField("Enemies");
    
        for (int i = m_enemyMoveEventList.Count - 1; i >= 0; i--)
            m_enemyMoveEventList[i].ReceiveMoveEvent(obj);

    }
    // ---------------------------------------------------------------------------------------------------------//

    // 들어온 메시지 전송 각
    void Update()
    {
        SendNetworkChatMessage();
        SendMoveNetworkMessage();
        SendEnemyMoveNetworkMessage();
        SendNetworkOrderMessage();
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
        m_socket.SetupSocket(m_serverURL + ":8090");
    }
    public void SendNetworkMessage(string json)
    {
        MDebug.Log(json);
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

    public void SendOrderMessage(string json)
    {
        m_socket.SendOrderMessage(json);
    }

    public bool IsSocketAlived()
    {
        return m_socket.IsSocketAlived();
    }

    // 큐에 메시지를 담는다.
    public void PushChatMessage(string json)
    {
        this.m_socketMessages.Enqueue(json);
    }

    // 무브 이벤트 정보만 담는다.
    public void PushMoveMessage(string json)
    {
        if(m_socketMoveMessage.Count < 1000)
        this.m_socketMoveMessage.Enqueue(json);
    }

    // 적 무브 이벤트 정보만 담는다.
    public void PushEnemyMoveMessage(string json)
    {
        this.m_socketEnemyMoveMessage.Enqueue(json);
    }

    // 오더를 큐에 담는다
    public void PushOrderMessage(string json)
    {
        this.m_socketOrders.Enqueue(json);
    }


    // -- 게임 제어 ------------------------------------------------------//
    public void GameStart()
    {
        // 로그인 / 로비 세팅 완료되었다.
        m_robotUserList[0].gameObject.SetActive(true);
        CameraManager.Instance().MoveCamera(m_robotUserList[0].transform.parent.gameObject,GameSetting.CAMERA_ROBO,CameraManager.CAMERA_PLACE.ROBO_IN);
        
        GameStartUserSetup(GameManager.Instance().PLAYER.USER_NAME);

        SendOrderMessage(JSONMessageTool.ToJsonOrderUserCrateCharacter(
            GameManager.Instance().PLAYER.USER_NAME));
        SoundManager.Instance().PlayBGM(2);
        m_chatUI.SetActive(true);
    }

    public void GototheRobo()
    {
        CameraManager.Instance().MoveCamera(m_robotUserList[0].gameObject, GameSetting.CAMERA_ROBO, CameraManager.CAMERA_PLACE.ROBO_IN);        
    }

    void Start()
    { 
    }
 
}
