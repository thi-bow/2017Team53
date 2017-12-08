﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_bullet : Shell {

    public Weapon.Attack_State m_type;

    protected override void OnTriggerEnter(Collider col)
    {
        base.OnTriggerEnter(col);

        if (col.gameObject.tag != "Player") return;

        var collide = col.gameObject.GetComponent<BoneCollide>();
        if (collide != null)
            collide.Damage(shellDamage, m_type);
    }

    protected override IEnumerator Start()
    {
        return base.Start();
    }
}