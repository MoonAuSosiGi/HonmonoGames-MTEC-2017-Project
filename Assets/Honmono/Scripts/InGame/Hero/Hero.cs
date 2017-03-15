using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {

    public enum HERO_STATE
    {
        IDLE = 0,
        MOVE = 4,
        JUMP = 8,
        LADDER = 2
    }


    // 기본 정보 ----------------------------------------------//

    // 점프시 가할 힘
    private float m_jumpPower = 300.0f;

    // 이동 속도
    private float m_moveSpeed = 3.0f;

    //현재 상태 설정
    private int m_curState = (int)HERO_STATE.IDLE;

    // 리지드바디
    private Rigidbody2D m_rigidBody = null;

    // 애니메이터
    private Animator m_animator = null;

   //----------------------------------------------------------//

	// Use this for initialization
	void Start () {
        m_rigidBody = this.GetComponent<Rigidbody2D>();
        m_animator = this.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        Control();
	}


    void Control()
    {
        
        Vector2 pos = transform.position;

        //상황 설정
        // 좌 - 우 이동  상  점프 & 사다리  하 사다리
        float moveX = 0.0f;
        float moveY = 0.0f;
        float jump = 0.0f;

        if(Input.GetKey(KeyCode.A))
        {
            m_curState = BitControl.Set(m_curState, (int) HERO_STATE.MOVE);
             
            moveX = -m_moveSpeed * Time.deltaTime;
            this.GetComponent<SpriteRenderer>().flipX = false;
        }
        


        if(Input.GetKey(KeyCode.D))
        {
            m_curState = BitControl.Set(m_curState, (int)HERO_STATE.MOVE);
            moveX = m_moveSpeed * Time.deltaTime;
            this.GetComponent<SpriteRenderer>().flipX = true;

        }

        if(Input.GetKey(KeyCode.W))
        {
            // 요부분에서 사다리를 체크함

            // move 상태인지 체크
            bool move = BitControl.Get(m_curState, (int)HERO_STATE.MOVE);

            MDebug.Log("move " + move + " t " + BitControl.Get(m_curState, (int)HERO_STATE.JUMP));

            if(BitControl.Get(m_curState,(int)HERO_STATE.LADDER))
            {
                moveY = m_moveSpeed * Time.deltaTime;
            }
            else if(!BitControl.Get(m_curState,(int)HERO_STATE.JUMP))
            {
                m_curState = BitControl.Set(m_curState, (int)HERO_STATE.JUMP);
                jump = m_jumpPower;// * Time.deltaTime;
            }
        }

        if(Input.GetKey(KeyCode.S))
        {
            // 요부분에서 사다리를 체크함

            if (BitControl.Get(m_curState, (int)HERO_STATE.LADDER))
            {
                moveY = -m_moveSpeed * Time.deltaTime;
            }
            else
            {
                //딱히 하는거 없음
            }
        }

        // 상호작용 키
        if(Input.GetKey(KeyCode.G))
        {
            
            //this.m_animator.SetBool("Move", false);
        }

        // 이동 애니메이션 체크
        if(BitControl.Get(m_curState,(int)HERO_STATE.MOVE) && (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)))
        {
           m_curState = BitControl.Clear(m_curState, (int)HERO_STATE.MOVE);

        }

        // 최종 

        if (m_curState == (int)HERO_STATE.IDLE)
        {

            this.m_animator.SetBool("Move", false);
        }
        else
        {
            if (BitControl.Get(m_curState, (int)HERO_STATE.JUMP))
            {
                this.m_animator.SetBool("Move", false);
                this.m_rigidBody.AddForce(new Vector2(0, jump));
            }
            if (BitControl.Get(m_curState, (int)HERO_STATE.MOVE))
            {
                this.m_animator.SetBool("Move", true);
                this.transform.Translate(new Vector3(moveX, moveY));
            }
        }
        
    }

    // 충돌 --------------------------------------------------------------------//

    void OnCollisionEnter2D(Collision2D col)
    {
        // 부딪친 대상이 지형일 경우와 오브젝트일 경우가 있다.
        // 지형일 경우
        // 점프 중인지 체크
        if(BitControl.Get(m_curState,(int) HERO_STATE.JUMP))
        {
            m_curState =  BitControl.Clear(m_curState, (int)HERO_STATE.JUMP);
        }

       // MDebug.Log("Collision Enter");
    }

    void OnCollisionStay2D(Collision2D col)
    {
    //    MDebug.Log("Collision Stay");
    }

    void OnCollisionExit2D(Collision2D col)
    {
      //  MDebug.Log("Collision Exit");
    }
}
