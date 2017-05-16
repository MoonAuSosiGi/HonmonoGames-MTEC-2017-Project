using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class NetworkStage1Monster : MonoBehaviour {

    // -- 기본정보 ---------------------------------------------------------------------------------------------------------------------//
    private string m_name = null;
    private SkeletonAnimation m_skeletonAnimation = null;

    // ---------------------------------------------------------------------------------------------------------------------------------//
	// Use this for initialization
	void Start () {
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
}
