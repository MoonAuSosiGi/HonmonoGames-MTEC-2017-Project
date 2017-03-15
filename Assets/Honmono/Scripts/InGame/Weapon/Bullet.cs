using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {


    private SpriteRenderer m_renderer = null;
    private float m_moveSpeed = 15.0f;

	// Use this for initialization
	void Start () {
        m_renderer =  this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {

        float movex = (m_renderer.flipX) ? m_moveSpeed * Time.deltaTime : -m_moveSpeed * Time.deltaTime;
        float posx = transform.position.x;


        // TODO 고쳐야 할 코드
        TargetMoveCamera camera = Camera.main.GetComponent<TargetMoveCamera>();
        Vector3 backPos = camera.BACKGROUND_POS;
        float leftCheck = backPos.x - camera.BACKGROUND_HALF_WIDTH;
        float rightCheck = backPos.x + camera.BACKGROUND_HALF_WIDTH;
        //float upCheck = backPos.y + camera.BACKGROUND_HALF_HEIGHT;
        //float downCheck = backPos.x - camera.BACKGROUND_HALF_HEIGHT;

        if (posx + movex + m_renderer.bounds.size.x / 2.0f <= leftCheck) DeleteBullet();
        if (posx + movex - m_renderer.bounds.size.x / 2.0f >= rightCheck) DeleteBullet();

        transform.Translate(movex, 0, 0);
	}

    void DeleteBullet()
    {
        BulletManager.Instance().RemoveBullet(this);
    }
}
