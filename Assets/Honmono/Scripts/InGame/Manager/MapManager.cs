using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;


// MapManager 하위에 map 의 prefab을 가져온다.

public class MapManager : Singletone<MapManager> {
    //---------------------------------------//
    // background 나가는 체크용

    // 배경 밖으로 나갈 수 없게 처리
    [SerializeField]
    private GameObject m_limitBackground = null;

    float m_cameraHalfWdith = 0.0f;
    float m_cameraHalfHeight = 0.0f;

    float m_backgroundHalfWidth = 0.0f;
    float m_backgroundHalfHeight = 0.0f;

    //-----------------------------------------------------------------------------------------//
    // Object Pool

    // 유저들 풀 - 임시로 게임 오브젝트를 받음 - 플레이어는 여기에 속해있다.
    [SerializeField]
    private List<Hero> m_users = new List<Hero>();

    private List<USER_UPDATE> m_usersEnabled = new List<USER_UPDATE>();

    //temp

   struct USER_UPDATE
    {
        public string name;
        public Vector3 pos;
        public USER_UPDATE(string str,Vector3 p) { name = str;  pos = p; }
    }


    //----------------------------------------------------------------------------------------//
    // 바깥에서 사용할 GET
    public float CAMERA_HALF_WIDTH { get { return this.m_cameraHalfWdith; } }
    public float CAMERA_HALF_HEIGHT { get { return this.m_cameraHalfHeight; } }
    public float BACKGROUND_HALF_WIDTH { get { return this.m_backgroundHalfWidth; } }
    public float BACKGROUND_HALF_HEIGHT { get { return this.m_backgroundHalfHeight; } }

    public Vector3 BACKGROUND_POS { get { return m_limitBackground.transform.position; } }


   //----------------------------------------------------------------------------------------//

    void Start()
    {
        // camera
        this.m_cameraHalfWdith = (Camera.main.orthographicSize * Screen.width / Screen.height);
        this.m_cameraHalfHeight = (this.m_cameraHalfWdith * Screen.height / Screen.width);
        NetworkManager.Instance().SetupWebSocketMove();
        NetworkManager.Instance().SetMoveRecv(ReceieveMove);

        //temp 

    }

    //prefab을 부르는 마법의 함수
    public void SetupMap(string prefabPath)
    {
        // TODO prefab 생성

        // 리밋 배경 가져오기

        // 리밋 배경을 통한 영역 세팅
        this.m_backgroundHalfWidth = this.m_limitBackground.GetComponent<SpriteRenderer>().bounds.size.x / 2.0f;
        this.m_backgroundHalfHeight = this.m_limitBackground.GetComponent<SpriteRenderer>().bounds.size.y / 2.0f;
    }

    public void SetupUsers()
    {
        // TODO...
    }

    void Update()
    {
        for(int i =0; i < m_users.Count;i++)
        {
            if (m_users[i].m_isMe)
                continue;

            for (int j = 0; j < m_usersEnabled.Count; j++)
            {
                if(m_users[i].USERNAME == m_usersEnabled[j].name)
                {
                    m_users[i].transform.position = m_usersEnabled[j].pos;
                    m_users[i].GetComponent<SpriteRenderer>().enabled = true;
                    m_users[i].enabled = true;
                    m_users[i].GetComponent<Animator>().enabled = true;
                }
                else
                {
                    m_users[i].GetComponent<SpriteRenderer>().enabled = false;
                    m_users[i].enabled = false;
                    m_users[i].GetComponent<Animator>().enabled = false;
                }
            }
            
            
        }
    }
    //Character Position Receieve
    void ReceieveMove(object sender, MessageEventArgs e)
    {
        MDebug.Log(e.Data);
        JSONObject obj = new JSONObject(e.Data);
        MDebug.Log(obj.IsArray);

        JSONObject users = obj.GetField("Users");

        if(users.IsArray)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (m_users[i].m_isMe)
                    continue;
                
                for(int j = 0; j < users.Count;j++)
                {
                    string userName = users[j].GetField("UserName").str;
                    float x = users[j].GetField("x").f;
                    float y = users[j].GetField("y").f;
                    if (userName == "")
                        continue;
                    m_usersEnabled.Add(new USER_UPDATE(userName, new Vector3(x, y)));

                    MDebug.Log("user " + userName + " x " + x + " y " + y);
                }
                
                

            }
        }
    }

}
