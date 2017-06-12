using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPopup : MonoBehaviour,NetworkManager.NetworkMessageEventListenrer,PopupManager.PopupHide
{


    //-- 기본 정보 -----------------------------------------------------------------//

    [SerializeField]
    private List<GameObject> m_playerList = new List<GameObject>();

    [SerializeField]
    private Image m_readyButton = null;
    [SerializeField]
    private Image m_startButton = null;

    [SerializeField]
    private Sprite m_sprReady_normal = null;
    [SerializeField]
    private Sprite m_sprReady_setting = null;
    [SerializeField]
    private Sprite m_sprStart_normal = null;
    [SerializeField]
    private Sprite m_sprStart_setting = null;

    private int m_readyCount = 0;

    // 호스트 체크용
    private bool m_hostCheck = false;

    public AudioClip m_ready = null;
    public AudioClip m_gameStart = null;
    //------------------------------------------------------------------------------//

    void Start()
    {

        SoundManager.Instance().PlayBGM(1);
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);

        for (int i = 0; i < m_playerList.Count; i++)
        {
            GameObject player = m_playerList[i];
            //서버에서 체크하기 전에 세팅 금지
            if(i+2 == GameManager.Instance().PLAYER.NETWORK_INDEX)
            {
                // 나다
                Image wait = GetProfileWait(player);
                if (wait != null)
                    wait.enabled = false;


                GetProfileHide(player).enabled = false;
                GetProfileName(player).text = GameManager.Instance().PLAYER.USER_NAME;

                SetPlayerStatus(player, 0, GameManager.Instance().PLAYER.STATUS.STAT_SPEED);
                SetPlayerStatus(player, 1, GameManager.Instance().PLAYER.STATUS.STAT_POWER);
                SetPlayerStatus(player, 2, GameManager.Instance().PLAYER.STATUS.STAT_REPAIR);
            }
            else
            {
                Image wait = GetProfileWait(player);
                if (wait != null)
                    wait.enabled = true;
                GetProfileImage(player).color = Color.black;
                GetProfileHide(player).enabled = true;
                GetProfileName(player).enabled = false;

                SetPlayerStatus(player, 0, 0);
                SetPlayerStatus(player, 1, 0);
                SetPlayerStatus(player, 2, 0);
            }
        }

        SetPlayerStatus(m_playerList[3], 2, 0);

        //NETWORK INDEX 요청
        NetworkManager.Instance().SendOrderMessage(
            JSONMessageTool.ToJsonOrderRequest(
                NetworkOrderController.ORDER_NAME, NetworkOrderController.ORDER_SPACE));


        if (GameManager.Instance().PLAYER.NETWORK_INDEX == 2)
        {
            //host
            m_startButton.gameObject.SetActive(true);
            m_readyButton.gameObject.SetActive(false);
        }
        else if(GameManager.Instance().PLAYER.NETWORK_INDEX > 2)
        {
            //user
            m_startButton.gameObject.SetActive(false);
            m_readyButton.gameObject.SetActive(true);
        }
        else
        {
            //옵저버
            m_startButton.gameObject.SetActive(false);
            m_readyButton.gameObject.SetActive(false);
        }

    }

    //-- host 정보가 세팅 되었는지 확인하고 되었다면 접속 알림
    void Update()
    {
        if(!string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME) && !m_hostCheck)
        {
            //접속 되었음 !
            m_hostCheck = true;
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderUserEnter(
                GameManager.Instance().PLAYER.NETWORK_INDEX,
                GameManager.Instance().PLAYER.STATUS,
                GameManager.Instance().PLAYER.USER_NAME, false));
        }
    }

    //-- Player Setup Func --------------------------------------------------------//

    private void SetPlayerStatus(GameObject player, int index, int point)
    {
        List<Image> list = new List<Image>();
        string name = "";
        switch (index)
        {
            case 0: name = "SPEEDLIST"; break;
            case 1: name = "POWERLIST"; break;
            case 2: name = "REPAIRLIST"; break;
        }
        if (string.IsNullOrEmpty(name))
            return;
        Transform t = player.transform.FindChild("Status/" + name);

        for (int i = 0; i < t.childCount; i++)
        {
            Image img = t.GetChild(i).GetComponent<Image>();

            if (i < point)
                img.enabled = true;
            else
                img.enabled = false;
        }
        
    }

    private Image GetProfileWait(GameObject player)
    {
        if (player.transform.FindChild("Wait") == null)
            return null;
        return player.transform.FindChild("Wait").GetComponent<Image>();
    }
    private Text GetProfileName(GameObject player)
    {
        return player.transform.FindChild("Profile/ProfileName").GetComponent<Text>();
    }
    private Image GetProfileImage(GameObject player)
    {
        return player.transform.FindChild("Profile/ProfileImg").GetComponent<Image>();
    }
    private Image GetProfileHide(GameObject player)
    {
        return player.transform.FindChild("Profile/Hide").GetComponent<Image>();
    }
    private GameObject GetRush(GameObject player)
    {
        return player.transform.FindChild("Rush").gameObject;
    }
    private Text GetRushName(GameObject player)
    {
        return player.transform.FindChild("Rush/ProfileName").GetComponent<Text>();
    }

    //Ready
    public void ReadyButton()
    {
        if (m_readyButton.sprite == m_sprReady_normal)
        {
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonOrderUserReady(true,GameManager.Instance().PLAYER.NETWORK_INDEX));
            m_readyButton.sprite = m_sprReady_setting;

            GetRush(m_playerList[GameManager.Instance().PLAYER.NETWORK_INDEX-1]).SetActive(true);
            GetRushName(m_playerList[GameManager.Instance().PLAYER.NETWORK_INDEX-1]).text = 
                GameManager.Instance().PLAYER.USER_NAME;
        }
        else
        {
            GetRush(m_playerList[GameManager.Instance().PLAYER.NETWORK_INDEX-1]).SetActive(false);
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonOrderUserReady(false, GameManager.Instance().PLAYER.NETWORK_INDEX));
            m_readyButton.sprite = m_sprReady_normal;
        }
    }

    //Start
    public void StartButton()
    {
      //  if(m_startButton.sprite == m_sprStart_setting)
        {
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderGameSatart());
        }

    }

    // 다른 사람 정보

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(
        NetworkManager.MessageEvent e)
    {
        //연결된게 아닌 경우 / 
        if (e.msgType != NetworkManager.USER_CONNECT)
        {
            // 다른사람이 내게 요청을 했다. 나의 정보를 
            if (e.msgType == NetworkManager.USER_INFO_REQ)
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonOrderUserEnter(
                        GameManager.Instance().PLAYER.NETWORK_INDEX,
                        GameManager.Instance().PLAYER.STATUS,
                        GameManager.Instance().PLAYER.USER_NAME,
                        m_readyButton.sprite == m_sprReady_setting));
            //유저가 나갔다
            else if(e.msgType == NetworkManager.USER_LOGOUT)
            {
                // 로그아웃
                GameObject user = m_playerList[(int)e.msg
                    .GetField(NetworkManager.USER_LOGOUT)
                    .GetField(NetworkManager.USER_LOGOUT).i - 1];
                if (GetProfileWait(user) != null)
                    GetProfileWait(user).enabled = true;
                GetProfileHide(user).enabled = true;
                GetProfileName(user).enabled = false;
                GetProfileImage(user).color = Color.black;
                SetPlayerStatus(user, 0, 0);
                SetPlayerStatus(user, 1, 0);
                SetPlayerStatus(user, 2, 0);

                // 레디 정보를 받아와야함
                if (e.msg.GetField(NetworkManager.USER_LOGOUT)
                    .GetField(NetworkManager.READY_STATE).b)
                {
                    m_readyCount--;
                    GetRush(m_playerList[(int)e.orders.GetField(NetworkManager.MSG)
                        .GetField(NetworkManager.USER_LOGOUT)
                        .GetField(NetworkManager.USER_INDEX).i-1]).SetActive(false);
                }

                if (m_readyCount >= 3)
                {
                    m_startButton.sprite = m_sprStart_setting;
                }
                else
                    m_startButton.sprite = m_sprStart_normal;
            }
            else if(e.msgType == NetworkManager.USER_READY)
            {
                //레디
                bool b = e.msg.GetField(NetworkManager.USER_READY)
                    .GetField(NetworkManager.USER_READY).b;

                if (b)
                {
                    GetRushName(
                        m_playerList[(int)e.msg
                                        .GetField(NetworkManager.USER_READY)
                                        .GetField(NetworkManager.USER_INDEX).i-1])
                                            .text = e.targetName;
                    GetRush(m_playerList[(int)e.msg
                        .GetField(NetworkManager.USER_READY)
                        .GetField(NetworkManager.USER_INDEX).i-1]).SetActive(true);
                    m_readyCount++;

                    SoundManager.Instance().PlaySound(m_ready);
                }
                else
                {
                    GetRush(m_playerList[(int)e.msg
                        .GetField(NetworkManager.USER_READY).GetField(NetworkManager.USER_INDEX).i-1])
                        .SetActive(false);
                    m_readyCount--;
                }
                if (m_readyCount >= 3)
                {
                    m_startButton.sprite = m_sprStart_setting;
                }
                else
                    m_startButton.sprite = m_sprStart_normal;

            }
            else if(e.msgType == NetworkManager.GAME_START)
            {
                SoundManager.Instance().PlaySound(m_gameStart);

                // 여기서 게임 시작 처리 
                List<string> users = new List<string>();
                for (int i = 0; i < m_playerList.Count; i++)
                {
                    MDebug.Log(" " + i + " " +GetProfileName(m_playerList[i]).text);
                    if (string.IsNullOrEmpty(GetProfileName(m_playerList[i]).text))
                        continue;
                    users.Add(GetProfileName(m_playerList[i]).text);
                }
                GameManager.Instance().HudSetup(users);

                NetworkManager.Instance().RemoveNetworkOrderMessageEventListener(this);
                NetworkManager.Instance().GameStart();
                
                
                PopupManager.Instance().ClosePopup(gameObject);
            }
            return;
        }
        //##########################################################################################
        //#### 다른 유저의 정보가 넘어왔으니 세팅 ##################################################


        int index = (int)e.msg
            .GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.USER_CONNECT).i;
        
        // 나자신을 세팅할 필요는 없음
        if (index == GameManager.Instance().PLAYER.NETWORK_INDEX)
            return;

        if (index - 2 < 0)
            return;
        
        GameObject player = m_playerList[index-2];

        if (player == null)
            return;

        // 이미 세팅되어있다면 패스
        if (GetProfileName(player).text == e.msg
            .GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.CLIENT_ID).str)
        {
            return;
        }
        else
        {
            // 정보를 던져줘야함 
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderUserEnter(
                GameManager.Instance().PLAYER.NETWORK_INDEX,
                GameManager.Instance().PLAYER.STATUS,
                GameManager.Instance().PLAYER.USER_NAME, false));
        }

        if (GetProfileWait(player) != null)
            GetProfileWait(player).enabled = false;
        GetProfileHide(player).enabled = false;
        GetProfileName(player).enabled = true;
        GetProfileImage(player).color = Color.white;
        GetProfileName(player).text = e.orders.GetField(NetworkManager.MSG)
            .GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.CLIENT_ID).str;

        SetPlayerStatus(player, 0, (int)e.msg.GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.STATUS_SPEED).i);
        SetPlayerStatus(player, 1, (int)e.msg.GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.STATUS_POWER).i);
        SetPlayerStatus(player, 2, (int)e.msg.GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.STATUS_REPAIR).i);

        try
        {
            GetRush(player).SetActive(e.orders.GetField(NetworkManager.MSG).GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.READY_STATE).b);
            GetRushName(player).text = e.orders.GetField(NetworkManager.MSG).GetField(NetworkManager.USER_CONNECT).GetField(NetworkManager.CLIENT_ID).str;
        }catch(Exception)
        {

        }
        

    }
    
    
    // 접속 끊기 이벤트
    void OnApplicationQuit()
    {
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderUserLogOut(
            GameManager.Instance().PLAYER.NETWORK_INDEX,
            GameManager.Instance().PLAYER.STATUS,
            GameManager.Instance().PLAYER.USER_NAME,
            m_readyButton.sprite == m_sprReady_setting));
    }

    public void HideEndEvent()
    {
        GameManager.Instance().ChangeScene(GameManager.PLACE.ROBO_IN);
    }
}
