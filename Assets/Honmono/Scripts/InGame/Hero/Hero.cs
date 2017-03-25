using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour, NetworkManager.NetworkMessageEventListenrer
{

    public enum HERO_STATE
    {
        IDLE = 0,
        MOVE = 4,
        JUMP = 8,
        LADDER = 2
    }


    // 기본 정보 --------------------------------------------------------------------------------------//

    // 렌더러
    private SpriteRenderer m_renderer = null;
    // 점프시 가할 힘
    private float m_jumpPower = 300.0f;

    // 이동 속도
    private float m_moveSpeed = 6.0f;

    //현재 상태 설정
    private int m_curState = (int)HERO_STATE.IDLE;

    // 리지드바디
    private Rigidbody2D m_rigidBody = null;

    // 애니메이터
    private Animator m_animator = null;

    // 연출용
    [SerializeField]
    private Light m_Light = null;

    // 자기자신 판별용
    [SerializeField]
    public bool m_isMe = true;

    [SerializeField]
    private string m_userName = "";


    //기존 위치
    Vector3 m_prevPos = Vector3.zero;
    // USER NAME
    public string USERNAME { get { return m_userName; } }

    //-- normal map animation ---------------------------------------------------------------------------//
    // normal map 의 경우 unity animation 에서
    // 변경이 불가능한 것으로 보이므로 스크립트 내에서 바꿈
    // [일단은 임시 구현] TODO 더 나은 방법을 찾기
    [SerializeField]
    private List<Texture> m_moveNormalList = new List<Texture>();
    [SerializeField]
    private List<Texture> m_idleNormalList = new List<Texture>();
    private List<Texture> m_currentNormalList = null;
    private int m_currentNormalIndex = 0;
    Vector3 m_targetPos = Vector3.zero;
    float m_syncTime = 0.0f;
    float m_delay = 0.0f;
    float m_lastSyncTime = 0.0f;


    //-- animation ---------------------------------------------------------------------------------------//
    private void ChangeNormalAnimation()
    {
        if (m_renderer == null || m_currentNormalList == null)
            return;
        this.m_renderer.material.SetTexture("_BumpMap", m_currentNormalList[m_currentNormalIndex++]);

        if (m_currentNormalIndex >= m_currentNormalList.Count)
            m_currentNormalIndex = 0;

    }
    private void SetupNormalAnimation(int type)
    {
        // type  = 0  fly  type = 1 fire
        this.m_currentNormalList = (type == 0) ? this.m_moveNormalList : this.m_idleNormalList;
        m_currentNormalIndex = 0;
        this.m_renderer.material.SetTexture("_BumpMap", m_currentNormalList[m_currentNormalIndex++]);
        if (m_currentNormalIndex >= m_currentNormalList.Count)
            m_currentNormalIndex = 0;

    }

    // 연출용 라이트
    void MoveLeft()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 7.0f, "to", -7.0f, "time", 3.0f, "onupdatetarget", gameObject, "onupdate", "LightUpdate", "oncompletetarget", gameObject, "oncomplete", "MoveRight"));
    }

    void MoveRight()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", -7.0f, "to", 7.0f, "time", 3.0f, "onupdatetarget", gameObject, "onupdate", "LightUpdate", "oncompletetarget", gameObject, "oncomplete", "MoveLeft"));
    }

    void LightUpdate(float s)
    {
        m_Light.transform.localPosition = new Vector3(s, m_Light.transform.localPosition.y);
    }

    //--네트워크--------------------------------------------------------------------------------------------------------------//

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        // 임시  //면 처리할 필요 없음
        if (m_userName != e.user)
            return;

        //임시
        m_renderer.enabled = true;

        switch (e.msgType)
        {
            case NetworkManager.MOVE:
                if (m_isMe)
                    return;
                Vector3 newPos = new Vector3(e.msg.GetField("x").f, e.msg.GetField("y").f);

                float distance = Vector3.Distance(transform.position, newPos);

                if (distance <= 0)
                    return;
                m_targetPos = newPos;

                m_syncTime = 0.0f;
                m_delay = Time.time - m_lastSyncTime;
                m_lastSyncTime = Time.time;
                this.m_renderer.flipX = e.msg.GetField(NetworkManager.DIR).b;
                this.m_animator.SetBool("Move", true);

                break;
        }
    }

    //------------------------------------------------------------------------------------------------------------------------//

    // Use this for initialization
    void Start()
    {

        // 이 Hero 는 Player 본인이다.
        if (m_isMe)
            GameManager.Instance().HeroSetup(this);
            
        // 움직였을 때만 패킷을 전송해야 한다. 그러기 위한 디스턴스 판별용 포지션 적용
        m_prevPos = transform.position;

        //-- 필요 컴포넌트 받아오기-------------------------------//
        m_renderer = this.GetComponent<SpriteRenderer>();
        m_rigidBody = this.GetComponent<Rigidbody2D>();
        m_animator = this.GetComponent<Animator>();
        //-------------------------------------------------------//
        
        // 노말맵 애니메이션 세팅용
        this.m_currentNormalList = this.m_idleNormalList;


        // 보여주기용 라이팅
        MoveLeft();
        
        // 네트워크 이벤트 옵저버 등록
        NetworkManager.Instance().AddNetworkMessageEventListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isMe)
            Control();
        else
        {
            NetworkMoveLerp();
        }
    }

    //-- Network Message 에 따른 이동 보간 ( 네트워크 플레이어 ) ------------------------------------------------------------//
    void NetworkMoveLerp()
    {
        m_syncTime += Time.deltaTime;

        // 네트워크 보간( 테스트 완료 - 로컬 )
        if (m_delay > 0)
            transform.position = Vector3.Lerp(transform.position, m_targetPos, m_syncTime / m_delay);
    }
    //-----------------------------------------------------------------------------------------------------------------------//

    //-- 실 캐릭터 조작 -----------------------------------------------------------------------------------------------------//
    void Control()
    {

        Vector2 pos = transform.position;

        //상황 설정
        // 좌 - 우 이동  상  점프 & 사다리  하 사다리
        float moveX = 0.0f;
        float moveY = 0.0f;
        float jump = 0.0f;

        if (Input.GetKey(KeyCode.A))
        {
            m_curState = BitControl.Set(m_curState, (int)HERO_STATE.MOVE);

            moveX = -m_moveSpeed * Time.deltaTime;
            this.GetComponent<SpriteRenderer>().flipX = false;
        }



        if (Input.GetKey(KeyCode.D))
        {
            m_curState = BitControl.Set(m_curState, (int)HERO_STATE.MOVE);
            moveX = m_moveSpeed * Time.deltaTime;
            this.GetComponent<SpriteRenderer>().flipX = true;

        }

        if (Input.GetKey(KeyCode.W))
        {
            // 요부분에서 사다리를 체크함

            // move 상태인지 체크
            bool move = BitControl.Get(m_curState, (int)HERO_STATE.MOVE);

            //     MDebug.Log("move " + move + " t " + BitControl.Get(m_curState, (int)HERO_STATE.JUMP));

            if (BitControl.Get(m_curState, (int)HERO_STATE.LADDER))
            {
                moveY = m_moveSpeed * Time.deltaTime;
            }
            else if (!BitControl.Get(m_curState, (int)HERO_STATE.JUMP))
            {
                m_curState = BitControl.Set(m_curState, (int)HERO_STATE.JUMP);
                jump = m_jumpPower;// * Time.deltaTime;
            }
        }

        if (Input.GetKey(KeyCode.S))
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
        if (Input.GetKey(KeyCode.G))
        {

            //this.m_animator.SetBool("Move", false);
        }

        // 이동 애니메이션 체크
        if (BitControl.Get(m_curState, (int)HERO_STATE.MOVE) && (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)))
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

        MoveSend();
    }

    void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos, pos);
        m_prevPos = transform.position;
        //    MDebug.Log(t);
        if (distance <= 0)
            return;

        NetworkManager.Instance().SendNetworkMessage(JSONMessageTool.ToJsonMove(pos.x, pos.y, m_renderer.flipX));
        // NetworkManager.Instance().SendMovePos(GameManager.Instance().PLAYER.ToJsonPositionInfo());
    }

    // 충돌 --------------------------------------------------------------------//

    void OnCollisionEnter2D(Collision2D col)
    {
        // 부딪친 대상이 지형일 경우와 오브젝트일 경우가 있다.
        // 지형일 경우
        // 점프 중인지 체크
        if (BitControl.Get(m_curState, (int)HERO_STATE.JUMP))
        {
            m_curState = BitControl.Clear(m_curState, (int)HERO_STATE.JUMP);
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
