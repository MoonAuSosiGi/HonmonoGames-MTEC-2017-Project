using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public class Stage1Monster : Monster , NetworkManager.NetworkMessageEventListenrer
{

    // -- 기본 정보 --------------------------------------------------------//
    private HeroRobo m_robo = null;

    // 공격 가능 범위 체크용
    private float m_attackableTick = 0.0f;

    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;

    private const string ANI_ATTACK = "attack";
    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";

    private bool m_isNetworkObject = false;

    public bool NETWORKING {  set { m_isNetworkObject = value; } }

    private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_prevPos = Vector3.zero;

    float m_lastSendTime = 0.0f;

    bool m_networkObjectCheck = false;

    private string m_prevState = null;
    private string m_curState = null;

    private TextMesh m_text;

    public bool m_tutorial = false;

    public TutoRobo m_tutoRobo = null;
    // ---------------------------------------------------------------------//

    void Start()
    {
        m_text = this.transform.GetChild(0).GetComponent<TextMesh>();
        m_robo = GameManager.Instance().ROBO;
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_hp = 5;
        this.m_skeletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
        m_curState = "idle";
        m_text.text = "HP : " + m_hp;
    }

    bool NetworkObjectCheck()
    {
        if (m_tutorial)
        {

            if (m_pattern == null)
            {
                m_pattern = new MonsterPattern(m_skeletonAnimation , ANI_MOVE , ANI_ATTACK , null);
                m_skeletonAnimation.state.Complete += State_Complete;
            }
            return true;
        }
        if (m_isNetworkObject)
            return false;        

        if (m_networkObjectCheck)
            return true;
        if (GameManager.Instance().m_curSceneState.Equals("play") && 
            NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            Vector3 pos = transform.position;
            m_name = "monster_" + GameManager.Instance().PLAYER.USER_NAME + "_" + this.GetHashCode();
            // 미친 손을 봐야해
            if (m_skeletonAnimation.skeletonDataAsset.name.IndexOf("mon2") >= 0)
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(m_name , "monster1",pos.x,pos.y,-1.0f));
            else
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(m_name , "monster2" , pos.x , pos.y , -1.0f));
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderStateValueChange(m_name , m_hp));
            MoveSend();

            NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
            m_networkObjectCheck = true;
            m_pattern = new MonsterPattern(m_skeletonAnimation , ANI_MOVE , ANI_ATTACK , m_name);
            return true;
        }
        else if(!string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME) && ! string.IsNullOrEmpty(GameManager.Instance().PLAYER.USER_NAME) &&
            !NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            GameObject.Destroy(gameObject);
        }
        return false;
    }

    private void State_Complete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.animation.name.Equals(ANI_ATTACK))
        {
            m_skeletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);
            if (m_attackTarget != null)
            {
                if(!m_tutorial)
                {
                    HeroRobo robo = m_attackTarget.GetComponent<HeroRobo>();

                    if (robo != null)
                        robo.Damage(m_power);
                    else
                    {
                        Hero hero = m_attackTarget.GetComponent<Hero>();

                        if (hero != null)
                            hero.Damage(m_power);
                    }
                }
                else
                {
                    TutoRobo robo = m_attackTarget.GetComponent<TutoRobo>();

                    if (robo != null)
                        robo.Damage(m_power);

                }
               
            }
            
        }
    }

    void Update()
    {
        m_prevState = m_curState;
        if (!NetworkObjectCheck())
            return;
       
        if (FindMoveAbleCheck())
            Move();

        // 쿨타임 중이면 아~~무것도 안함
        if (CoolTime())
            return;

        if (!AttackAbleCheck())
            return;
        SetCoolTime(Attack());

    }



    protected override void Move()
    {


        if (m_pattern != null)
        {
            if(m_tutorial)
                m_pattern.Move(gameObject , m_tutoRobo.gameObject);
            else
                m_pattern.Move(gameObject , m_robo.gameObject);
        }
            
        

        if (m_robo.transform.position.x < transform.position.x)
            m_skeletonAnimation.skeleton.flipX = false;
        else
            m_skeletonAnimation.skeleton.flipX = true;


        if (m_tutorial)
            return;
        base.Move();


        if(!AttackAbleCheck())
            m_curState = "move";

        if (m_curState != m_prevState)
        {
            MDebug.Log("MOVE");
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonAIMessage(m_name , "" , ANI_MOVE , true));
        }
        MoveSend();
    }

    public override float Attack()
    {
        m_curState = "attack";
        if (m_tutorial)
        {
            float coolTime = 0.0f;
            if (m_pattern != null)
                coolTime = m_pattern.Attack(m_tutoRobo.gameObject , gameObject , m_index);
            return coolTime;
        }
        if (m_curState != m_prevState)
        {
            MDebug.Log("ATTACK");
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonAIMessage(m_name , "" , ANI_ATTACK , true));
        }
        return base.Attack();
    }

    protected void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos , pos);
        m_prevPos = transform.position;

        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));
        //dirPos.Normalize();


        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_name ,
            pos.x , pos.y ,0,
            m_skeletonAnimation.skeleton.flipX ,
            sendPos));
        m_lastSendTime = Time.deltaTime;
    }

    public override void Damage(int damage)
    {
        base.Damage(damage);
        m_text.text = "HP : " + m_hp;

        if(!m_tutorial)
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderStateValueChange(m_name , m_hp));

        if (m_hp <= 0)
        {
            MapManager.Instance().AddObject(GamePath.EFFECT,transform.position);

            if(!m_tutorial)
            {
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));
                NetworkManager.Instance().RemoveNetworkOrderMessageEventListener(this);
            }
            
            GameObject.Destroy(gameObject);
        }
    }

    // 공격 가능 범위 체크
    bool AttackAbleCheck()
    {
        Vector3 roboPos = (!m_tutorial) ? m_robo.transform.position : m_tutoRobo.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(roboPos , pos) <= GameSetting.MONSTER_ATTACK_DISTANCE)
        {
            return true;
        }

        return false;
    }

    bool FindMoveAbleCheck()
    {
        Vector3 roboPos = (!m_tutorial) ? m_robo.transform.position : m_tutoRobo.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(roboPos , pos) <= GameSetting.MONSTER_FIND_DISTANCE)
        {
            return true;
        }
        m_curState = "idle";

        if (m_tutorial)
            return false;
        if (m_curState != m_prevState)
        {
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonAIMessage(m_name , "" , ANI_IDLE , true));
        }
        return false;
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

        if (m_coolTimeTick > m_coolTime)
        {
            return false;
        }
        else
        {


            m_coolTimeTick += Time.deltaTime;
            return true;
        }

    }

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if(e.msgType.Equals(NetworkManager.HP_UPDATE))
        {
            // 데미지 입은것이 들어옴
            if(e.targetName.Equals(m_name))
            {
                Damage((int)e.msg.GetField(NetworkManager.HP_UPDATE).i);
            }
        }
    }
}
