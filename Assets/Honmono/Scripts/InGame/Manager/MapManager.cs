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


  

    // -- 오브젝트 생성 -------------------------------------------------------------------------------------------//

    public GameObject AddObject(string prefabPath,Vector2 pos)
    {
        // prefab 주소를 넘기면 자동으로 불러옴

        if (string.IsNullOrEmpty(prefabPath))
            return null;

        GameObject prefab = Resources.Load(prefabPath) as GameObject;
        GameObject obj = GameObject.Instantiate(prefab);
        obj.transform.parent = transform;
        obj.transform.position = pos;

        this.m_objectList.Add(obj);
        return obj;
    }

    // 게임 오브젝트 넣기
    public void AddObject(GameObject obj)
    {
        if (obj != null && !m_objectList.Contains(obj))
            m_objectList.Add(obj);
    }
    // -- 오브젝트 제거 -------------------------------------------------------------------------------------------//
    public void RemoveObject(GameObject obj)
    {
        if(m_objectList.Contains(obj))
        {
            m_objectList.Remove(obj);
            GameObject.Destroy(obj);
        }
    }

    public void RemoveObjectName(string name)
    {
        foreach(GameObject obj in m_objectList)
        {
            Monster monster = obj.GetComponent<Monster>();

            if(monster != null)
            {
                if(monster.MONSTER_NAME.Equals(name))
                    RemoveObject(obj);
                return;
            }
        }
    }

    // -- 적 생성 ----------------------------------------------------------------------------------------------- //


    public GameObject AddMonster(string prefabPath,string name,Vector3 pos)
    {
        GameObject monster = AddObject(prefabPath,pos);

        if(monster != null)
        {
            if(prefabPath.Equals(GamePath.BOSS1))
            { 
                monster.GetComponent<Stage1BOSS>().MONSTER_NAME = name;
                
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(name,"boss1",pos.x,pos.y,pos.z));
            }
            else if(prefabPath.Equals(GamePath.MONSTER1))
            {
                monster.GetComponent<Stage1Monster>().MONSTER_NAME = name;
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(name , "monster1"));
                
            }
            else if (prefabPath.Equals(GamePath.MONSTER2))
            {
                monster.GetComponent<Stage1Monster>().MONSTER_NAME = name;
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(name , "monster2"));
            }
        }
        return monster;
    }
}
