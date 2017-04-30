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
        BOSS
    }
    private CAMERA_PLACE m_place = CAMERA_PLACE.GAME_START;

    public GameObject m_playerIntheROBO = null;
    public GameObject m_playerROBO = null;
    public GameObject m_playerIntheSPACE = null;
    

    public GameObject m_gameStart = null;
    public GameObject m_robotPlace = null;
    public GameObject m_stage1Place = null;
    public GameObject m_star1Place = null;
    public GameObject m_boss = null;

    public GameObject m_inTheStar = null;
    public GameObject m_andObj = null;


    public GameObject m_bossROBO = null;
    public GameObject m_starPos = null;
    //---------------------------------------------------------------//

    void Awake()
    {
        m_startSize = Camera.main.orthographicSize;
        m_targetMove = this.GetComponent<TargetMoveCamera>();
    }

    
    // 이동 !
    public void MoveCamera(GameObject targetPos, float cameraSize,CAMERA_PLACE place)
    {
        m_targetMove.enabled = false;
        m_targetSize = cameraSize;
        m_targetPos = targetPos.transform.position;
        SetCameraPlace(place);
        CameraHide();
    }

    public void MoveCameraAndObject(GameObject targetPos,float cameraSize,CAMERA_PLACE place,
        GameObject andObj)
    {
        m_targetMove.enabled = false;
        m_targetSize = cameraSize;
        m_targetPos = targetPos.transform.position;
        m_andObj = andObj;
        SetCameraPlace(place);
        
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
                obj = m_gameStart; colliderSize = new Vector2(25.0f, 20.0f);
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
                GameManager.Instance().ROBO.transform.position = m_starPos.transform.position;
                break;
            case CAMERA_PLACE.BOSS:
                obj = m_boss; colliderSize = new Vector2(25.0f , 20.0f);
                GameManager.Instance().ROBO.transform.position = m_bossROBO.transform.position;
                m_targetPos = m_boss.transform.position;
                break;
        }

        if (m_place == CAMERA_PLACE.STAR ||  m_place == CAMERA_PLACE.BOSS)
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
            "from", m_startSize, "to", 0.1f,
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

      
        m_targetMove.enabled = true;
    }

    private void CameraHideEnd()
    {
        CameraShow();
       // Invoke("CameraShow", 0.5f);
        transform.position = new Vector3(m_targetPos.x, m_targetPos.y, transform.position.z);

        // 이동해야함
        if (m_andObj != null)
        {
            Vector3 pos = transform.position;
            m_andObj.transform.position = new Vector3(pos.x , pos.y , -1.0f);
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
