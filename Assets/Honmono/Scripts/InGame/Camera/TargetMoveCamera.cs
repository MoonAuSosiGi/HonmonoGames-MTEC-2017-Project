using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMoveCamera : MonoBehaviour {


    //----------------------------------------//
    // Target 을  따라가는 카메라
    // 여기서 Target 은 주인공 로봇
    [SerializeField]
    private GameObject m_target = null;

    // 배경 밖으로 나갈 수 없게 처리
    [SerializeField]
    private GameObject m_limitBackground = null;
    //---------------------------------------//
    // background 나가는 체크용
    float m_cameraHalfWdith = 0.0f;
    float m_cameraHalfHeight = 0.0f;

    float m_backgroundHalfWidth = 0.0f;
    float m_backgroundHalfHeight = 0.0f;


    //--------------------------------------//
    // 바깥에서 사용할 GET
    public float CAMERA_HALF_WIDTH {  get { return this.m_cameraHalfWdith; } }
    public float CAMERA_HALF_HEIGHT { get { return this.m_cameraHalfHeight; } }
    public float BACKGROUND_HALF_WIDTH { get { return this.m_backgroundHalfWidth; } }
    public float BACKGROUND_HALF_HEIGHT { get { return this.m_backgroundHalfHeight; } }

    public Vector3 BACKGROUND_POS { get { return m_limitBackground.transform.position; } }

	// Use this for initialization
	void Start () {
        // background width, height
        this.m_backgroundHalfWidth = this.m_limitBackground.GetComponent<SpriteRenderer>().bounds.size.x / 2.0f;
        this.m_backgroundHalfHeight = this.m_limitBackground.GetComponent<SpriteRenderer>().bounds.size.y /2.0f;

        // camera
        this.m_cameraHalfWdith = (Camera.main.orthographicSize * Screen.width / Screen.height);
        this.m_cameraHalfHeight = (this.m_cameraHalfWdith * Screen.height / Screen.width);
    }
	
	// Update is called once per frame
	void Update () {

        Vector3 pos = transform.position;
        Vector3 targetPos = m_target.transform.position;
        Vector3 backPos = m_limitBackground.transform.position;


        float targetX = targetPos.x, targetY = targetPos.y;
        // 왼쪽으로 넘어가는가!?
        if (targetPos.x - this.m_cameraHalfWdith <= backPos.x - this.m_backgroundHalfWidth) targetX = pos.x;
        // 오른쪽으로 넘어가는가?
        if (targetPos.x + this.m_cameraHalfWdith >= backPos.x + this.m_backgroundHalfWidth) targetX = pos.x;
        // 아래로 넘어가는가?
        if (targetPos.y - this.m_cameraHalfHeight <= backPos.y - this.m_backgroundHalfHeight) targetY = pos.y;
        // 위로 넘어가는가 ?
        if (targetPos.y + this.m_cameraHalfHeight >= backPos.y + this.m_backgroundHalfHeight) targetY = pos.y;

        transform.position = new Vector3(targetX,targetY, pos.z);
	}
    

    
}
