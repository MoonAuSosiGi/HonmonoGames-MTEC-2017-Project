using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoDoor : MonoBehaviour {

    GameObject m_player = null;
    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.transform.tag.Equals("Player"))
        {
            m_player = col.gameObject;
            iTween.MoveTo(gameObject , iTween.Hash("y" , -15.67f , "easeType" , "easeOutBack" , "oncompletetarget" , gameObject , "oncomplete" , "DoorEnd"));
        }
    }

    void DoorEnd()
    {
        CameraManager.Instance().MoveCameraAndObject(gameObject , 10 , CameraManager.CAMERA_PLACE.TUTORIAL_ROBO , m_player , gameObject , "EndTuto",false);
    }

    void EndTuto()
    {
        transform.parent.parent.gameObject.SetActive(false);
    }
}
