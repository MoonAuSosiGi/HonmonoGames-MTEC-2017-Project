using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSetting  {

    // -- 스탯 ------------------------------------------------------------------------//
    public static class STAT
    {
        public const int MAX_SPEED = 5;
        public const int MAX_POWER = 5;
        public const int MAX_REPAIR = 5;
    }

    // 카메라값
    public const float CAMERA_SPACE = 10.0f;
    public const float CAMERA_ROBO = 8.0f;

    
    // -- 주인공 관련 값 ( 휴먼 ) -----------------------------------------------------//
    public const float HERO_MOVE_SPEED = 6.0f;
    public const float HERO_MAX_HP = 100;

    // -- 주인공 관련 값 ( 로봇 ) -----------------------------------------------------//
    public const float HERO_ROBO_SPEED = 6.0f;
    public const float HERO_ROBO_BULLET_SPEED = 10.0f;
    public const int HERO_ROBO_MAX_HP = 100;

    // -- 보스 패턴에 관련된 세팅값 ---------------------------------------------------//

    public const float BOSS1_SPEED = 8.0f;

    public const float BOSS1_ATTACK_ABLE_COOLTIME = 4.0f;
    public const float BOSS1_PATTERN_A_ABLE_COOLTIME = 4.0f;
    public const float BOSS1_PATTERN_B_ABLE_COOLTIME = 15.0f;
    public const float BOSS1_PATTERN_C_ABLE_COOLTIME = 6.0f;

    public const float BOSS1_ATTACK_ABLE_DISTANCE = 10.0f;
    public const float BOSS1_DEF_ATTACK_COOLTIME = 1.0f;



    public const float BOSS1_PATTERN_A_ATTACK_COOLTIME =  2.0f;
    public const float BOSS1_PATTERN_B_ATTACK_COOLTIME = 1.0f;
    public const float BOSS1_PATTERN_C_ATTACK_COOLTIME = 2.0f;

    public const float BOSS1_PATTERN_A_BULLET_REACH_TIME = 2.0f;
    public const float BOSS1_PATTERN_B_BULLET_REACH_TIME = 3.0f;
    public const float BOSS1_PATTERN_C_BULLET_REACH_TIME = 2.0f;

    public const float BOSS1_PATTERN_D_HP_CONDITION = 0.3f;
    public const float BOSS1_PATTERN_D_SPECIAL = 2.5f;
    public const float BOSS1_PATTERN_D_DEF = 0.5f;

    // 실제 어택 수행후 쿨타임
    public const float BOSS1_PATTERN_A_ATTACK_COOL = 2.0f;
    public const float BOSS1_PATTERN_B_ATTACK_COOL = 1.0f;
    public const float BOSS1_PATTERN_C_ATTACK_COOL = 6.0f;
    // -- 몬스터에 관련된 세팅값 -----------------------------------------------------//
    public const float MONSTER_HP = 100.0f;
    public const float MONSTER_ATTACK_DISTANCE = 6.0f;
    public const float MONSTER_FIND_DISTANCE = 15.0f;
}
