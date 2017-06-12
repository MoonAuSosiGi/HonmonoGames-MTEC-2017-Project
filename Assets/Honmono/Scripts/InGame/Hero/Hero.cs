using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;

using Spine.Unity;
public class Hero : MonoBehaviour, NetworkManager.NetworkMoveEventListener , NetworkManager.NetworkMessageEventListenrer
{

    //4 byte 00000000 00000000 00000000 00000000
    public enum HERO_STATE
    {
        IDLE = 0,
        ATTACK = 1,
        LADDER = 2,
        MOVE = 3,
        JUMP = 4,
        JUMP_FALL = 5,
        CONTROL_GUN = 6,
        CONTROL_DRIVE = 7,
        CONTROL_INVEN = 8,
        CONTROL_HEAL = 9,
        CONTROL_OUT_DOOR = 10,
        CONTROL_ENERGY_CHARGE = 11

    }
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


   
    // 기본 정보 --------------------------------------------------------------------------------------//

    private int m_hp = 10;

    public int HP {
        get { return m_hp; }
        set
        {
            m_hp = value;
            GameManager.Instance().ChangeCharacterHP(m_hp , GameSetting.HERO_MAX_HP);
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCharacterHPUpdate(
                m_hp , GameSetting.HERO_MAX_HP));
        }
    }

    // sound
    public AudioClip m_walkSound = null;
    public AudioClip m_interaction = null;
    public AudioClip m_repair = null;
    public AudioClip m_repairFin = null;
    public AudioClip m_robotEnter = null;
    public AudioClip m_robotExit = null;

    // 우주공간?
    public bool m_inSpace = true;

    public bool IN_SPACE { get { return m_inSpace; } set { m_inSpace = value; } }
    // -- 데미지 쿨타임
    bool m_damageCoolTime = false;
    MeshRenderer m_meshRenderer = null;

    // 렌더러
    private SkeletonAnimation m_skletonAnimation = null;
    // 로봇과의 렌더링
    private BansheeGz.BGSpline.Curve.BGCurve m_bgCurve = null;
    // 점프시 가할 힘
    private float m_jumpPower = 300.0f;

    // 이동 속도
    private float m_moveSpeed = 6.0f;

    //현재 상태 설정
    private int m_curState = (int)HERO_STATE.IDLE;

    // 리지드바디
    private Rigidbody2D m_rigidBody = null;
    
    // 자기자신 판별용
    [SerializeField]
    public bool m_isMe = true;

    [SerializeField]
    private string m_userName = "";

    private string m_userControlName = "";
    private Monster m_monster = null;
    private GameObject m_damagePointFix = null;

    //기존 위치
    Vector3 m_prevPos = Vector3.zero;
    // USER NAME
    public string USERNAME {
        get { return m_userName; }
        set { m_userName = value; }
    }

    public bool IS_PLAYER
    {
        get { return m_isMe; }
        set
        {
            m_isMe = value;

            if (m_isMe)
            {
                GameManager.Instance().HeroSetup(this);
                m_rigidBody.gravityScale = (m_inSpace) ? 0.0f : 1.0f;
            }
        }
    }


    // -- 사다리용 ------------------------------------------------------------------------------------------------------------//
    private SkeletonAnimation m_climb = null;
    private bool m_LadderState = false;

    // -- 에너지 차지 ---------------------------------------------------------------------------------------------------------//
    public SpriteRenderer m_chargePad = null;
    public GameObject m_chargeTopObj = null;
    public GameObject m_chargeBottomObj = null;
   
    // -- NetworkMessage ------------------------------------------------------------------------------------------------------//
    private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_startPos = Vector3.zero;

    //charge
    private Vector3 m_prevChargePos = Vector3.zero;
    private float m_lastSendTime_charge = 0.0f;
    private float m_lastSendTime = 0.0f;

    // state
    private int m_prevState = 0;
    //private float m_delay = 0.0f;

    Vector3 m_recvPrevPos = Vector3.zero;
    Vector3 m_veloCity = Vector3.zero;
    //--네트워크--------------------------------------------------------------------------------------------------------------//

    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        if (m_isMe)
            return;
        JSONObject obj = json;
        JSONObject users = obj.GetField("Users");

        // 메시지 형태 
        //{"Users":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}

        float x = 0.0f, y = 0.0f, z = 0.0f;
        bool flip = false;
        bool ck = false;
        Vector3 drPos = Vector3.zero;
        Vector3 targetPos = Vector3.zero;
        for(int i = 0; i < users.Count; i++)
        {
            if(users[i].GetField(NetworkManager.USERNAME).str.Equals(m_userName))
            {
                
                x = users[i].GetField("X").f;
                y = users[i].GetField("Y").f;
                z = users[i].GetField("Z").f;
                flip = users[i].GetField(NetworkManager.DIR).b;
                ck = true;
                JSONObject v = users[i].GetField(NetworkManager.DIRVECTOR);
                drPos = new Vector3(v.GetField("X").f , v.GetField("Y").f , v.GetField("Z").f);
                break;
            }
        }
        
        // 유저 이름과 같으면 할 필요가 없다
        if (!ck)  return;

        // 지금 메시지를 받은 이 객체와의 공간 판별 후 불일치시 아무것도 하지 않음
        int areaCheck = (int)z;

        switch(areaCheck)
        {
            case (int)NetworkOrderController.AreaInfo.AREA_SPACE:
                if (!m_inSpace) return; break;
            case (int)NetworkOrderController.AreaInfo.AREA_ROBOT:
                if (m_inSpace) return; break;
        }
        Vector3 newPos = new Vector3(x , y);


        // 기존 방향과 다르다면!?
        if (this.m_skletonAnimation.skeleton.flipX != flip)
        {
            this.m_skletonAnimation.skeleton.flipX = flip;
            targetPos = newPos;
        }
        else
            targetPos = drPos;

        m_targetPos = targetPos;
    }
    // Order 받는용
    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {

        // 상태 체인지
        if (e.msgType == NetworkManager.STATE_CHANGE && (e.targetName).Equals(m_userName))
        {
            if (m_isMe)
                return;
            m_curState = (int)e.msg.GetField(NetworkManager.STATE_CHANGE).i;
        }
        // 로봇 조종자            
        else if(e.msgType == NetworkManager.ROBOT_DRIVER)
        {
            if (e.msg.GetField(NetworkManager.ROBOT_DRIVER).b)
            {

                GameManager.Instance().ROBO.MOVE_PLYAER = e.user;

                //test
               

                if (m_isMe && m_userName.Equals(e.user + "_robo"))
                    GameManager.Instance().ChangeScene(GameManager.PLACE.ROBO,gameObject, "InteractionEffect",false);


            }
            else
            {
                if (m_isMe && m_userName.Equals(e.user + "_robo"))
                {
                    m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.CONTROL_DRIVE);
                    GameManager.Instance().ChangeScene(GameManager.PLACE.ROBO_IN_DRIVE);
                    GameManager.Instance().HeroSetup(this);
                }
               GameManager.Instance().ROBO.MOVE_PLYAER = null;

            }

        }
        // 이녀석은 총기를 조작하는 사람
        else if (e.msgType == NetworkManager.ROBOT_GUNNER)
        {

            if (e.msg.GetField(NetworkManager.ROBOT_GUNNER).b)
            {
                GameManager.Instance().ROBO.GUN_PLAYER = e.user;
                
                if(m_isMe && m_userName.Equals(e.user + "_robo"))
                    GameManager.Instance().ChangeScene(GameManager.PLACE.ROBO , gameObject , "InteractionEffect" , false);
            }
            else
            {
                if (m_isMe && m_userName.Equals(e.user + "_robo"))
                {
                    m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.CONTROL_GUN);
                    GameManager.Instance().ChangeScene(GameManager.PLACE.ROBO_IN_GUN);
                    GameManager.Instance().HeroSetup(this);
                }

                GameManager.Instance().ROBO.GUN_PLAYER = null;

            }
        }
      
    }

    void InteractionEffect()
    {
        PopupManager.Instance().AddPopup("InteractionPopup");
    }
    //------------------------------------------------------------------------------------------------------------------------//

    void Awake()
    {
        

        //-- 필요 컴포넌트 받아오기-------------------------------//
        m_skletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_bgCurve = this.GetComponent<BansheeGz.BGSpline.Curve.BGCurve>();
        m_rigidBody = this.GetComponent<Rigidbody2D>();
        m_climb = transform.GetChild(2).GetComponent<SkeletonAnimation>();
        m_meshRenderer = this.GetComponent<MeshRenderer>();
        //-------------------------------------------------------//

    }
    // Use this for initialization
    void Start()
    {
        // 움직였을 때만 패킷을 전송해야 한다. 그러기 위한 디스턴스 판별용 포지션 적용
        m_prevPos = transform.position;

        m_skletonAnimation.state.Complete += CompleteAnimation;

        // 네트워크 이벤트 옵저버 등록

        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        NetworkManager.Instance().AddNetworkMoveEventListener(this);


        m_skletonAnimation.state.SetAnimation(0, ANI_IDLE, true);

        m_skletonAnimation.state.Data.SetMix(ANI_MOVE , ANI_IDLE , 0.25f);
        m_skletonAnimation.state.Data.SetMix(ANI_MOVE , ANI_REPAIR , 0.6f);
    }

    void OnEnable()
    {
        if (!m_isMe)
            transform.GetChild(0).GetComponent<TextMesh>().text = USERNAME.Split('_')[0];


        if (!m_isMe)
            return;

        if(m_isMe)
            transform.GetChild(0).GetComponent<TextMesh>().text = GameManager.Instance().PLAYER.USER_NAME;
        Vector3 pos = transform.position;
        float area = (float)((m_inSpace) ? (int)NetworkOrderController.AreaInfo.AREA_SPACE : (int)NetworkOrderController.AreaInfo.AREA_ROBOT);
        NetworkManager.Instance().SendMoveMessage(
          JSONMessageTool.ToJsonMove(
              m_userName ,
              pos.x , pos.y ,
              1 , // :: Area 선택해서 날림
              false,
              m_prevPos));
    }

    void OnDisable()
    {
        if (!m_isMe)
            return;
    }

    void CompleteAnimation(TrackEntry trackEntry)
    {
        if (!m_isMe)
            return;

        if (trackEntry.animation.name.Equals(ANI_REPAIR))
        {
            m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.ATTACK);
            m_skletonAnimation.state.ClearTrack(1);
            if(m_damagePointFix != null)
            {
                m_damagePointFix.GetComponent<RoboDamagePoint>().DamageFix();
            }
            else if(m_monster != null)
            {
                m_monster.Damage(1);
            }
        }
    }

    void Update()
    {
        m_prevPos = transform.position;
        m_prevState = m_curState;

        if (NetworkOrderController.OBSERVER_MODE)
        { 
            if(m_rigidBody != null)
                m_rigidBody.simulated = false;
        }

        if (m_isMe)
        {
            

            if (!m_inSpace)
                Control();
            else
                ControlInSpace();
            StateSend();

        }
        else
        {
            NetworkMoveLerp();
            NetworkAnimation();
        }

    }

    //-- Network Message 에 따른 이동 보간 ( 네트워크 플레이어 ) ------------------------------------------------------------//
    void NetworkMoveLerp()
    {
        transform.position = m_targetPos;
    }

    // -- Network Message 에 따른 애니메이션 처리 ---------------------------------------------------------------------------//
    void NetworkAnimation()
    {
        if (m_curState == (int)HERO_STATE.IDLE)
        {
            CheckAndSetAnimation(ANI_IDLE , true);
            //m_skletonAnimation.state.SetAnimation(0,ANI_IDLE, true);
        }
        else
        {
            if (BitControl.Get(m_curState, (int)HERO_STATE.JUMP))
            {
                // IDLE / MOVE 상태일 때만 점프 애니메이션 
                if (IsCurrentAnimation(ANI_IDLE) || IsCurrentAnimation(ANI_MOVE))
                {
                    //레디 -> 점핑 ㄱㄱ
                    m_skletonAnimation.state.SetAnimation(0, ANI_JUMP_READY, false);
                    m_skletonAnimation.state.AddAnimation(0, ANI_JUMP_JUMPING, true, 0f);
                }

            }

            if (BitControl.Get(m_curState, (int)HERO_STATE.JUMP_FALL))
            {
                //떨어지는 중
                if (!BitControl.Get(m_curState , (int)HERO_STATE.MOVE))
                    CheckAndSetAnimation(ANI_JUMP_FALL, false);
            }

            if (BitControl.Get(m_curState, (int)HERO_STATE.MOVE))
            {
                if (!IsCurrentAnimation(ANI_MOVE))
                {
                    m_skletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);//
                }
            }
            if (BitControl.Get(m_curState , (int)HERO_STATE.LADDER))
            {
                m_skletonAnimation.enabled = false;
                this.GetComponent<MeshRenderer>().enabled = false;
                m_climb.gameObject.SetActive(true);

                if (BitControl.Get(m_curState , (int)HERO_STATE.MOVE))
                {
                    if (m_climb.state.GetCurrent(0) != null &&
                        !m_climb.state.GetCurrent(0).animation.name.Equals("animation"))
                        m_climb.state.SetAnimation(0 , "animation" , true);
                    else if (m_climb.state.GetCurrent(0) == null)
                        m_climb.state.SetAnimation(0 , "animation" , true);
                }
                else
                {
                    if(m_climb.state != null)
                        m_climb.state.ClearTrack(0);
                }
            }
            else
            {
                m_skletonAnimation.enabled = true;
                this.GetComponent<MeshRenderer>().enabled = true;
                m_climb.gameObject.SetActive(false);
            }

            if (BitControl.Get(m_curState , (int)HERO_STATE.ATTACK))
                m_skletonAnimation.state.SetAnimation(1 , ANI_REPAIR , false);
        }
        
    }
    //-----------------------------------------------------------------------------------------------------------------------//

    //-- 오브젝트  조작 -----------------------------------------------------------------------------------------------------//
    // :: 로봇 조종
    void ObjectRoboDriveControlCheck()
    {
        if (BitControl.Get(m_curState , (int)HERO_STATE.CONTROL_DRIVE))
        {
            if (string.IsNullOrEmpty(GameManager.Instance().ROBO.MOVE_PLYAER))
            {
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderRobotSetting());
            }
            else
            {
                //탈출
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderRobotSetting(false));
            }
        }
    }

    // :: 총 조종
    void ObjectRoboGunControlCheck()
    {
        if (BitControl.Get(m_curState , (int)HERO_STATE.CONTROL_GUN))
        {
            if (string.IsNullOrEmpty(GameManager.Instance().ROBO.GUN_PLAYER))
            {
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonOrderRobotGunSetting());
            }
            else
            {
                // 탈출
                NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonOrderRobotGunSetting(false));
            }
        }
    }

    // :: 행성 나가기
    void ObjectRoboOutDoorControlCheck()
    {
        if (BitControl.Get(m_curState , (int)HERO_STATE.CONTROL_OUT_DOOR))
        {

            string func = "";

            switch (GameManager.Instance().SCENE_PLACE)
            {
                case GameManager.PLACE.ROBO_IN:
                case GameManager.PLACE.ROBO_IN_DRIVE:
                case GameManager.PLACE.ROBO_IN_GUN:
                    func = "RobotOutEnd";

                    GameManager.PLACE place = GameManager.PLACE.PLANET1;
                    switch(GameManager.Instance().ROBO.ROBO_PLACE_NAME)
                    {
                        case "PLANET1": place = GameManager.PLACE.PLANET1; break;
                        case "PLANET2": place = GameManager.PLACE.PLANET2; break;
                    }
                    GameManager.Instance().ChangeScene(place , gameObject , func);
                    break;
                case GameManager.PLACE.PLANET:
                case GameManager.PLACE.PLANET1:
                case GameManager.PLACE.PLANET2:
                    this.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
                    func = "RobotInEnd";

                    GameManager.Instance().ChangeScene(GameManager.PLACE.ROBO_IN_DRIVE , gameObject , func);
                    break;

            }
        }
    }
    // ::::: 행성 관련 메소드
    void RobotOutEnd()
    {
        m_skletonAnimation.skeleton.SetSkin(GameManager.Instance().GetCharacterSuitSkin(
            GameManager.Instance().PLAYER.SKELETON_DATA_ASSET));
        m_skletonAnimation.skeleton.SetToSetupPose();
    }
    //들어올때
    void RobotInEnd()
    {
        m_skletonAnimation.skeleton.SetSkin(GameManager.Instance().GetCharacterNormalSkin(
            GameManager.Instance().PLAYER.SKELETON_DATA_ASSET));
        m_skletonAnimation.skeleton.SetToSetupPose();
        //m_curState = BitControl.Set(m_curState , (int)HERO_STATE.LADDER);

        //m_LadderState = false;
        //m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.LADDER);
        //m_skletonAnimation.enabled = true;
        //this.GetComponent<MeshRenderer>().enabled = true;
        //m_climb.state.ClearTrack(0);
        //m_climb.gameObject.SetActive(false);
    }

    // ::::::::::::::::::::::::::::

    // :: 힐링 존 
    void ObjectRoboHealControlCheck()
    {
        // 기능 구현해야함
        if (BitControl.Get(m_curState , (int)HERO_STATE.CONTROL_HEAL))
        {
            m_userControlName = null;
            m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.CONTROL_HEAL);
        }
    }

    // :: 에너지 충전
    void ObjectRoboEnergyControlCheck()
    {
        if(BitControl.Get(m_curState,(int)HERO_STATE.CONTROL_ENERGY_CHARGE))
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
               
                if (iTween.Count(m_chargePad.gameObject) <= 0)
                {
                    iTween.MoveTo(m_chargePad.gameObject ,
                        iTween.Hash("y" , m_chargeTopObj.transform.position.y , "oncompletetarget" , gameObject ,
                        "oncomplete" , "PadTweenEnd" , "speed" , 4.0f,"onupdatetarget",gameObject,"onupdate", "ChargeMoveSend"));
                    GameManager.Instance().ROBO.ENERGY = GameManager.Instance().ROBO.ENERGY + 0.5f;
                }
                else
                {
                    
                }
            }
        }
    }

    void ChargeMoveSend()
    {
        if (string.IsNullOrEmpty(m_userName) || string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME))
            return;
     
        Vector3 pos =  m_chargePad.gameObject.transform.position;
        float distance = Vector3.Distance(m_prevChargePos , pos);
        
        Vector3 velocity = (m_chargePad.transform.position - m_prevChargePos) / Time.deltaTime;
        Vector3 sendPos = m_prevChargePos + (velocity * (Time.deltaTime - m_lastSendTime_charge));
        NetworkManager.Instance().SendEnemyMoveMessage(
           JSONMessageTool.ToJsonEnemyMove(
               NetworkOrderController.ORDER_NAME + "_pad" ,
               pos.x , pos.y ,
               0 , // :: Area 선택해서 날림
               false ,
               new Vector3(pos.x,pos.y,-1.0f)));
        m_lastSendTime_charge = Time.deltaTime;
    }

    void PadTweenEnd()
    {
        iTween.MoveTo(m_chargePad.gameObject ,
                         iTween.Hash("y" , m_chargeBottomObj.transform.position.y,"speed",4.0f
                         ,"onupdatetarget" , gameObject , "onupdate" , "ChargeMoveSend"));
    }

    // :: 인벤
    void ObjectRoboInvenControlCheck()
    {
        if (BitControl.Get(m_curState , (int)HERO_STATE.CONTROL_INVEN))
        {
            m_userControlName = null;
            m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.CONTROL_INVEN);
        }
    }

    //-- 실 캐릭터 조작 -----------------------------------------------------------------------------------------------------//

    // 수직 이동 및 점프
    float VerticalMoveControl()
    {
        float moveY = 0.0f;
        float jump = 0.0f;
        if (Input.GetKey(KeyCode.W))
        {
            if (m_LadderState)
            {
                m_curState = BitControl.Set(m_curState , (int)HERO_STATE.MOVE);
                m_curState = BitControl.Set(m_curState , (int)HERO_STATE.LADDER);
                moveY = m_moveSpeed * Time.deltaTime;
                m_rigidBody.gravityScale = 0.0f;
                m_rigidBody.velocity = Vector2.zero;

                m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.JUMP);
                m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.JUMP_FALL);
            }
            else if (!BitControl.Get(m_curState , (int)HERO_STATE.JUMP)
                && !BitControl.Get(m_curState , (int)HERO_STATE.JUMP_FALL))
            {
                // 특정 오브젝트 밟자마자 (점프 안끝났는데) 점프하는 경우 방지
                if (m_rigidBody.velocity.y <= 0.0f)
                {
                    m_curState = BitControl.Set(m_curState , (int)HERO_STATE.JUMP);
                    jump = m_jumpPower;
                }

            }
        }

        if (Input.GetKey(KeyCode.S))
        {

            if (m_LadderState)
            {
                m_curState = BitControl.Set(m_curState , (int)HERO_STATE.MOVE);
                m_curState = BitControl.Set(m_curState , (int)HERO_STATE.LADDER);
                moveY = -m_moveSpeed * Time.deltaTime;
                m_rigidBody.gravityScale = 0.0f;
                m_rigidBody.velocity = Vector2.zero;

                m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.JUMP);
                m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.JUMP_FALL);
            }
            else
            {
                //딱히 하는거 없음
            }
        }
        return (BitControl.Get(m_curState,(int)HERO_STATE.JUMP)) ? jump  : moveY;
    }

    // 가로 이동
    float HorizontalMoveControl()
    {
        float moveX = 0.0f;

        if (Input.GetKey(KeyCode.A))
        {
            m_curState = BitControl.Set(m_curState , (int)HERO_STATE.MOVE);

            moveX = -m_moveSpeed * Time.deltaTime;

            this.m_skletonAnimation.skeleton.flipX = false;
            //this.GetComponent<SpriteRenderer>().flipX = false;
        }



        if (Input.GetKey(KeyCode.D))
        {
            m_curState = BitControl.Set(m_curState , (int)HERO_STATE.MOVE);
            moveX = m_moveSpeed * Time.deltaTime;

            this.m_skletonAnimation.skeleton.flipX = true;

        }
        return moveX;
    }

    //특정 오브젝트 조작시
    void ObjectControl()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            int state = -1;

            switch (m_userControlName)
            {
                case "ROBOT_HEAL": state = (int)HERO_STATE.CONTROL_HEAL; break;
                case "ROBOT_DRIVE": state = (int)HERO_STATE.CONTROL_DRIVE; break;
                case "ROBOT_INVEN": state = (int)HERO_STATE.CONTROL_INVEN; break;
                case "ROBOT_GUN": state = (int)HERO_STATE.CONTROL_GUN; break;
                case "ROBOT_OUT_DOOR": state = (int)HERO_STATE.CONTROL_OUT_DOOR; break;
                case "ROBOT_STATUSVIEW": break;
                case "ROBOT_ENERGY_CHARGE": state = (int)HERO_STATE.CONTROL_ENERGY_CHARGE; break;
            }
            SoundManager.Instance().PlaySound(m_interaction);
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
        if (Input.GetKeyUp(KeyCode.Space) && !BitControl.Get(m_curState,(int)HERO_STATE.CONTROL_ENERGY_CHARGE))
        {
            m_curState = BitControl.Set(m_curState , (int)HERO_STATE.ATTACK);
        }
    }
    // 중력이 적용되는 곳에서의 컨트롤 
    void Control()
    {
        Vector2 pos = transform.position;

        //상황 설정
        // 좌 - 우 이동  상  점프 & 사다리  하 사다리
        float moveX = 0.0f;
        float moveY = 0.0f;
        float jump = 0.0f;

        // 가로이동
        moveX = HorizontalMoveControl();

        // 점프 및 수직 이동
        if (BitControl.Get(m_curState , (int)HERO_STATE.LADDER))
            moveY = VerticalMoveControl();
        else
            jump = VerticalMoveControl();

        // 공격 혹은 수리 
        AttackControl();
        // 충전
        ObjectRoboEnergyControlCheck();

        ObjectControl();
       

        // 이동 애니메이션 체크
        if ((BitControl.Get(m_curState, (int)HERO_STATE.MOVE) && 
            (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))))
        {

            m_curState = BitControl.Clear(m_curState , (int)HERO_STATE.MOVE);
            if (!BitControl.Get(m_curState , (int)HERO_STATE.LADDER))
            {
                CheckAndSetAnimation(ANI_IDLE , true);
                m_curState = (int)HERO_STATE.IDLE;
            }
            else
            {
                m_climb.state.ClearTrack(0);
            }
        }

        // 떨어지는 상태임

        if (!BitControl.Get(m_curState, (int)HERO_STATE.LADDER))
        {
            if (m_rigidBody.velocity.y < 0.0f)
            {
                m_curState = BitControl.Clear(m_curState, (int)HERO_STATE.JUMP);
                m_curState = BitControl.Set(m_curState, (int)HERO_STATE.JUMP_FALL);
            }
            if (m_rigidBody.velocity.y >= 0.0f)
            {
                m_curState = BitControl.Clear(m_curState, (int)HERO_STATE.JUMP_FALL);
                //t
                if(!BitControl.Get(m_curState,(int)HERO_STATE.MOVE))
                    CheckAndSetAnimation(ANI_IDLE , true);
            }
        }
        
      

        // -- 상태값에 따라 애니메이션 처리 및 이동 점프 처리 ----------------------------------------

        if (m_curState == (int)HERO_STATE.IDLE)
        {
            CheckAndSetAnimation(ANI_IDLE, true);
        }
        else
        {
            if (BitControl.Get(m_curState, (int)HERO_STATE.JUMP))
            {
                // 점프 ㄱㄱ
                this.m_rigidBody.AddForce(new Vector2(0, jump));
                // IDLE / MOVE 상태일 때만 점프 애니메이션 
                if (IsCurrentAnimation(ANI_IDLE))// || IsCurrentAnimation(ANI_MOVE))
                {
                    //레디 -> 점핑 ㄱㄱ
                    m_skletonAnimation.state.SetAnimation(0, ANI_JUMP_READY, false);
                    m_skletonAnimation.state.AddAnimation(0, ANI_JUMP_JUMPING, true, 0f);
                }

            }
            
            if(BitControl.Get(m_curState,(int)HERO_STATE.JUMP_FALL))
            {
                //떨어지는 중
                if (!BitControl.Get(m_curState , (int)HERO_STATE.MOVE))
                    CheckAndSetAnimation(ANI_JUMP_FALL, false);
            }

            if (BitControl.Get(m_curState, (int)HERO_STATE.MOVE))
            {
                if (!IsCurrentAnimation(ANI_MOVE))
                {
                    m_skletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);//
                }
                this.transform.Translate(new Vector3(moveX, moveY));
            }

            if(BitControl.Get(m_curState , (int)HERO_STATE.LADDER))
            {
                m_skletonAnimation.enabled = false;
                this.GetComponent<MeshRenderer>().enabled = false;
                m_climb.gameObject.SetActive(true);

                if(BitControl.Get(m_curState,(int)HERO_STATE.MOVE))
                {
                    if (m_climb.state.GetCurrent(0) != null &&
                        !m_climb.state.GetCurrent(0).animation.name.Equals("animation"))
                        m_climb.state.SetAnimation(0 , "animation" , true);
                    else if (m_climb.state.GetCurrent(0) == null)
                        m_climb.state.SetAnimation(0 , "animation" , true);
                }
            }

            //test
            if(BitControl.Get(m_curState,(int)HERO_STATE.ATTACK))
                m_skletonAnimation.state.SetAnimation(1 , ANI_REPAIR , false);
        }
        Debug.DrawLine(pos, new Vector3(pos.x,pos.y - GetComponent<BoxCollider2D>().bounds.size.y,0.0f),Color.red);

        MoveSend();
    }


    void ControlInSpace()
    {
        Vector2 pos = transform.position;

        //상황 설정
        // 좌 - 우 이동  상  점프 & 사다리  하 사다리
        float moveX = 0.0f;
        float moveY = 0.0f;

        if (Input.GetKey(KeyCode.A))
        {
            m_curState = BitControl.Set(m_curState, (int)HERO_STATE.MOVE);

            moveX = -m_moveSpeed * Time.deltaTime;
            this.m_skletonAnimation.skeleton.flipX = false;
            //this.GetComponent<SpriteRenderer>().flipX = false;
        }



        if (Input.GetKey(KeyCode.D))
        {
            m_curState = BitControl.Set(m_curState, (int)HERO_STATE.MOVE);
            moveX = m_moveSpeed * Time.deltaTime;
            this.m_skletonAnimation.skeleton.flipX = true;
            
            //this.GetComponent<SpriteRenderer>().flipX = true;

        }

        if (Input.GetKey(KeyCode.W))
        {
            m_curState = BitControl.Set(m_curState, (int)HERO_STATE.MOVE);
            moveY = m_moveSpeed * Time.deltaTime;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            m_curState = BitControl.Set(m_curState, (int)HERO_STATE.MOVE);
            moveY = -m_moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            // 요부분에서 사다리를 체크함

            if (BitControl.Get(m_curState, (int)HERO_STATE.MOVE))
            {
                moveY = -m_moveSpeed * Time.deltaTime;
            }
            else
            {
                //딱히 하는거 없음
            }
        }

        // 상호작용 키
        if (Input.GetKey(KeyCode.R))
        {

            //this.m_animator.SetBool("Move", false);
        }

        // 이동 애니메이션 체크
        if (BitControl.Get(m_curState, (int)HERO_STATE.MOVE) && 
            (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) ||
            (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))))
        {
            m_curState = BitControl.Clear(m_curState, (int)HERO_STATE.MOVE);

        }

        // 최종 

        if (m_curState == (int)HERO_STATE.IDLE)
        {
            CheckAndSetAnimation(ANI_IDLE, true);
        }
        else
        {
            if (BitControl.Get(m_curState, (int)HERO_STATE.MOVE))
            {
                if (IsCurrentAnimation(ANI_IDLE))
                {
                    m_skletonAnimation.state.SetAnimation(0, ANI_MOVE, true);
                }
                this.transform.Translate(new Vector3(moveX, moveY));
            }
        }
        
        
//        MoveSend();
    }


    // -- 스파인 애니메이션용 -------------------------------------------------------//
    bool IsCurrentAnimation(string ani)
    {
        if (m_skletonAnimation == null)
            return false;
        return m_skletonAnimation.state.GetCurrent(0).animation.name == ani;
    }
   
    void CheckAndSetAnimation(string ani,bool loop)
    {
        if (IsCurrentAnimation(ani) == false)
            m_skletonAnimation.state.SetAnimation(0, ani, loop);
    }

    //-------------------------------------------------------------------------------//
    void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3. Distance(m_prevPos, pos);

        float area = (float)((m_inSpace) ? (int)NetworkOrderController.AreaInfo.AREA_SPACE : (int)NetworkOrderController.AreaInfo.AREA_ROBOT);
        
        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + ( velocity * (Time.deltaTime - m_lastSendTime));
        
        NetworkManager.Instance().SendMoveMessage(
            JSONMessageTool.ToJsonMove(
                m_userName,
                pos.x, pos.y,
                area, // :: Area 선택해서 날림
                m_skletonAnimation.skeleton.flipX,
                sendPos));
        m_lastSendTime = Time.deltaTime;

    }

    void StateSend()
    {
        if (m_prevState != m_curState)
        {
           // if (!BitControl.Get(m_curState , (int)HERO_STATE.IDLE))
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonOrderStateValueChange(
                        m_userName , m_curState));
        }
    }

    //-- (행성/로봇내부) 충돌 -----------------------------------------------------------------------------------------------//

    //사다리용
    void OnTriggerEnter2D(Collider2D col)
    {
        MDebug.Log("Enter " + col.tag);

        if(col.tag.Equals("DAMAGE_POINT"))
        {
            m_damagePointFix = col.gameObject;
        }
        else if(col.tag.Equals("ENEMY"))
        {
            m_monster = col.GetComponent<Monster>();
        }

        if(col.transform.tag.Equals("LADDER"))
        {
            m_LadderState = true;
        }
        else if (!string.IsNullOrEmpty(col.transform.tag))
            m_userControlName = col.transform.tag;

        
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!col.transform.tag.Equals("LADDER") && !string.IsNullOrEmpty(col.transform.tag))
            m_userControlName = col.transform.tag;

        if (col.transform.tag == "LADDER" &&
            (BitControl.Get(m_curState, (int)HERO_STATE.JUMP) ||
            BitControl.Get(m_curState, (int)HERO_STATE.JUMP_FALL)))
        {

        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        MDebug.Log("Exit " + col.tag);

        if (m_damagePointFix != null && m_damagePointFix.Equals(col.gameObject))
            m_damagePointFix = null;

        if (!string.IsNullOrEmpty(col.transform.tag))
            m_userControlName = null;

        if (m_monster != null && col.transform.tag.Equals("ENEMY"))
            m_monster = null;

        if (col.tag == "LADDER")
        {
            m_LadderState = false;
            m_rigidBody.gravityScale = 1.0f;
            m_curState = BitControl.Clear(m_curState, (int)HERO_STATE.LADDER);

            // 사다리
            m_skletonAnimation.enabled = true;
            this.GetComponent<MeshRenderer>().enabled = true;
            
            try
            {
                if (m_climb.state.GetCurrent(0) != null)
                    m_climb.state.ClearTrack(0);
                m_climb.gameObject.SetActive(false);
            }
            catch(Exception)
            {

            }
            
        }
        
    }

    bool OrderCheckAndReturn()
    {
        return (m_userName == NetworkOrderController.ORDER_NAME);
    }

    void OnApplicationQuit()
    {
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderUserLogOut(
            GameManager.Instance().PLAYER.NETWORK_INDEX ,
            GameManager.Instance().PLAYER.STATUS ,
            GameManager.Instance().PLAYER.USER_NAME ,
            false));
    }




    public void Damage(int damage,bool left = true,float power = 100.0f)
    {
        if (m_damageCoolTime)
            return;
        HP -= damage;
        m_damageCoolTime = true;
        

        if (m_rigidBody != null)
        {
            float xpower = (!left) ? -power : power;  
            m_rigidBody.AddForce(new Vector2(xpower , m_jumpPower * 0.5f));

            InvokeRepeating("DamageEffect" ,0.1f, 0.1f);
        }
    }

    void DamageEffect()
    {
        Color color = m_meshRenderer.material.color;

        color.g -= 0.5f;
        color.b -= 0.5f;
        m_meshRenderer.material.color = color;
        if(color.g <= 0.0f)
        {
            CancelInvoke("DamageEffect");
            InvokeRepeating("DamageEffectEnd" , 0.1f , 0.1f);
        }
    }
    void DamageEffectEnd()
    {
        Color color = m_meshRenderer.material.color;

        color.g += 0.3f;
        color.b += 0.3f;
        m_meshRenderer.material.color = color;
        if (color.g >= 1.0f)
        {
            CancelInvoke("DamageEffectEnd");
            m_damageCoolTime = false;
        }
    }
}
