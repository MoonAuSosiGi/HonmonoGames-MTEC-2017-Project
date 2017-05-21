using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 카메라에 트윈 적용 등 
public class CameraManager : Singletone<CameraManager>
{

    //-- 기본 필요 정보 ---------------------------------------------//
    private TargetMoveCamera m_targetMove = null;
    private float m_startSize = 0.0f;
    private const string TWEEN_EASETYPE = "easeInOutCirc";
    private Vector3 m_targetPos = Vector3.zero;

    public bool CAMERA_ROBO_MOVE
    {
        get { return m_targetMove.enabled; }
        set { m_targetMove.enabled = value; }
    }

    private float m_targetSize = 10.0f;

    private Vector3 m_prevPos = Vector3.zero;


    //여기서 이동하면서 알아서 background 를 찾은 뒤 처리
    public enum CAMERA_PLACE
    {
        GAME_START = 0,
        ROBO_IN,
        STAGE1,
        STAR,
        BOSS,
        TUTORIAL_PLAYERMOVE,
        TUTORIAL_ROBO
    }
    private CAMERA_PLACE m_place = CAMERA_PLACE.GAME_START;

    public CAMERA_PLACE PLACE { get { return m_place; } }

    public GameObject m_playerIntheROBO = null;
    public GameObject m_playerROBO = null;
    public GameObject m_playerIntheSPACE = null;
    

    // 게임 시작 위치로 갈 때 사용
    public GameObject m_gameStart = null;
    public GameObject m_boss = null;
    public GameObject m_boss1Place = null;

    // Place 붙은 것들은 background 검출용
    public GameObject m_robotPlace = null;
    public GameObject m_stage1Place = null;
    public GameObject m_star1Place = null;    

    public GameObject m_inTheStar = null;
    public GameObject m_andObj = null;

    
    public GameObject m_starPos = null;

    public GameObject m_Title = null;

    public GameObject m_TutorialPlayerBG = null;
    public GameObject m_TutorialPlayerPos = null;

    public GameObject m_TutorialROBOBG = null;
    public GameObject m_TutorialROBOPos = null;
    public GameObject m_tutorobo = null;

    private GameObject m_funcTarget = null;
    private string m_func = null;
    private bool m_showEnd = true;
    //---------------------------------------------------------------//

    void Awake()
    {
        m_startSize = Camera.main.orthographicSize;
        m_targetMove = this.GetComponent<TargetMoveCamera>();
    }


    // 이동 !
    public void MoveCamera(GameObject targetPos , float cameraSize , CAMERA_PLACE place , GameObject targetFunc = null , string func = null,bool showEnd = true)
    {
        if (iTween.Count(gameObject) > 0)
            return;
        Camera.main.orthographic = true;
        
        m_targetMove.enabled = false;
        m_targetSize = cameraSize;
        m_targetPos = targetPos.transform.position;
        SetCameraPlace(place);

        m_funcTarget = targetFunc;
        m_func = func;
        m_showEnd = showEnd;
        CameraHide();
    }

    public void MoveCameraAndObject(GameObject targetPos,float cameraSize,CAMERA_PLACE place,
        GameObject andObj, GameObject targetFunc = null , string func = null,bool showEnd = true)
    {
        if (iTween.Count(gameObject) > 0)
            return;
        m_targetMove.enabled = false;
        m_targetSize = cameraSize;
        m_targetPos = targetPos.transform.position;
        m_andObj = andObj;
        SetCameraPlace(place);

        m_funcTarget = targetFunc;
        m_func = func;
        m_showEnd = showEnd;
        CameraHide();
        
    }

    // 카메라 위치 설정을 위한 
    public void SetCameraPlace(CAMERA_PLACE place)
    {
        this.m_place = place;
        Vector2 colliderSize = this.GetComponent<BoxCollider2D>().size;
        GameObject obj = null;
        switch (m_place)
        {
            case CAMERA_PLACE.GAME_START:
                //obj = m_gameStart; 
                colliderSize = new Vector2(25.0f, 20.0f);
                m_targetMove.m_target = m_playerROBO;
                break;
            case CAMERA_PLACE.ROBO_IN:
                obj = m_robotPlace; colliderSize = new Vector2(15.0f, 12.0f);
                m_targetMove.m_target = m_playerIntheROBO;
                break;
            case CAMERA_PLACE.STAGE1:
                obj = m_stage1Place; colliderSize = new Vector2(25.0f, 20.0f);
                m_targetMove.m_target = m_playerROBO;
                //m_targetPos = obj.transform.position;
                break;
            case CAMERA_PLACE.STAR:
                obj = m_star1Place; colliderSize = new Vector2(25.0f , 20.0f);
                m_targetMove.m_target = m_andObj;
               // GameManager.Instance().ROBO.transform.position = m_starPos.transform.position;
                break;
            case CAMERA_PLACE.BOSS:
                obj = m_boss1Place; colliderSize = new Vector2(25.0f , 20.0f);
                m_targetPos = m_boss.transform.position;
                break;
            case CAMERA_PLACE.TUTORIAL_PLAYERMOVE:
                obj = m_TutorialPlayerBG;
                m_targetMove.m_target = m_TutorialPlayerPos;
                m_targetPos = m_TutorialPlayerPos.transform.position;
                break;
            case CAMERA_PLACE.TUTORIAL_ROBO:
                obj = m_TutorialROBOBG;
                m_targetMove.m_target = m_tutorobo;
                m_targetPos = m_TutorialROBOPos.transform.position;
                break;
        }

        if ( m_place == CAMERA_PLACE.BOSS)
        {
            m_targetMove.m_test = true;
        }
        else
            m_targetMove.m_test = false;



        this.GetComponent<BoxCollider2D>().size = colliderSize;
        if (obj != null)
            m_targetMove.SetBackgrounds(obj);
    }


    private void CameraHide()
    {
        this.GetComponent<BoxCollider2D>().enabled = false;
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", m_startSize, "to", 0.01f,
            "easeType", TWEEN_EASETYPE,
            "onupdatetarget", gameObject,
            "onupdate", "CameraTweenUpdate",
            "oncompletarget", gameObject,
            "oncomplete", "CameraHideEnd"));
    }

    private void CameraShow()
    {
        iTween.ValueTo(gameObject, iTween.Hash(
              "from", 0.1f, "to", m_targetSize,
              "easeType", TWEEN_EASETYPE,
              "onupdatetarget", gameObject,
              "onupdate", "CameraTweenUpdate",
              "oncompletarget", gameObject,
              "oncomplete", "CameraShowEnd"));
    }

    private void CameraTweenUpdate(float v)
    {
        Camera.main.orthographicSize = v;
    }

    private void CameraShowEnd()
    {
        this.GetComponent<BoxCollider2D>().enabled = true;
        m_startSize = m_targetSize;

        if (m_funcTarget != null && m_showEnd)
            m_funcTarget.SendMessage(m_func , SendMessageOptions.DontRequireReceiver);

        if (m_place != CAMERA_PLACE.GAME_START)
            m_targetMove.enabled = true;

        m_funcTarget = null;
        m_func = null;
    }

    private void CameraHideEnd()
    {
        CameraShow();
        // Invoke("CameraShow", 0.5f);
        

        transform.position = new Vector3(m_targetPos.x, m_targetPos.y, transform.position.z);

        if (m_funcTarget != null && !m_showEnd)
            m_funcTarget.SendMessage(m_func , SendMessageOptions.DontRequireReceiver);

        if (m_place == CAMERA_PLACE.GAME_START)
        {
            
            m_Title.SetActive(false);
        }

        // 이동해야함
        if (m_andObj != null)
        {
            Vector3 pos = transform.position;
            m_andObj.transform.position = new Vector3(pos.x , pos.y , -1.0f);
            m_andObj = null;
        }

    }


    // Camera Size 동적 조절
    void OnTriggerStay2D(Collider2D col)
    {

        if (m_place != CAMERA_PLACE.ROBO_IN)
            return;
        if (col.transform.position.y > transform.position.y && col.name == "UP")
        {
            if (col.transform.position.y > 9.0f && transform.position.y < 10.74f)
            {
                if (this.GetComponent<Camera>().orthographicSize - 0.3f > 4.0f)
                {

                    this.GetComponent<Camera>().orthographicSize = this.GetComponent<Camera>().orthographicSize - 0.3f;

                }
                else
                {
                    m_targetMove.SetCameraCorrection(new Vector2(0.0f, 1.73f));
                    this.GetComponent<Camera>().orthographicSize = 4.0f;
                    this.GetComponent<BoxCollider2D>().size = new Vector2(10.0f, 8.0f);
                    transform.position = new Vector3(transform.position.x, 10.74f, transform.position.z);
                }
            }

        }
        else if (col.transform.position.y < transform.position.y && col.name == "DOWN")
        {

            m_targetMove.SetCameraCorrection(new Vector2(0.0f, 0.0f));
            if (this.GetComponent<Camera>().orthographicSize + 0.3f < 6.0f)
            {
                this.GetComponent<Camera>().orthographicSize = this.GetComponent<Camera>().orthographicSize + 0.3f;
            }
            else
            {

                this.GetComponent<BoxCollider2D>().size = new Vector2(15.0f, 12.0f);
                this.GetComponent<Camera>().orthographicSize = 6.0f;
            }
        }
    }
}
