﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UniRx;
using UniRx.Triggers;

namespace Enemy
{
    public enum standard_State
    {
        move,
        warning,
        chase,
        attack,
        dead
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy_Standard : EnemyBase<Enemy_Standard, standard_State>, IEnemy
    {

        [Header("現在のステート")]
        public standard_State m_state;

        [Header("分隊番号")]
        public int m_group;

        public float m_time = 2.0f;
        public Vector3 m_lastPosition;
        public Transform[] m_lootPosition;

        public ApproachAttack m_attack;


        private Enemy_Standard_battle_behavior m_tree;

        private readonly Vector3 m_vectorZero = new Vector3(0, 0, 0);

        [Header("デバッグ確認用")]
        public float m_distance;
        public int m_rootNum;

        protected override void Start()
        {
            m_stateList.Add(new StateMove(this));
            m_stateList.Add(new StateWarning(this));
            m_stateList.Add(new StateChase(this));
            m_stateList.Add(new StateAttack(this));
            m_stateList.Add(new StateDead(this));

            m_stateMachine = new StateMachine<Enemy_Standard>();

            m_attack = GetComponent<ApproachAttack>();
            m_tree = new Enemy_Standard_battle_behavior(this);

            base.Start();
        }

        public override void Initialize()
        {
            ChangeState(standard_State.move);
        }

        public override void ChangeState(standard_State state)
        {
            base.ChangeState(state);
            this.m_state = state;
        }

        public override void Damage(int damage)
        {
            if (_charaPara.isDead) { return; }

            EnemyMgr.i.GetWarningEnemys(transform.position);
            base.Damage(damage);
        }

        public override void Dead()
        {
            Debug.Log("死んだぁ！！");
            ChangeState(standard_State.dead);
            EnemyMgr.i.OnDeadEnemy(m_group);
            base.Dead();
            Destroy(gameObject);
        }

        public Transform[] LootPosition
        {
            get { return m_lootPosition; }
            set { m_lootPosition = value; }
        }


        public void RightShot(Vector3 vector)
        {
            EnemyRightArmtShot(new Ray(m_viewPoint.position, vector));
        }

        public void LeftShot(Vector3 vector)
        {
            EnemyLeftArmShot(new Ray(m_viewPoint.position, vector));
        }

        #region ---------------  State処理  ---------------

        // 移動ステート
        public class StateMove : State<Enemy_Standard>
        {
            public StateMove(Enemy_Standard dev) : base(dev) { }

            private float distance;
            private int currentRootNum = 0;

            public override void OnEnter()
            {

                // 最初の徘徊ポジションの決定
                // 現在のポジションから一番近いポジションをスタートにする
                distance = Vector3.SqrMagnitude(_base.m_lootPosition[0].position - _base.transform.position);
                float adis;
                for (int i = 1; i < _base.m_lootPosition.Length; i++)
                {
                    adis = Vector3.SqrMagnitude(_base.m_lootPosition[i].position - _base.transform.position);
                    if (distance > adis)
                    {
                        distance = adis;
                        currentRootNum = i;
                    }
                }
                _base.m_rootNum = currentRootNum;
            }

            public override void OnExecute()
            {
                // メイン視界に敵を発見
                if (_base.IsMainSearch())
                {
                    _base.ChangeState(standard_State.chase);
                    return;
                }
                // サブ視界に敵、又は何かを発見
                if (_base.IsSubSearch())
                {
                    _base.ChangeState(standard_State.warning);
                    return;
                }

                // ルート徘徊
                if (_base.m_agent.remainingDistance < 2.0f && _base.m_agent.hasPath)
                {
                    currentRootNum = (currentRootNum + 1) % _base.m_lootPosition.Length;
                    _base.m_rootNum = currentRootNum;
                }
                _base.m_agent.SetDestination(_base.m_lootPosition[currentRootNum].position);
            }

            public override void OnExit()
            {
            }
        }


        // 警戒ステート
        public class StateWarning : State<Enemy_Standard>
        {
            public StateWarning(Enemy_Standard dev) : base(dev) { }

            public override void OnExecute()
            {
                // メイン視界に敵を発見
                if (_base.IsMainSearch() || _base.IsSubSearch())
                {
                    _base.ChangeState(standard_State.chase);
                    return;
                }

                // ターゲットの最終発見ポイントがあればそこまで移動
                if (_base.m_lastPosition != _base.m_vectorZero)
                {
                    _base.m_agent.SetDestination(_base.m_lastPosition);

                    if (_base.m_agent.remainingDistance < 2.0f && _base.m_agent.hasPath)
                    {
                        _base.m_lastPosition = _base.m_vectorZero;
                        _base.ChangeState(standard_State.move);
                    }
                    return;
                }
            }
        }


        // 追跡ステート
        public class StateChase : State<Enemy_Standard>
        {
            public StateChase(Enemy_Standard dev) : base(dev) { }

            private int m_timer = 0;

            public override void OnEnter()
            {
                EnemyMgr.i.GetWarningEnemys(_base.transform.position);
            }

            public override void OnExecute()
            {
                // ターゲットが視界外に消えてからn秒後にMoveStateに移行
                if (!_base.IsMainSearch() && !_base.IsSubSearch())
                {
                    m_timer++;
                    if(m_timer >= (_base.m_time / Time.deltaTime))
                    {
                        _base.m_lastPosition = _base.m_target.position;
                        _base.ChangeState(standard_State.warning);
                        return;
                    }
                }
                else
                {
                    // 視界内、尚且つタイマーがカウントされていればリセット
                    if(m_timer > 0)
                    {
                        m_timer = 0;
                    }
                }


                if(_base.IsAttackSearch())
                {
                    _base.ChangeState(standard_State.attack);
                    return;
                }

                _base.m_agent.SetDestination(_base.m_target.position);
                _base.m_distance = _base.m_agent.remainingDistance;
            }

            public override void OnExit()
            {
                _base.m_agent.ResetPath();
            }
        }


        // 攻撃ステート
        public class StateAttack : State<Enemy_Standard>
        {
            public StateAttack(Enemy_Standard dev) : base(dev) { }

            public override void OnEnter()
            {
            }

            public override void OnExecute()
            {
                if (!_base.IsAttackSearch())
                {
                    _base.ChangeState(standard_State.chase);
                    return;
                }

                _base.m_tree.UpdateBattleState();
            }

            public override void OnExit()
            {
            }
        }


        public class StateDead : State<Enemy_Standard>
        {
            public StateDead(Enemy_Standard dev) : base(dev) { }

            public override void OnEnter()
            {
            }

            public override void OnExecute()
            {
            }

            public override void OnExit()
            {
            }
        }

        #endregion
    }
}
