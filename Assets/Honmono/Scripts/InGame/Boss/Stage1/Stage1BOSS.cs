using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class Stage1BOSS : Monster , NetworkManager.NetworkMoveEventListener, NetworkManager.NetworkMessageEventListenrer
{
    // -- Network ------------------------------------------------------------------//
    private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_prevPos = Vector3.zero;
    private bool m_isNetworkObject = false;
    float m_syncTime = 0.0f;
    float m_delay = 0.0f;
    float m_lastSyncTime = 0.0f;
    public string m_BOSS_NAME = null;


    public bool m_networkObject = false;
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
    void OnGUI()
    {
        //if (m_pattern == null)
        //    return;
        //string txt = "";
        //float tick = 0.0f;
        //float coolTime = m_coolTimeTick;

        //txt = m_pattern.GetType().ToString();
        //if (m_pattern is PatternA)
        //    tick = m_patternATick;
        //else if (m_pattern is PatternB)
        //    tick = m_patternBTick;
        //else if (m_pattern is PatternC)
        //    tick = m_patternCTick;
        //else if (m_pattern is PatternNormal)
        //    tick = m_attackableTick;


        //GUI.TextArea(new Rect(500 , 150 , 100 , 100) , txt + " tick : " + tick + " coolTime ? : " + coolTime);
    }

    protected override void Move()
    {
        // Move 
        string moveAni = null;
        if (m_pattern is PatternNormal ||
            m_pattern is PatternA ||
            m_pattern is PatternB)
            moveAni = ANI_AB_MOVE;

        if (!string.IsNullOrEmpty(moveAni))
        {
            CheckAndSetAnimation(0 , moveAni , true);
            NetworkManager.Instance().SendOrderMessage(
                  JSONMessageTool.ToJsonAIMessageAnimation("C" , moveAni , 0 , true));
        }

        if (m_pattern is PatternC || m_pattern is PatternD)
            moveAni = ANI_CD_MOVE;

        //
        if (!string.IsNullOrEmpty(moveAni) && m_pattern is PatternC)
        {
            CheckAndSetAnimation(1 , moveAni , true);
            NetworkManager.Instance().SendOrderMessage(
                  JSONMessageTool.ToJsonAIMessageAnimation("C" , moveAni , 1 , true));
        }


        if (m_pattern != null)
        {
            float distance = Vector3.Distance(transform.parent.position , GameManager.Instance().ROBO.transform.position);

            if(distance >= 15.0f)
                m_pattern.Move(transform.parent.gameObject ,
                    GameManager.Instance().ROBO.gameObject);
        }
        MoveSend();
    }

    public override void Damage(float damage)
    {
        base.Damage(damage);

        if(m_hp <=0.0f)
        {
            //CameraManager.Instance().MoveCamera(null , 10.0f , CameraManager.CAMERA_PLACE.STAGE1);
            GameObject obj = MapManager.Instance().AddObject(GamePath.EFFECT);
            obj.transform.position = transform.position;
            GameObject.Destroy(gameObject);
        }
    }

    public override float Attack()
    {
        if (m_pattern is PatternNormal)
        {
            CheckAndSetAnimation(1,ANI_ATTACK_A , true);
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessageAnimation("normal" , ANI_ATTACK_A , 1 , true));

        }
        else if(m_pattern is PatternA)
        {
            CheckAndSetAnimation(1 , ANI_ATTACK_A , true);
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessageAnimation("A" , ANI_ATTACK_A , 1 , true));
        }
        else if (m_pattern is PatternB)
        {
            CheckAndSetAnimation(1 , ANI_ATTACK_B , true);
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessageAnimation("B" , ANI_ATTACK_B , 1 , true));
        }
        else if (m_pattern is PatternC)
        {

        }
        return base.Attack();
    }

 

    void Start()
    {
        // 처음 패턴은 A다.
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        
        m_pattern = new PatternA(m_skeletonAnimation);

        CheckAndSetAnimation(0,ANI_AB_MOVE , true); 
      
   //     m_skeletonAnimation.state.Complete += CompleteEvent;
        

    }

    public void NetworkSetup()
    {
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        NetworkManager.Instance().AddNetworkEnemyMoveEventListener(this);
    }
    
    void Update()
    {


        if (NetworkOrderController.ORDER_NAME != GameManager.Instance().PLAYER.USER_NAME
            && NetworkOrderController.ORDER_SPACE == 0)
            return;
        
        // 방향설정
        Vector3 p = GameManager.Instance().ROBO.transform.position - transform.parent.position;
        p.Normalize();
        float angle = (Mathf.Atan2(p.x , p.y) * Mathf.Rad2Deg);
        transform.parent.rotation = Quaternion.Euler(0 , 0 , -angle);


        if (m_networkObject)
        {
            m_syncTime += Time.deltaTime;

            //네트워크 보간(테스트 완료 - 로컬 )
            if (m_delay > 0)
                transform.position = Vector3.Lerp(transform.position , m_targetPos , m_syncTime / m_delay);
            return;
        }
            

        if (m_pattern != null)
            m_pattern.Update(gameObject);

        // 체력이 30% 이하로 떨어지면 
        if (m_hp <= m_fullHp * GameSetting.BOSS1_PATTERN_D_HP_CONDITION)
        {
            // 광폭화 모드가 아니라면 광폭화 모드다 !!!!
            if(!(m_pattern is PatternD))
            {
                m_pattern = new PatternD(m_skeletonAnimation);
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
                m_pattern = new PatternNormal(m_skeletonAnimation);
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
            m_pattern = new PatternA(m_skeletonAnimation);

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
            m_pattern = new PatternB(m_skeletonAnimation);

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
            m_pattern = new PatternC(m_skeletonAnimation);

        return true;
    }


    // -- 전투 관련 -------------------------------------------------------------------   //
    void OnCollisionEnter2D(Collision2D col)
    {
        //로직 TODO
        // 닿은 대상의 이름으로 판별
        // 로봇의 총알
        //   - 총알 삭제 명령
        //   - 몬스터 데미지 명령
        //   - 몬스터 데미지 애니메이션 명령
     
    }

    void OnCollisionStay2D(Collision2D col)
    {
        //    MDebug.Log("Collision Stay");
    }

    void OnCollisionExit2D(Collision2D col)
    {
        //  MDebug.Log("Collision Exit");
    }

    // ----------------------------------------------------------------------------------//
    void MoveSend()
    {
        Vector3 pos = transform.parent.position;
        float distance = Vector3.Distance(m_prevPos , pos);
        m_prevPos = transform.parent.position;
        //    MDebug.Log(t);
        if (distance <= 0)
            return;
        
        NetworkManager.Instance().SendEnemyMoveMessage(JSONMessageTool.ToJsonEnemyMove(m_BOSS_NAME ,
            pos.x , pos.y , transform.rotation.eulerAngles.z , false,Vector3.zero));
    }

    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        switch(e.msgType)
        {
            case NetworkManager.AI:
                CheckAndSetAnimation((int)e.msg.GetField(NetworkManager.AI_ANI_INDEX).i ,
                    e.msg.GetField(NetworkManager.AI_ANI_NAME).str , e.msg.GetField(NetworkManager.AI_ANI_LOOP));
                break;
            case NetworkManager.AI_C:
                if (IsCurrentAnimation(2,"transform"))
                    return;
                m_skeletonAnimation.state.SetAnimation(2 , "transform" , false);
                m_skeletonAnimation.state.AddAnimation(2 , "attack_C_charge" , false , 0.0f);
                m_skeletonAnimation.state.AddAnimation(2 , "attack_C_fire" , false , 0.0f);
                break;
            case NetworkManager.AI_C_LASER:
                MDebug.Log("LASER ");
                if(e.msg.GetField(NetworkManager.AI_C_LASER).b)
                {
                    m_laser.gameObject.SetActive(true);
                    m_laser.Play("boss_laser");
                    m_laser.GetComponent<BoxCollider2D>().enabled = true;
                }
                else
                {                    
                    m_laser.SetInteger("laser" , 1);
                    m_laser.Play("Wait");
                    m_laser.gameObject.SetActive(false);
                    m_laser.GetComponent<BoxCollider2D>().enabled = false;
                    m_skeletonAnimation.state.ClearTrack(2);
                }
                break;

            case NetworkManager.AI_D_ROTATE:
                Vector3 pos = m_patternDRotate.transform.position;
                m_patternDRotate.gameObject.SetActive(true);
                m_patternDRotate.transform.position = new Vector3(e.msg.GetField("X").f , e.msg.GetField("Y").f , pos.z);
                transform.parent.position = m_patternDRotate.transform.position;
                m_patternDRotate.transform.Rotate(0.0f , 0.0f , e.msg.GetField(NetworkManager.AI_D_ROTATE).f);
                break;
            case NetworkManager.AI_D_END:
                {
                    iTween.ScaleTo(m_patternDRotate , iTween.Hash("x" , 0.0f , "y" , 0.0f));
                    m_skeletonAnimation.state.SetAnimation(2 , "move_fast_close" , true);
                }
                break;
                

        }

    }

    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        JSONObject obj = json;
        JSONObject users = obj.GetField("Enemies");

        //{"Enemies":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}

        float x = 0.0f, y = 0.0f, z = 0.0f;
        bool flip = false;
        bool ck = false;
        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].GetField("Name").str == m_BOSS_NAME)
            {
                x = users[i].GetField("X").f;
                y = users[i].GetField("Y").f;
                z = users[i].GetField("Z").f;
                flip = users[i].GetField(NetworkManager.DIR).b;
                ck = true;
                break;
            }
        }

        Vector3 newPos = new Vector3(x , y , -1.0f);

        float distance = Vector3.Distance(transform.parent.position , newPos);
        //this.m_renderer.flipX = flip;

        if (!ck)
            return;

        if (z > transform.rotation.eulerAngles.z)
        {

            transform.rotation = Quaternion.Euler(0 , 0 , z);
        }
        if (distance <= 0)
        {
            ////    this.m_animator.SetBool("Move", false);
            //        return;
        }


        m_syncTime = 0.0f;
        m_delay = Time.time - m_lastSyncTime;
        m_lastSyncTime = Time.time;
        m_targetPos = newPos; //* m_delay;
                              // transform.position = newPos;

    }

}
