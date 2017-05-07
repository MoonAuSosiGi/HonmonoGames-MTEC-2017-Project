using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, NetworkManager.NetworkMoveEventListener
{

    // -- 기본 정보 -------------------------------------------------------------------------//
    private SpriteRenderer m_renderer = null;
    private float m_moveSpeed = 10.0f;
    private string m_bulletName = "";
    private bool m_isNetworkObject = false;

    Vector3 m_bulletDir = Vector3.left;

    public string BULLET_NAME { get { return m_bulletName; } }

    public float BULLET_SPEED { get { return m_moveSpeed; } set { m_moveSpeed = value; } }
    private bool m_filp = false;

    // -- 네트워크 ------------------------------------------------------------------------- //
    private Vector3 m_prevPos = Vector3.zero;
    private Vector3 m_targetPos = Vector3.zero;
    private float m_lastSendTime = 0.0f;

    // ------------------------------------------------------------------------------------- //
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
            transform.position = m_targetPos;
        }


    }

    void DeleteBullet()
    {
        CancelInvoke();

        string json = JSONMessageTool.ToJsonEnemyMove(
            m_bulletName , 
            transform.position.x , transform.position.y , transform.rotation.eulerAngles.y , false,Vector3.zero, "Delete");

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
        Vector3 drPos = Vector3.zero;
   //     MDebug.Log("bullet " +json);
        for (int i = 0; i < users.Count; i++)
        {
            if (m_bulletName.Equals(users[i].GetField("Name").str))
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

        if (!ck)
        {
            // 여기에 없다는 것은 삭제되어야 한다.
          //  NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_bulletName , "myTeam_bullet"));
            return;
        }

        
        if (z > transform.rotation.eulerAngles.z)
        {

            transform.rotation = Quaternion.Euler(0 , 0 , z);
        }



        m_targetPos = drPos; //* m_delay;
                              // transform.position = newPos;
    }

    void MoveSend()
    {
        Vector3 pos = transform.position;
        m_prevPos = transform.position;

        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));

        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_bulletName ,
            pos.x , pos.y , 
            transform.rotation.eulerAngles.z , 
            m_filp,
            sendPos));
        
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

    void OnTriggerEnter2D(Collider2D col)
    {
      //  MDebug.Log("이거다 " + col.transform.name);
    }
    void OnTriggerStay2D(Collider2D col)
    {
        if(m_isNetworkObject &&  (m_bulletName.IndexOf("boss") >= 0))
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
