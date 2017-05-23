using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class TutoHero : MonoBehaviour {

    // 렌더러
    private SkeletonAnimation m_skletonAnimation = null;
    // 로봇과의 렌더링
    private BansheeGz.BGSpline.Curve.BGCurve m_bgCurve = null;
    // 점프시 가할 힘
    private float m_jumpPower = 300.0f;

    // 이동 속도
    private float m_moveSpeed = 6.0f;

    //현재 상태 설정
    private int m_curState = (int)Hero.HERO_STATE.IDLE;

    // 리지드바디
    private Rigidbody2D m_rigidBody = null;

    // Animation 이름
    private const string ANI_IDLE = "idle";
    private const string ANI_JUMP_FALLING = "jump_falling";
    private const string ANI_JUMP_JUMPING = "jump_jumping";
    private const string ANI_JUMP_READY = "jump_ready";
    private const string ANI_JUMP_FALL = "jump_~fall";
    private const string ANI_MOVE = "move";
    private const string ANI_REPAIR = "repair";
    private const string ANI_STAND = "stand";
    private const string ANI_STAND_UP = "stand_up";


    // Use this for initialization
    void Start () {
        //-- 필요 컴포넌트 받아오기-------------------------------//
        m_skletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_rigidBody = this.GetComponent<Rigidbody2D>();

        //-------------------------------------------------------//
    }

    // Update is called once per frame
    void Update () {
        Control();
	}
    float VerticalMoveControl()
    {
        float moveY = 0.0f;
        float jump = 0.0f;
        if (Input.GetKey(KeyCode.W))
        {
            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER))
            {
                moveY = m_moveSpeed * Time.deltaTime;
                m_rigidBody.gravityScale = 0.0f;
                m_rigidBody.velocity = Vector2.zero;

                m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.JUMP);
                m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.JUMP_FALL);
            }
            else if (!BitControl.Get(m_curState , (int)Hero.HERO_STATE.JUMP)
                && !BitControl.Get(m_curState , (int)Hero.HERO_STATE.JUMP_FALL))
            {
                // 특정 오브젝트 밟자마자 (점프 안끝났는데) 점프하는 경우 방지
                if (m_rigidBody.velocity.y <= 0.0f)
                {
                    m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.JUMP);
                    jump = m_jumpPower;
                }

            }
        }

        if (Input.GetKey(KeyCode.S))
        {

            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER))
            {
                moveY = -m_moveSpeed * Time.deltaTime;
                m_rigidBody.gravityScale = 0.0f;
                m_rigidBody.velocity = Vector2.zero;

                m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.JUMP);
                m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.JUMP_FALL);
            }
            else
            {
                //딱히 하는거 없음
            }
        }
        return (BitControl.Get(m_curState , (int)Hero.HERO_STATE.JUMP)) ? jump : moveY;
    }

    // 가로 이동
    float HorizontalMoveControl()
    {
        float moveX = 0.0f;

        if (Input.GetKey(KeyCode.A))
        {
            m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.MOVE);

            moveX = -m_moveSpeed * Time.deltaTime;

            this.m_skletonAnimation.skeleton.flipX = false;
            //this.GetComponent<SpriteRenderer>().flipX = false;
        }



        if (Input.GetKey(KeyCode.D))
        {
            m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.MOVE);
            moveX = m_moveSpeed * Time.deltaTime;

            this.m_skletonAnimation.skeleton.flipX = true;

        }
        return moveX;
    }

    void Control()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            transform.gameObject.SetActive(false);
            Camera.main.GetComponent<TargetMoveCamera>().m_test = true;
            return;
        }


        Vector2 pos = transform.position;

        //상황 설정
        // 좌 - 우 이동  상  점프 & 사다리  하 사다리
        float moveX = 0.0f;
        float moveY = 0.0f;
        float jump = 0.0f;

        // 특정 컨트롤을 다루고 있는 상태가 아닐 경우
        //  if(GetMoveAbleCheck())

        // 이동에 관련된 처리를 하는 녀석들 float 

        // 가로이동
        moveX = HorizontalMoveControl();

        // 점프 및 수직 이동
        if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER))
            moveY = VerticalMoveControl();
        else
            jump = VerticalMoveControl();





        // 이동 애니메이션 체크
        if (!BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER) && (BitControl.Get(m_curState , (int)Hero.HERO_STATE.MOVE) &&
            (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))))
        {
            CheckAndSetAnimation(ANI_IDLE , true);
            m_curState = (int)Hero.HERO_STATE.IDLE; //BitControl.Clear(m_curState, (int)HERO_STATE.MOVE);

        }

        // 떨어지는 상태임

        if (!BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER))
        {
            if (m_rigidBody.velocity.y < 0.0f)
            {
                m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.JUMP);
                m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.JUMP_FALL);
            }
            if (m_rigidBody.velocity.y >= 0.0f)
            {
                m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.JUMP_FALL);
            }
        }



        // -- 상태값에 따라 애니메이션 처리 및 이동 점프 처리 ----------------------------------------

        if (m_curState == (int)Hero.HERO_STATE.IDLE)
        {
            CheckAndSetAnimation(ANI_IDLE , true);
        }
        else
        {
            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.JUMP))
            {
                // 점프 ㄱㄱ
                this.m_rigidBody.AddForce(new Vector2(0 , jump));
                // IDLE / MOVE 상태일 때만 점프 애니메이션 
                if (IsCurrentAnimation(ANI_IDLE) || IsCurrentAnimation(ANI_MOVE))
                {
                    //레디 -> 점핑 ㄱㄱ
                    m_skletonAnimation.state.SetAnimation(0 , ANI_JUMP_READY , false);
                    m_skletonAnimation.state.AddAnimation(0 , ANI_JUMP_JUMPING , true , 0f);
                }

            }

            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.JUMP_FALL))
            {
                //떨어지는 중
                CheckAndSetAnimation(ANI_JUMP_FALL , false);
            }

            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.MOVE)
                || BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER))
            {
                if (IsCurrentAnimation(ANI_IDLE))
                {
                    m_skletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);
                }
                this.transform.Translate(new Vector3(moveX , moveY));
            }
        }
        Debug.DrawLine(pos , new Vector3(pos.x , pos.y - GetComponent<BoxCollider2D>().bounds.size.y , 0.0f) , Color.red);

    }

    // -- 스파인 애니메이션용 -------------------------------------------------------//
    bool IsCurrentAnimation(string ani)
    {
        if (m_skletonAnimation == null)
            return false;
        return m_skletonAnimation.state.GetCurrent(0).animation.name == ani;
    }

    void CheckAndSetAnimation(string ani , bool loop)
    {
        if (IsCurrentAnimation(ani) == false)
            m_skletonAnimation.state.SetAnimation(0 , ani , loop);
    }

    //-------------------------------------------------------------------------------//
}
