using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;


// MapManager 하위에 map 의 prefab을 가져온다.

public class MapManager : Singletone<MapManager> {
    //------------------------------------------------------------------------------------------//
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

    // 기타 오브젝트 리스트
    // 플레이어를 제외한 오브젝트의 리스트가 여기에 들어온다.
    // (총알도 제외한 리스트)
    [SerializeField]
    private List<GameObject> m_objectList = new List<GameObject>();

    public HeroRobo m_robo = null;
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

        GameManager.Instance().PLAYER.PLAYER_HERO = m_users[0];
        GameManager.Instance().ROBO = m_robo;
        //temp 

    }

    public void GameStart()
    {

        // 더이상 여기서 플레이어를 처리하지 않음
        //m_users[0].gameObject.SetActive(true);

        //m_robo.gameObject.SetActive(true);

        //CameraManager.Instance().MoveCamera(m_robo.gameObject);
        
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


    // -- 오브젝트 생성 -------------------------------------------------------------------------------------------//

    public GameObject AddObject(string prefabPath)
    {
        // prefab 주소를 넘기면 자동으로 불러옴

        if (string.IsNullOrEmpty(prefabPath))
            return null;

        GameObject prefab = Resources.Load(prefabPath) as GameObject;
        GameObject obj = GameObject.Instantiate(prefab);
        obj.transform.parent = transform;

        this.m_objectList.Add(obj);
        return obj;
    }

    // -- 적 생성 ----------------------------------------------------------------------------------------------- //


    public GameObject AddMonster(string prefabPath,string name,Vector3 pos)
    {
        GameObject monster = AddObject(prefabPath);

        if(monster != null)
        {
            if(prefabPath.Equals(GamePath.BOSS1))
            { 
                monster.transform.GetChild(0).GetComponent<Stage1BOSS>().MONSTER_NAME = name;
                monster.transform.position = pos;
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(name,"boss1",pos.x,pos.y));
            }
        }
        return monster;
    }
}
