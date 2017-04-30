using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, NetworkManager.NetworkMoveEventListener
{


    private SpriteRenderer m_renderer = null;
    private float m_moveSpeed = 10.0f;
    private string m_bulletName = "";
    private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_prevPos = Vector3.zero;
    private bool m_isNetworkObject = false;
    float m_syncTime = 0.0f;
    float m_delay = 0.0f;
    float m_lastSyncTime = 0.0f;
    Vector3 m_bulletDir = Vector3.left;
    public string BULLET_NAME { get { return m_bulletName; } }

    public float BULLET_SPEED { get { return m_moveSpeed; } set { m_moveSpeed = value; } }
    private bool m_filp = false;

    // Use this for initialization
    void Start()
    {
        m_renderer = this.GetComponent<SpriteRenderer>();
        m_prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (!m_isNetworkObject)
        {

            float movex = (m_filp) ? m_moveSpeed * Time.deltaTime : -m_moveSpeed * Time.deltaTime;
            float posx = transform.position.x;

            if (m_bulletName.IndexOf("boss") >= 0)
            {
                transform.position += m_bulletDir * m_moveSpeed * Time.deltaTime;
                MoveSend();
                return;
            }

            transform.Translate(0 , -movex , 0);
            MoveSend();
        }
        else
        {
            m_syncTime += Time.deltaTime;

            //네트워크 보간(테스트 완료 - 로컬 )
            if (m_delay > 0)
                transform.position = Vector3.Lerp(transform.position , m_targetPos , m_syncTime / m_delay);
        }


    }

    void DeleteBullet()
    {
        CancelInvoke();

        string json = JSONMessageTool.ToJsoinEnemyMove(m_bulletName , transform.position.x , transform.position.y , transform.rotation.eulerAngles.y , false , "Delete");

        MDebug.Log("동네 사람들 이생키 죽었어요 " + json);

        NetworkManager.Instance().SendEnemyMoveMessage(json);
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_bulletName , "myTeam_bullet"));
        BulletManager.Instance().RemoveBullet(this);
    }

    public void SetupBullet(string name , bool networkObject , Vector3 dir , float speed = 0.0f , bool flip = false)
    {
        m_bulletName = name;

        m_isNetworkObject = networkObject;
        m_filp = flip;


        if(m_bulletName.IndexOf("boss")>=0)
        {
            m_bulletDir = dir;
            float angle = (Mathf.Atan2(m_bulletDir.x , m_bulletDir.y) * Mathf.Rad2Deg);// + 45.0f;
            transform.rotation = Quaternion.Euler(0.0f , 0.0f , angle);
        }
        

        if (m_isNetworkObject)
        {
            this.GetComponent<Rigidbody2D>().simulated = false;
            NetworkManager.Instance().AddNetworkEnemyMoveEventListener(this);
            //   InvokeRepeating("MoveSend", 0.0f, 1.0f / 60.0f);
            

            // ANGLE 스프라이트 방향 때문에 조절해야함 -값 붙음
            

        }
    }

    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        if (!m_isNetworkObject)
            return;
        JSONObject obj = json;
        JSONObject users = obj.GetField("Enemies");

        //{"Enemies":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}

        float x = 0.0f, y = 0.0f, z = 0.0f;
        bool flip = false;
        bool ck = false;
        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].GetField("Name").str == m_bulletName)
            {
                x = users[i].GetField("X").f;
                y = users[i].GetField("Y").f;
                z = users[i].GetField("Z").f;
                flip = users[i].GetField(NetworkManager.DIR).b;
                ck = true;
                break;
            }
        }

        if (!ck)
        {
            // 여기에 없다는 것은 삭제되어야 한다.
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_bulletName , "myTeam_bullet"));
            return;
        }

        Vector3 newPos = new Vector3(x , y , -1.0f);

        float distance = Vector3.Distance(transform.position , newPos);
        //this.m_renderer.flipX = flip;

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

    void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos , pos);
        m_prevPos = transform.position;
        //    MDebug.Log(t);
        if (distance <= 0)
            return;

        NetworkManager.Instance().SendEnemyMoveMessage(JSONMessageTool.ToJsoinEnemyMove(m_bulletName ,
            pos.x , pos.y , transform.rotation.eulerAngles.z , m_filp));
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (m_isNetworkObject)
            return;
        MDebug.Log("얘 삭제되었음");

        if (col.transform.name == "ROBO")
        {

        }

        //일단 최종 좌표를 던져본다.



        // DeleteBullet();
    }

    void OnTriggerEnter2D(Collision2D col)
    {
      //  MDebug.Log("이거다 " + col.transform.name);
    }
    void OnTriggerStay2D(Collider2D col)
    {
        if(m_isNetworkObject ||  (m_bulletName.IndexOf("boss") >= 0))
        {
            // 에너미
            if(col.name == "ROBO")
            {
                GameObject obj = MapManager.Instance().AddObject(GamePath.EFFECT);
                obj.transform.position = col.transform.position;
                GameObject.Destroy(gameObject);
            }
        }
        else
        {
            if (col.tag == "ENEMY")
            {
                if (!ck)
                {
                    col.GetComponent<Monster>().Damage(10.0f);
                    ck = true;
                }
                
                GameObject obj = MapManager.Instance().AddObject(GamePath.EFFECT);
                obj.transform.position = col.transform.position;
                GameObject.Destroy(gameObject);
            }
        }
        
    }

    bool ck = false;
}
