using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using Spine.Unity;

public class Stage2Boss : Monster,NetworkManager.NetworkMessageEventListenrer {
    // -- Network ------------------------------------------------------------------//
    private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_prevPos = Vector3.zero;
    float m_lastSendTime = 0.0f;
    public GameObject m_tail = null;
    public GameObject m_target = null;
    private float m_angle = 0.0f;
    // ----------------------------------------------------------------------------//
    private Vector3 m_dir = Vector3.zero;
    // 총 발사하기 위한 본 리스트
    public List<GameObject> m_boneList = new List<GameObject>();
    // 박살나기 유무를 위한 본 리스트
    public List<Stage2BOSSBone> m_destroyBoneList = new List<Stage2BOSSBone>();

 
    public class Stage2BOSSBone
    {
        public int m_hp;
        public SpriteRenderer m_renderer;
    }


    // Move 용
    private float m_moveTick = 0.0f;
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
    private const string ANI_MOVE = "move";

    // --------------------------------------------------------------------------------------------//

    void Start()
    {
        m_hp = 100;
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_pattern = new Boss2PatternA(m_skeletonAnimation , ANI_MOVE , "" , m_name);
        m_skeletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);
        Monster.m_index = 0;
        DestroyListSetup();
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        if (m_target == null)
            m_target = GameManager.Instance().ROBO.gameObject;
        m_dir = m_target.transform.position - transform.position;
        m_dir.Normalize();
        Rotate();
    }
    
    void DestroyListSetup()
    {
        Transform t = transform;
        while (m_destroyBoneList.Count < 10)
        {
             t = t.GetChild(0);

            if (t.childCount >= 2)
            {
                Stage2BOSSBone b = new Stage2BOSSBone();
                b.m_hp = 2;
                b.m_renderer = t.GetChild(1).GetComponent<SpriteRenderer>();
                m_destroyBoneList.Add(b);
            }
        }
    }

    void Update()
    {
        if(m_hp <= 0)
        {
            MapManager.Instance().AddObject(GamePath.EFFECT , transform.position);
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));
            GameObject.Destroy(gameObject);
            return;
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            GameManager.Instance().SetCurrentEnemy(this);

            Damage(20);
        }
        if (m_pattern != null)
            m_pattern.Update(gameObject);

        // 체력이 30% 이하로 떨어지면 
        if (m_hp <= m_fullHp * GameSetting.BOSS1_PATTERN_D_HP_CONDITION)
        {
            
            // 광폭화 모드가 아니라면 광폭화 모드다 !!!!
            if (!(m_pattern is Boss2PatternD))
            {
                m_tail.gameObject.SetActive(false);
                m_pattern = new Boss2PatternD(m_skeletonAnimation , ANI_MOVE , null , m_name);
            }

        }
        else
        {
            
            if (!PatternA_AbleCheck())
            {
                if (!PatternB_AbleCheck())
                {
                    m_attackableTick = 0.0f;
                    m_patternATick = 0.0f;
                    m_patternBTick = 0.0f;
                    m_patternCTick = 0.0f;
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

    public void Damage(int damage,string boneName)
    {
        base.Damage(damage);
        SpriteRenderer target = null;

        if(boneName.Equals("head"))
        {

        }
        else if(boneName.Equals("Tail"))
        {

        }
        else
        {
            string num = Regex.Replace(boneName , @"\D" , "");
            MDebug.Log(boneName);
            int index = int.Parse(num);
            target = m_destroyBoneList[index-1].m_renderer;

            m_destroyBoneList[index - 1].m_hp -= 1;

            if(m_destroyBoneList[index - 1].m_hp <= 0)
            {
                m_destroyBoneList[index - 1].m_renderer.enabled = true;
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonPartDestroy(index-1));
            }
                
        }
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonHPUdate(m_name , m_hp));
    }

    public void ShootBullet(int[] index,bool randDir = false)
    {
        for(int i = 0; i < index.Length; i++)
        {
            Bullet b = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_BOSS1_P1);
            GameObject bone = m_boneList[index[i]];
            
            Vector3 dir = m_target.transform.position - bone.transform.position;
            dir.Normalize();

            if(randDir)
            {
                float t = (UnityEngine.Random.Range(0 , 2) == 1) ? 1.0f : -1.0f;
                dir *= t;

                dir = new Vector3(dir.x + UnityEngine.Random.Range(-0.3f , 0.3f) , dir.y + UnityEngine.Random.Range(-0.3f , 0.3f));
            }

            string name = GameManager.Instance().PLAYER.USER_NAME + "_boss2_" + Monster.m_index++;
            b.SetupBullet(name , false , dir);
            b.BULLET_SPEED = 20.0f;
            Vector3 pos = bone.transform.position;
            b.transform.position = pos;

            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonCreateOrder(
                    name ,
                    "boss1_bullet" ,
                    pos.x , pos.y ,
                    b.transform.rotation.eulerAngles.y ,
                    false));
        }
    }
    public void ShootEgg(int[] index , bool randDir = false)
    {
        for (int i = 0; i < index.Length; i++)
        {
            Bullet b = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.EGG_BULLET);
            GameObject bone = m_boneList[index[i]];

            Vector3 dir = m_target.transform.position - bone.transform.position;
            dir.Normalize();

            if (randDir)
            {
                float t = (UnityEngine.Random.Range(0 , 2) == 1) ? 1.0f : -1.0f;
                dir *= t;

                dir = new Vector3(dir.x + UnityEngine.Random.Range(-0.3f , 0.3f) , dir.y + UnityEngine.Random.Range(-0.3f , 0.3f));
            }

            string name = GameManager.Instance().PLAYER.USER_NAME + "_boss2_" + Monster.m_index++;
            b.SetupBullet(name , false , dir);
            b.BULLET_SPEED = 20.0f + UnityEngine.Random.Range(-10.0f , 10.0f);
            Vector3 pos = bone.transform.position;
            b.transform.position = pos;

            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonCreateOrder(
                    name ,
                    "boss1_bullet" ,
                    pos.x , pos.y ,
                    b.transform.rotation.eulerAngles.y ,
                    false));
        }
    }
    private void Rotate()
    {
        // 방향설정
        Vector3 p = m_target.transform.position - transform.position;
        p.Normalize();
        m_angle = (Mathf.Atan2(p.x , p.y) * Mathf.Rad2Deg);
        m_angle = -m_angle - 85.0f;
        transform.eulerAngles = new Vector3(0 , 0 , m_angle);  // 방향설정
    }
    

    protected override void Move()
    {
        m_moveTick += Time.deltaTime;

        if(m_moveTick >= 5.0f)
        {
            Rotate();

            m_dir = m_target.transform.position - transform.position;
            m_dir.Normalize();
           
            m_moveTick = 0.0f;
        }
        transform.position += m_dir * Time.deltaTime * 15.0f;

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
            transform.eulerAngles.z ,
            m_skeletonAnimation.skeleton.flipX ,
            sendPos));
        m_lastSendTime = Time.deltaTime;
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
    

    // 패턴 A 체크용
    bool PatternA_AbleCheck()
    {
        m_patternATick += Time.deltaTime;

        if (m_patternATick >= GameSetting.BOSS1_PATTERN_A_ABLE_COOLTIME)
        {
            return false;
        }

        // 이 시간 동안에는 패턴 A로 공격한다
        if (!(m_pattern is Boss2PatternA))
            m_pattern = new Boss2PatternA(m_skeletonAnimation , ANI_MOVE , "" , m_name);

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
        if (!(m_pattern is Boss2PatternB))
            m_pattern = new Boss2PatternB(m_skeletonAnimation , ANI_MOVE , "" , m_name);

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
        if (!(m_pattern is Boss2PatternC))
            m_pattern = new Boss2PatternC(m_skeletonAnimation , ANI_MOVE ,"" , m_name);

        return true;
    }

    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if (e.msgType.Equals(NetworkManager.DAMAGE))
        {
            if (e.targetName.Equals(m_name))
            {
                GameManager.Instance().SetCurrentEnemy(this);
                Damage((int)e.msg.GetField(NetworkManager.DAMAGE).i);
            }
        }
    }
}
