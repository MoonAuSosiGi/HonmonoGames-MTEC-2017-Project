using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpacePentrationMonster : Monster , NetworkManager.NetworkMessageEventListenrer {

    // -- 기본 정보 ---------------------------------------------------//
    private const string ANI_MOVE = "move";

    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;

    // Networking
    Vector3 m_prevPos = Vector3.zero;
    float m_lastSendTime = 0.0f;

    private bool m_isNetworkObject = false;
    bool m_networkObjectCheck = false;
    public bool NETWORKING { set { m_isNetworkObject = value; } }

    private GameObject m_target = null;
    // --------------------------------------------------------------//

    void Start()
    {
        m_skeletonAnimation = GetComponent<SkeletonAnimation>();

        m_skeletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);
        m_hp = 10;
        m_fullHp = 10;
    }

    void Update()
    {

        if (!NetworkObjectCheck())
            return;

        if (m_target == null)
            m_target = GameManager.Instance().ROBO.gameObject;

        if (FindMoveAbleCheck())
            Move();
        
        {
            if (CoolTime())
                return;

            SetCoolTime(Attack());
        }
    }

    bool NetworkObjectCheck()
    {
        if (m_isNetworkObject)
            return false;

        if (m_networkObjectCheck)
            return true;
        if (GameManager.Instance().m_curSceneState.Equals("play") &&
            NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            Vector3 pos = transform.position;
            m_name = "monster_" + GameManager.Instance().PLAYER.USER_NAME + "_" + this.GetHashCode();
            string dataName = m_skeletonAnimation.skeletonDataAsset.name;

            NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(m_name , "PentrationMonsterSpace" , pos.x , pos.y , -1.0f));


            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderStateValueChange(m_name , m_hp));

           
            MoveSend();
            m_networkObjectCheck = true;
            return true;
        }
        else if (!string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME) && !string.IsNullOrEmpty(GameManager.Instance().PLAYER.USER_NAME) &&
            !NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            GameObject.Destroy(gameObject);
        }
        return false;
    }

    bool FindMoveAbleCheck()
    {
        if (m_target == null)
            return false;
        Vector3 targetPos = m_target.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(targetPos , pos) <= GameSetting.MONSTER_FIND_DISTANCE)
        {
            return true;
        }
        return false;
    }

    protected override void Move()
    {
        if (m_target.transform.position.x + 2.0f < transform.position.x)
            m_skeletonAnimation.skeleton.flipX = false;
        else
            m_skeletonAnimation.skeleton.flipX = true;

        Vector3 dir = m_target.transform.position - transform.position;
        dir.Normalize();

        transform.position += dir * 10.0f * Time.deltaTime;
        MoveSend();
    }

    protected void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos , pos);
        m_prevPos = transform.position;

        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));

        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_name ,
            pos.x , pos.y , 0 ,
            m_skeletonAnimation.skeleton.flipX ,
            sendPos));
        m_lastSendTime = Time.deltaTime;
    }
    public override void Damage(int damage)
    {
        base.Damage(damage);
        if (m_hp <= 0)
        {
            MapManager.Instance().AddObject(GamePath.EFFECT , transform.position);
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));            
            GameObject.Destroy(gameObject);
            return;
        }
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonHPUdate(m_name , m_hp));
    }


    bool AttackAbleCheck()
    {
        if (m_target == null)
            return false;
        Vector3 targetPos = m_target.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(targetPos , pos) <= 10.0f)
        {
            return true;
        }
        return false;
    }

    public override float Attack()
    {
        //여기서 생성명령 
        if (AttackAbleCheck())
        {
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));
            GameObject.Destroy(gameObject);
            MapManager.Instance().PentrationMonsterCreate();
        }
        else
            return 10.0f;
        return 10.0f;
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
        if (e.msgType.Equals(NetworkManager.HP_UPDATE) && 
            !GameManager.Instance().PLAYER.USER_NAME.Equals(e.user))
        {
            // 데미지 입은것이 들어옴
            if (e.targetName.Equals(m_name))
            {
                GameManager.Instance().SetCurrentEnemy(this);
                base.Damage((int)e.msg.GetField(NetworkManager.HP_UPDATE).i);
                
                if (m_hp <= 0)
                {
                    MapManager.Instance().AddObject(GamePath.EFFECT , transform.position);
                    NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));
                    GameObject.Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
