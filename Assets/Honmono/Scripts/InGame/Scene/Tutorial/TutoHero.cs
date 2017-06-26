using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class TutoHero : MonoBehaviour {

    // 튜토
    public TutorialController m_tuto = null;
    // 렌더러
    private SkeletonAnimation m_skletonAnimation = null;
    // 사다리
    private SkeletonAnimation m_climb = null;
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
    //이동
    public List<GameObject> m_tutoMove = new List<GameObject>();
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


    private string m_userControlName = "";
    private GameObject m_damagePointFix = null;
    private bool m_LadderState = false;
    // -- 에너지 차지 ---------------------------------------------------------------------------------------------------------//
    public SpriteRenderer m_chargePad = null;
    public GameObject m_chargeTopObj = null;
    public GameObject m_chargeBottomObj = null;

    public GameObject m_tutoRobo = null;
    bool m_tutoMoveCk = false;
    public GameObject m_tutoEnemy = null;
    // Use this for initialization
    void Start () {
        //-- 필요 컴포넌트 받아오기-------------------------------//
        m_skletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_rigidBody = this.GetComponent<Rigidbody2D>();
        m_climb = this.transform.GetChild(2).GetComponent<SkeletonAnimation>();
        m_skletonAnimation.state.Complete += CompleteAnimation;
        //-------------------------------------------------------//
    }

    // Update is called once per frame
    void Update () {
        if (GameManager.Instance().PLAYER.NETWORK_INDEX != 1)
            return;
        Control();

        if (m_tutoMoveCk)
            TutoMoveCheck();

    }

    void CompleteAnimation(TrackEntry trackEntry)
    {
        if (trackEntry.animation.name.Equals(ANI_REPAIR))
        {
            m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.ATTACK);
            m_skletonAnimation.state.ClearTrack(1);
            if (m_damagePointFix != null)
            {
                m_damagePointFix.GetComponent<RoboDamagePoint>().DamageFix();
            }
        }
    }

    void ShowWeapon(bool v)
    {
        transform.GetChild(1).gameObject.SetActive(v);
    }

    // :: 튜토 세팅
    void TutoMoveCheck()
    {
        bool check = true;
        foreach(GameObject t in m_tutoMove)
        {
            if(!t.GetComponent<TutoMove>().SUCCESS)
            {
                check = false;
                t.SetActive(true);
                break;
            }
        }

        if(check)
        {
            m_tutoMoveCk = false;
            m_tuto.TutorialAction_ObjectInteraction("tuto_move");
            m_tutoRobo.GetComponent<TutoRobo>().CUR_STATE = 0;
            GameManager.Instance().ChangeScene(GameManager.PLACE.TUTORIAL_ROBO_IN);
            //CameraManager.Instance().MoveCamera(gameObject , GameSetting.CAMERA_ROBO ,
            //    CameraManager.CAMERA_PLACE.TUTORIAL_ROBO_IN);
        }
        
    }

    // :: 로봇 조종
    void ObjectRoboDriveControlCheck()
    {
        if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.CONTROL_DRIVE))
        {
            if (m_tutoRobo.GetComponent<TutoRobo>().CUR_STATE == 0)
            {
                m_tutoRobo.GetComponent<TutoRobo>().CUR_STATE = 1;
                GameManager.Instance().ChangeScene(GameManager.PLACE.TUTORIAL_ROBO);
                //CameraManager.Instance().MoveCamera(
                //    m_tutoRobo ,
                //    GameSetting.CAMERA_SPACE ,
                //    CameraManager.CAMERA_PLACE.TUTORIAL_ROBO_SPACE);
                m_tuto.TutorialAction_ObjectInteraction("drive_controller");
                m_tutoMoveCk = true;
            }
            else
            {
                m_tutoRobo.GetComponent<TutoRobo>().CUR_STATE = 0;
                GameManager.Instance().ChangeScene(GameManager.PLACE.TUTORIAL_ROBO_IN);
                //CameraManager.Instance().MoveCamera(gameObject , GameSetting.CAMERA_ROBO , 
                //    CameraManager.CAMERA_PLACE.TUTORIAL_ROBO_IN);
            }
        }
    }

    // :: 총 조종
    void ObjectRoboGunControlCheck()
    {
        if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.CONTROL_GUN))
        {
            if (m_tutoRobo.GetComponent<TutoRobo>().CUR_STATE == 0)
            {
                if(m_tutoEnemy != null)
                 m_tutoEnemy.SetActive(true);
                m_tutoRobo.GetComponent<TutoRobo>().CUR_STATE = 2;
                GameManager.Instance().ChangeScene(GameManager.PLACE.TUTORIAL_ROBO);
                //CameraManager.Instance().MoveCamera(
                //    m_tutoRobo ,
                //    GameSetting.CAMERA_SPACE ,
                //    CameraManager.CAMERA_PLACE.TUTORIAL_ROBO_SPACE);
                //   m_tuto.TutorialAction_ObjectInteraction("gun_controller");
            }
            else
            {
                m_tutoRobo.GetComponent<TutoRobo>().CUR_STATE = 0;
                GameManager.Instance().ChangeScene(GameManager.PLACE.TUTORIAL_ROBO_IN);
                //CameraManager.Instance().MoveCamera(gameObject , GameSetting.CAMERA_ROBO , CameraManager.CAMERA_PLACE.TUTORIAL_ROBO_IN);
            }
        }
    }

    // :: 행성 나가기
    void ObjectRoboOutDoorControlCheck()
    {
        if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.CONTROL_OUT_DOOR))
        {
            GameObject target = null;
            int cameraSize = 6;
        //    CameraManager.CAMERA_PLACE place = CameraManager.CAMERA_PLACE.ROBO_IN;
            string func = "";

            //switch (CameraManager.Instance().PLACE)
            //{
            //    case CameraManager.CAMERA_PLACE.ROBO_IN:
            //        target = CameraManager.Instance().m_inTheStar;
            //        place = CameraManager.CAMERA_PLACE.STAR;
            //        func = "RobotOutEnd";

            //        break;
            //    case CameraManager.CAMERA_PLACE.STAR:
            //        target = CameraManager.Instance().m_robotPlace;

            //        cameraSize = 4;
            //        place = CameraManager.CAMERA_PLACE.ROBO_IN;
            //        this.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
            //        func = "RobotInEnd";
            //        break;

            //}

            //CameraManager.Instance().MoveCameraAndObject(target , cameraSize , place , gameObject , gameObject , func , false);
    


        }
    }
    // ::::: 행성 관련 메소드
    void RobotOutEnd()
    {
        m_skletonAnimation.skeleton.SetSkin("char_01_a");
        m_skletonAnimation.skeleton.SetToSetupPose();
    }
    //들어올때
    void RobotInEnd()
    {
        m_skletonAnimation.skeleton.SetSkin("char_01");
        m_skletonAnimation.skeleton.SetToSetupPose();
        m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.LADDER);
    }

    // ::::::::::::::::::::::::::::

    // :: 힐링 존 
    void ObjectRoboHealControlCheck()
    {
        // 기능 구현해야함
        if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.CONTROL_HEAL))
        {
            m_userControlName = null;
            m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.CONTROL_HEAL);
            m_tuto.TutorialAction_Heal(100 , 100);
        }
    }

    // :: 에너지 충전
    void ObjectRoboEnergyControlCheck()
    {
        if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.CONTROL_ENERGY_CHARGE))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {

                if (iTween.Count(m_chargePad.gameObject) <= 0)
                {
                    iTween.MoveTo(m_chargePad.gameObject ,
                        iTween.Hash("y" , m_chargeTopObj.transform.position.y , "oncompletetarget" , gameObject ,
                        "oncomplete" , "PadTweenEnd" , "speed" , 4.0f));
                    GameManager.Instance().ROBO.ENERGY = GameManager.Instance().ROBO.ENERGY + 0.5f;
                }
                else
                {

                }
            }
        }
    }



    void PadTweenEnd()
    {
        m_tuto.TutorialAction_Charge(100,100);
        iTween.MoveTo(m_chargePad.gameObject ,
                         iTween.Hash("y" , m_chargeBottomObj.transform.position.y , "speed" , 4.0f));
    }

    // :: 인벤
    void ObjectRoboInvenControlCheck()
    {
        if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.CONTROL_INVEN))
        {
            m_userControlName = null;
            m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.CONTROL_INVEN);
        }
    }
    float VerticalMoveControl()
    {
        float moveY = 0.0f;
        float jump = 0.0f;
        if (Input.GetKey(KeyCode.W))
        {
            if (m_LadderState)
            {
                m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.MOVE);
                m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.LADDER);
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

            if (m_LadderState)
            {
                m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.MOVE);
                m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.LADDER);
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

    void ObjectControl()
    {
        // 특정 컨트롤 조작시에 조작키를 눌렀을 때 
        if (Input.GetKeyUp(KeyCode.R))
        {
            int state = -1;

            switch (m_userControlName)
            {
                case "ROBOT_HEAL": state = (int)Hero.HERO_STATE.CONTROL_HEAL; break;
                case "ROBOT_DRIVE": state = (int)Hero.HERO_STATE.CONTROL_DRIVE; break;
                case "ROBOT_INVEN": state = (int)Hero.HERO_STATE.CONTROL_INVEN; break;
                case "ROBOT_GUN": state = (int)Hero.HERO_STATE.CONTROL_GUN; break;
                case "ROBOT_OUT_DOOR": state = (int)Hero.HERO_STATE.CONTROL_OUT_DOOR; break;
                case "ROBOT_STATUSVIEW": break;
                case "ROBOT_ENERGY_CHARGE": state = (int)Hero.HERO_STATE.CONTROL_ENERGY_CHARGE; break;
            }
           // SoundManager.Instance().PlaySound(m_interaction);
            if (state > 0)
                m_curState = BitControl.Set(m_curState , state);

            // 실제 오브젝트 작동
            //로봇 조종 체크
            ObjectRoboDriveControlCheck();

            //로봇 총 조작 체크
            ObjectRoboGunControlCheck();

            //행성 나가기 체크
            ObjectRoboOutDoorControlCheck();

            // 힐링 존 체크
            ObjectRoboHealControlCheck();

            // 인벤 체크
            ObjectRoboInvenControlCheck();


        }
    }

    // 공격하기
    void AttackControl()
    {
        if (Input.GetKeyUp(KeyCode.Space) && !BitControl.Get(m_curState , (int)Hero.HERO_STATE.CONTROL_ENERGY_CHARGE))
        {
            m_curState = BitControl.Set(m_curState , (int)Hero.HERO_STATE.ATTACK);
        }
    }

    void Control()
    {
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

        // 공격 혹은 수리 
        AttackControl();
        // 충전
        ObjectRoboEnergyControlCheck();

        ObjectControl();



        // 이동 애니메이션 체크
        if ((BitControl.Get(m_curState , (int)Hero.HERO_STATE.MOVE) &&
            (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))))
        {

            m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.MOVE);
            if (!BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER))
            {
                CheckAndSetAnimation(ANI_IDLE , true);
                m_curState = (int)Hero.HERO_STATE.IDLE;
            }
            else
            {
                m_climb.state.ClearTrack(0);
            }
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
                if (!BitControl.Get(m_curState , (int)Hero.HERO_STATE.MOVE))
                    CheckAndSetAnimation(ANI_IDLE , true);
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
                if (IsCurrentAnimation(ANI_IDLE))// || IsCurrentAnimation(ANI_MOVE))
                {
                    //레디 -> 점핑 ㄱㄱ
                    m_skletonAnimation.state.SetAnimation(0 , ANI_JUMP_READY , false);
                    m_skletonAnimation.state.AddAnimation(0 , ANI_JUMP_JUMPING , true , 0f);
                }

            }

            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.JUMP_FALL))
            {
                //떨어지는 중
                if (!BitControl.Get(m_curState , (int)Hero.HERO_STATE.MOVE))
                    CheckAndSetAnimation(ANI_JUMP_FALL , false);
            }

            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.MOVE))
            {
                if (!IsCurrentAnimation(ANI_MOVE))
                {
                    m_skletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);//
                }
                this.transform.Translate(new Vector3(moveX , moveY));
            }

            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.LADDER))
            {
                m_skletonAnimation.enabled = false;
                this.GetComponent<MeshRenderer>().enabled = false;
                m_climb.gameObject.SetActive(true);

                if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.MOVE))
                {
                    if (m_climb.state.GetCurrent(0) != null &&
                        !m_climb.state.GetCurrent(0).animation.name.Equals("animation"))
                        m_climb.state.SetAnimation(0 , "animation" , true);
                    else if (m_climb.state.GetCurrent(0) == null)
                        m_climb.state.SetAnimation(0 , "animation" , true);
                    MDebug.Log("여기");
                }
            }

            //test
            if (BitControl.Get(m_curState , (int)Hero.HERO_STATE.ATTACK))
                m_skletonAnimation.state.SetAnimation(1 , ANI_REPAIR , false);
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
    //-- (행성/로봇내부) 충돌 -----------------------------------------------------------------------------------------------//

    //사다리용
    void OnTriggerEnter2D(Collider2D col)
    {
        MDebug.Log("Enter " + col.tag);

        if (col.tag.Equals("DAMAGE_POINT"))
        {
            m_damagePointFix = col.gameObject;
        }

        if (col.transform.tag.Equals("LADDER"))
        {
            ShowWeapon(false);
            m_LadderState = true;
        }
        else if (!string.IsNullOrEmpty(col.transform.tag))
            m_userControlName = col.transform.tag;


    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!col.transform.tag.Equals("LADDER") && !string.IsNullOrEmpty(col.transform.tag))
            m_userControlName = col.transform.tag;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        MDebug.Log("Exit " + col.tag);

        if (m_damagePointFix != null && m_damagePointFix.Equals(col.gameObject))
            m_damagePointFix = null;

        if (!string.IsNullOrEmpty(col.transform.tag))
            m_userControlName = null;

        if (col.tag == "LADDER")
        {
            ShowWeapon(true);
            m_LadderState = false;
            m_rigidBody.gravityScale = 1.0f;
            m_curState = BitControl.Clear(m_curState , (int)Hero.HERO_STATE.LADDER);

            // 사다리
            m_skletonAnimation.enabled = true;
            this.GetComponent<MeshRenderer>().enabled = true;
            if (m_climb == null)
                return;
            if(m_climb.state.GetCurrent(0) != null)
                m_climb.state.ClearTrack(0);
            m_climb.gameObject.SetActive(false);
        }

    }
}
