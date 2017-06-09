using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using System;

public class Stage1BOSS : Monster, NetworkManager.NetworkMessageEventListenrer
{
    // -- Network ------------------------------------------------------------------//
    private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_prevPos = Vector3.zero;
    
    float m_lastSendTime = 0.0f;
    float m_angle = 0.0f;
    
    // ----------------------------------------------------------------------------//

    // -- 기본 정보 --------------------------------------------------------------------------------//

    GameObject m_hero = null;


    // AI 체크 변수들

    // 공격 가능 범위 체크
    private float m_attackableTick = 0.0f;

    // Pattern A 조건 체크 Tick
    private float m_patternATick = 0.0f;

    // Pattern B 조건 체크 Tick
    private float m_patternBTick = 0.0f;
    
    // Pattern C 조건 체크 Tick
    private float m_patternCTick = 0.0f;


    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;



    // Animation 
    private const string ANI_ATTACK_A = "attack_A";
    private const string ANI_ATTACK_B = "attack_B";
    private const string ANI_ATTACK_C = "attack_C_fire";
    private const string ANI_ATTACK_C_READY = "attack_C_charge";
    private const string ANI_CHANGE_C = "transform";

    private const string ANI_D_SKILL_MOVE = "attack_D_in";

    private const string ANI_AB_MOVE = "move_fast_open";
    private const string ANI_CD_MOVE = "move_fast_close";

    public GameObject m_patternDRotate = null;
    public Animator m_laser = null;

    public Animator m_attackEffect = null;

    
    // --------------------------------------------------------------------------------------------//

    // AI 를 보고싶다



    bool NetworkObjectCheck()
    {
        if (!string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME))
        {
            m_name = NetworkOrderController.ORDER_NAME + "_boss_"+ this.GetHashCode();


            if (GameManager.Instance().CUR_PLACE == GameManager.ROBO_PLACE.BOSS_AREA)
            {
                if (!NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
                {
                    this.enabled = false;

                    this.gameObject.AddComponent<NetworkMoving>().NAME = m_name;
                    this.gameObject.AddComponent<NetworkStage1BOSS>().BOSS_NAME = m_name;
                }
                else
                    m_pattern = new PatternA(m_skeletonAnimation , ANI_AB_MOVE , ANI_ATTACK_A , m_name);
                return true;
            }
            
        }
        return false;
    }

    protected override void Move()
    {
        base.Move();
        MoveSend();
    }
    protected void MoveSend()
    {
        Vector3 pos = transform.position;
        //float distance = Vector3.Distance(m_prevPos , pos);
        m_prevPos = transform.position;

        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));
        //dirPos.Normalize();


        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_name ,
            pos.x , pos.y ,
            transform.eulerAngles.z,
            m_skeletonAnimation.skeleton.flipX ,
            sendPos));
        m_lastSendTime = Time.deltaTime;
    }

    public override void Damage(int damage)
    {
        base.Damage(damage);

        if(m_hp <=0)
        {
            //CameraManager.Instance().MoveCamera(null , 10.0f , CameraManager.CAMERA_PLACE.STAGE1);
            //GameObject obj = MapManager.Instance().AddObject(GamePath.EFFECT,tr);
            //obj.transform.position = transform.position;
            m_pattern = null;  
        }
        this.transform.GetChild(3).GetComponent<TextMesh>().text = "BOSS hp : " + m_hp + "/100";
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonHPUdate("boss1" , m_hp));
        
    }
    

 

    void Start()
    {
        // 처음 패턴은 A다.
        m_hp = 100;
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_pattern = new PatternA(m_skeletonAnimation , ANI_AB_MOVE , ANI_ATTACK_A , m_name);

        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        this.transform.GetChild(3).GetComponent<TextMesh>().text = "BOSS hp : " + m_hp + "/100";

    }
    
    void Update()
    {
        //if (!NetworkObjectCheck())
        //    return;

        if (m_hp <= 0)
        {
            MapManager.Instance().AddObject(GamePath.EFFECT , transform.position);
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));
            return;
        }
            

        
        // 방향설정
        Vector3 p = GameManager.Instance().ROBO.transform.position - transform.position;
        p.Normalize();
        m_angle = (Mathf.Atan2(p.x , p.y) * Mathf.Rad2Deg);
        m_angle = -m_angle - 85.0f;
        transform.eulerAngles = new Vector3(0 , 0 ,m_angle);

        Vector2 p2 = new Vector2(transform.position.x - 10.0f , transform.position.y);
        
        Debug.DrawLine(transform.position , p2 , Color.red);
            

        if (m_pattern != null)
            m_pattern.Update(gameObject);

        // 체력이 30% 이하로 떨어지면 
        if (m_hp <= m_fullHp * GameSetting.BOSS1_PATTERN_D_HP_CONDITION)
        {
            // 광폭화 모드가 아니라면 광폭화 모드다 !!!!
            if(!(m_pattern is PatternD))
            {
                m_pattern = new PatternD(m_skeletonAnimation,ANI_CD_MOVE,null, m_name);
            }
            
        }
        else
        {
            if(!AttackAbleCheck())
            {
                if(!PatternA_AbleCheck())
                {
                    if(!PatternB_AbleCheck())
                    {
                        if (!PatternC_AbleCheck())
                        {
                            m_attackableTick = 0.0f;
                            m_patternATick = 0.0f;
                            m_patternBTick = 0.0f;
                            m_patternCTick = 0.0f;
                            
                        }
                    }
                }
                
            }
        }
        // Move 
        Move();
        // 쿨타임 중이면 아~~무것도 안함
        if (CoolTime())
            return;

        SetCoolTime(Attack());
    }
    

    void SetCoolTime(float time)
    {
        if (time <= 0.0f)
            return;
        m_coolTime = time;
        m_coolTimeTick = 0.0f;
    }

    bool CoolTime()
    {
        
        if(m_coolTimeTick > m_coolTime)
        {
            // m_coolTime = 0.0f;
       //     m_coolTimeTick = m_coolTime;
            //  MDebug.Log("쿨타임 아님 "+m_coolTimeTick + " " + m_coolTime);
            return false;
        }
        else
        {   
          
            m_coolTimeTick += Time.deltaTime;
            return true;
        }
        
    }

    // 공격 가능 범위 체크용
    bool AttackAbleCheck()
    {
        if(m_hero == null)
            m_hero = GameManager.Instance().PLAYER.PLAYER_HERO.gameObject;
        m_attackableTick += Time.deltaTime;

        if(m_attackableTick >= GameSetting.BOSS1_ATTACK_ABLE_COOLTIME)
        {

            return false;
        }
        // 3 - 4초간 체크한다.
        Vector3 heroPos = m_hero.transform.position;
        Vector3 pos = transform.position;

        // 공격가능 범위인가?
        if (Vector3.Distance(heroPos, pos) <= GameSetting.BOSS1_ATTACK_ABLE_DISTANCE)
        {
            //기본 공격!!

            if (!(m_pattern is PatternNormal))
                m_pattern = new PatternNormal(m_skeletonAnimation,ANI_AB_MOVE,ANI_ATTACK_A, m_name);
        }

        return true;
        
    }

    // 패턴 A 체크용
    bool PatternA_AbleCheck()
    {
        m_patternATick += Time.deltaTime;

        if(m_patternATick >= GameSetting.BOSS1_PATTERN_A_ABLE_COOLTIME)
        {
            return false;
        }

        // 이 시간 동안에는 패턴 A로 공격한다
        if (!(m_pattern is PatternA))
            m_pattern = new PatternA(m_skeletonAnimation,ANI_AB_MOVE,ANI_ATTACK_A, m_name);

        return true;
    }

    // 패턴 B 체크용
    bool PatternB_AbleCheck()
    {
        m_patternBTick += Time.deltaTime;

        if (m_patternBTick >= GameSetting.BOSS1_PATTERN_B_ABLE_COOLTIME)
        {
            return false;
        }

        // 이 시간 동안에는 패턴 B로 공격한다
        if (!(m_pattern is PatternB))
            m_pattern = new PatternB(m_skeletonAnimation,ANI_AB_MOVE,ANI_ATTACK_B, m_name);

        return true;
    }

    // 패턴 C 체크용
    bool PatternC_AbleCheck()
    {
        m_patternCTick += Time.deltaTime;

        if (m_patternCTick >= GameSetting.BOSS1_PATTERN_C_ABLE_COOLTIME)
        {
            if (m_pattern != null)
                m_pattern.Exit();
            return false;
        }

        // 이 시간 동안에는 패턴 C로 공격한다
        if (!(m_pattern is PatternC))
            m_pattern = new PatternC(m_skeletonAnimation,ANI_CD_MOVE,ANI_ATTACK_C, m_name);

        return true;
    }

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if(e.msgType.Equals(NetworkManager.DAMAGE))
        {
            if(e.targetName.Equals(m_name))
            {
                Damage((int)e.msg.GetField(NetworkManager.DAMAGE).i);
            }
        }
    }


    // -- 전투 관련 -------------------------------------------------------------------   //


    // ----------------------------------------------------------------------------------//


}
