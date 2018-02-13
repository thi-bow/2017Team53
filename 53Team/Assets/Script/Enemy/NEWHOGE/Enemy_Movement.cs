﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Animator))]
public class Enemy_Movement : MonoBehaviour {

    public float m_moveSpeed = 3f;
    public float m_turnSpeed = 1f;

    private Animator m_animator;
    private bool m_isDead = false;

    private readonly int DEAD_MAX = 4;

	// Use this for initialization
	void Start () {
        m_animator = GetComponent<Animator>();
	}
	
    public void Move(Vector3 move, bool run = false, Transform look = null)
    {
        if (m_isDead) return;

        move.Normalize();

        // rotate
        float step = m_turnSpeed * Time.deltaTime;
        Vector3 dic = look == null ? move : (look.position - transform.position).normalized;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, dic, step, 0.0F);
        newDir.y = 0;
        transform.rotation = Quaternion.LookRotation(newDir);

        // position
        Vector3 dir = transform.InverseTransformDirection(move);
        float speed = run ? m_moveSpeed : m_moveSpeed * 0.5f;
        float axcel = look == null ? Mathf.Clamp(dir.z, 0, dir.z) : 1;
        Vector3 movement = move * axcel * speed * Time.deltaTime;
        transform.position = transform.position + movement;

        // animator
        if (m_animator)
        {
            dir = run ? dir : dir * 0.5f;
            UpdateAnimator(dir);
        }
    }

    public void UpdateAnimator(Vector3 dir) {
        m_animator.SetFloat("Forward", dir.z, 0.1f, Time.deltaTime);
        m_animator.SetFloat("Turn", dir.x, 0.1f, Time.deltaTime);
    }

    public void Dead()
    {
        if (m_isDead) return;

        m_isDead = true;

        if(m_animator)
            m_animator.SetBool("Dead" + (int)Random.Range(1, DEAD_MAX), true);
    }
}
