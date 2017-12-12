﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultScore : MonoBehaviour
{
    public static int KillCount = 0;
    public static int ShotKillCount = 0;
    public static int ApproachKillCount = 0;
    public static int DefaultKillCount = 0;
    public static int PargeKillCount = 0;

    public static void Reset()
    {
        KillCount = 0;
        ShotKillCount = 0;
        ApproachKillCount = 0;
        DefaultKillCount = 0;
        PargeKillCount = 0;
    }

    public static void AddKiilCount(Weapon.Attack_State state)
    {
        if(state == Weapon.Attack_State.approach)
        {
            ApproachKillCount++;
        }
        else if(state == Weapon.Attack_State.shooting)
        {
            ShotKillCount++;
        }
    }
}
