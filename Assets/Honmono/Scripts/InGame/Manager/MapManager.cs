using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    private List<GameObject> m_users = new List<GameObject>();

    


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

}
