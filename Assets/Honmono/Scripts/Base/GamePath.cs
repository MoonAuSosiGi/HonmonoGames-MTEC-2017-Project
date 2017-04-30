using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GamePath {

   
    public static string PREFABS = "Prefabs/";
    public static string WEAPON_BULLET_DEF = PREFABS + "Weapon/Bullet/Def_Bullet";
    public static string WEAPON_BULLET_BOSS = PREFABS + "Weapon/Bullet/Boss_Bullet";

    public static string POPUP = PREFABS + "Popup/";
    public static string ENEMY = PREFABS + "Enemy/";
    public static string BOSS = ENEMY + "BOSS/";

    // BOSS
    public static string BOSS1 = BOSS + "Stage1BOSS";
    public static string MONSTER1 = ENEMY + "Monster1";
    public static string MONSTER2 = ENEMY + "Monster2";

    public static string EFFECT = ENEMY + "EFFECT";

}
