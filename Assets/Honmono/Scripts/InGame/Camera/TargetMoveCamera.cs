using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMoveCamera : MonoBehaviour {


    //----------------------------------------//
    // Target 을  따라가는 카메라
    // 여기서 Target 은 주인공 로봇
    
    public GameObject m_target = null;

    // 배경 밖으로 나갈 수 없게 처리
    [SerializeField]
    public GameObject m_limitBackground = null;
    //---------------------------------------//
    // background 나가는 체크용
    float m_cameraHalfWdith = 0.0f;
    float m_cameraHalfHeight = 0.0f;

    float m_backgroundHalfWidth = 0.0f;
    float m_backgroundHalfHeight = 0.0f;

    Vector2 m_camera_correction = Vector2.zero;

    bool m_freezeX = false;
    bool m_freezeY = false;
    //--------------------------------------//
    // 바깥에서 사용할 GET
    public float CAMERA_HALF_WIDTH {  get { return this.m_cameraHalfWdith; } }
    public float CAMERA_HALF_HEIGHT { get { return this.m_cameraHalfHeight; } }
    public float BACKGROUND_HALF_WIDTH { get { return this.m_backgroundHalfWidth; } }
    public float BACKGROUND_HALF_HEIGHT { get { return this.m_backgroundHalfHeight; } }
    public bool CAMERA_MOVE_FREEZEX { get { return m_freezeX; }set { m_freezeX = value; } }
    public bool CAMERA_MOVE_FREEZEY { get { return m_freezeY; } set { m_freezeY = value; } }

    public Vector3 BACKGROUND_POS { get { return m_limitBackground.transform.position; } }

    public bool m_test = false;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {


        if(!m_test)
        {
            Vector3 pos = transform.position;
            Vector3 targetPos = m_target.transform.position;
            Vector3 backPos = m_limitBackground.transform.position;

            this.m_cameraHalfWdith = (Camera.main.orthographicSize * Screen.width / Screen.height);
            this.m_cameraHalfHeight = (this.m_cameraHalfWdith * Screen.height / Screen.width);
            float targetX = targetPos.x, targetY = targetPos.y;
            //왼쪽으로 넘어가는가!?
            if (targetPos.x - this.m_cameraHalfWdith
                <= backPos.x - this.m_backgroundHalfWidth)
                targetX = pos.x;
            // 오른쪽으로 넘어가는가?
            if (targetPos.x + this.m_cameraHalfWdith
                >= backPos.x + this.m_backgroundHalfWidth)
                targetX = pos.x;
            // 아래로 넘어가는가?
            if (targetPos.y - this.m_cameraHalfHeight
                < backPos.y - this.m_backgroundHalfHeight)
                targetY = pos.y;
            // 위로 넘어가는가 ?
            if (targetPos.y + this.m_cameraHalfHeight
                >= backPos.y + this.m_backgroundHalfHeight)
                targetY = pos.y;

            if (m_freezeX) targetX = transform.position.x;
            if (m_freezeY) targetY = transform.position.y;
            transform.position = new Vector3(targetX , targetY , pos.z);

            Debug.DrawLine(new Vector3(backPos.x - m_backgroundHalfWidth, backPos.y + m_backgroundHalfHeight , -1) , 
                new Vector3(backPos.x + m_backgroundHalfWidth , backPos.y + m_backgroundHalfHeight , 0),Color.red);
            Debug.DrawLine(new Vector3(backPos.x - m_backgroundHalfWidth, backPos.y - m_backgroundHalfHeight , -1) , 
                new Vector3(backPos.x + m_backgroundHalfWidth, backPos.y - m_backgroundHalfHeight , 0) , Color.blue);

            Debug.DrawLine(new Vector3(backPos.x - m_backgroundHalfWidth , backPos.y + m_backgroundHalfHeight , -1) , 
                new Vector3(backPos.x - m_backgroundHalfWidth , backPos.y - m_backgroundHalfHeight , 0) , Color.red);
            Debug.DrawLine(new Vector3(backPos.x + m_backgroundHalfWidth , backPos.y + m_backgroundHalfHeight , -1) , 
                new Vector3(backPos.x + m_backgroundHalfWidth , backPos.y - m_backgroundHalfHeight , 0) , Color.blue);

        }
        else
        {
            Vector3 targetPos = m_target.transform.position;
            transform.position = new Vector3(targetPos.x , targetPos.y,transform.position.z);
        }
        
	}

    public void SetBackgrounds(GameObject obj)
    {
        this.m_backgroundHalfWidth = 0.0f;
        this.m_backgroundHalfHeight = 0.0f;

        

        switch(obj.transform.childCount)
        {
            case 0:
                SpriteRenderer s = obj.GetComponent<SpriteRenderer>();
                m_backgroundHalfWidth = s.bounds.size.x;
                m_backgroundHalfHeight = s.bounds.size.y;
                break;
            case 1:
            case 2:
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    Transform t = obj.transform.GetChild(i);
                    SpriteRenderer spr = t.GetComponent<SpriteRenderer>();

                    if (spr != null)
                    {
                        this.m_backgroundHalfWidth += spr.bounds.size.x;
                        this.m_backgroundHalfHeight = spr.bounds.size.y;
                    }

                }
                break;
            case 4:
                int index = 0;
                for (int i = 0; i < obj.transform.childCount / 2; i++)
                {
                    Transform t = obj.transform.GetChild(i);
                    SpriteRenderer spr = t.GetComponent<SpriteRenderer>();
                    

                    if (spr != null )
                    {
                        this.m_backgroundHalfWidth += spr.bounds.size.x;
                        this.m_backgroundHalfHeight += spr.bounds.size.y;
                    }
                    index++;
                }
                break;

        }

        
        //if(obj.transform.childCount > 1)
        //{
        //    this.m_backgroundHalfWidth *= 0.5f;
        //    this.m_backgroundHalfHeight *= 0.5f;
        //}
        this.m_limitBackground = obj;
        this.m_backgroundHalfWidth *= 0.5f;
        this.m_backgroundHalfHeight *= 0.5f;
    }
    
    public void SetCameraCorrection(Vector2 pos)
    {
        this.m_camera_correction = pos;
    }

    
    
}
