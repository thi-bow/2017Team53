﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HogeRayShot : MonoBehaviour {

    public Camera m_camera;
    public LayerMask m_layerMask;

	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if(Physics.Raycast(ray, out hit, m_layerMask))
            {
                if (hit.collider.gameObject.GetComponent<BoneCollide>() != null)
                {
                    hit.collider.gameObject.GetComponent<BoneCollide>().Damage(10, Weapon.Attack_State.shooting);
                }
            }
        }
	}
}
