using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {

    // -- 플레이어 정보 -----------------------------------------------------------------------------------------//
    // 현재 플레이어의 모든 정보가 집약되어 있다.
    private Hero m_hero = null;
    private int m_NetworkIndex = 0;
    private string m_UserName = "";
    private int m_hp = 0;
    private PLAYER_STATUS m_status = new PLAYER_STATUS();
    private string m_skeletonDataAssetName = "";

    public string SKELETON_DATA_ASSET
    {
        get { return m_skeletonDataAssetName; }
        set { m_skeletonDataAssetName = value; }
    }


    public enum PlayerJob
    {
        DRIVER = 100,
        ARMY,
        ENGINEER,
        
    }

    // -- 프로퍼티 ---------------------------------------------------------------------------------------------//
    public Hero PLAYER_HERO { set { m_hero = value; } get{ return m_hero;}}
    public PLAYER_STATUS STATUS { get { return m_status; } }
    public string USER_NAME {
        set {
            m_UserName = value;  } get { return m_UserName; } }
    public int NETWORK_INDEX { set { m_NetworkIndex = value; } get { return m_NetworkIndex; } }

    // -- 정보 구조체 ------------------------------------------------------------------------------------------//

    // Status (스탯 정보)        
    public class PLAYER_STATUS
    {
        private int m_stat_speed = 0;
        private int m_stat_power = 0;
        private int m_stat_repair = 0;

        public int STAT_SPEED { get { return m_stat_speed; } set { m_stat_speed = value; } }
        public int STAT_POWER { get { return m_stat_power; } set { m_stat_power = value; } }
        public int STAT_REPAIR { get { return m_stat_repair; } set { m_stat_repair = value; } }

        public JSONObject ToJson()
        {
            return new JSONObject("");
        }
    }

    // -- 기본 메서드 -----------------------------------------------------------------------------------------//

    // 모든 데이터 JSON
    public JSONObject ToJsonAllInfo()
    {
        return new JSONObject();
    }

    // 이동 JSON
    public JSONObject ToJsonPositionInfo()
    {
        Vector3 pos = m_hero.transform.position;
        return new JSONObject("{\"UserName\":" + m_UserName + ",\"x\":" + pos.x + ",\"y\":" + pos.y + "}");
    }
    
    //---------------------------------------------------------------------------------------------------------//
    
}
