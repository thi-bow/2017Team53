﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CharaParameter
{

    [Header("キャラクターのHP")]
    #region Hp
    public int _hp = 1000;
    public int _bodyHp = 0;
    public int _rightArmHp = 0;
    public int _leftArmHp = 0;
    public int _legHp = 0;
    public int _boosterHp = 0;
    #endregion

    [Header("キャラクターのDefense")]
    #region Defense
    public int _defense = 0;
    public int _bodyDefense = 0;
    public int _rightArmDefense = 0;
    public int _leftArmDefense = 0;
    public int _legDefense = 0;
    public int _boosterDefense = 0;
    #endregion

    [Header("キャラクターのWeight")]
    #region Weight
    public int _totalWeight = 0;
    public int _maxWeight = 0;
    public int _bodyWeight = 0;
    public int _rightArmWeight = 0;
    public int _leftArmWeight = 0;
    public int _legWeight = 0;
    public int _boosterWeight = 0;
    #endregion

    [Header("キャラクターのWeight")]
    #region Lv
    public int _bodyLevel = 1;
    public int _rightArmLevel = 1;
    public int _leftArmLevel = 1;
    public int _legLevel = 1;
    public int _boosterLevel = 1;
    #endregion

    [Space(10)]
    public int _attack = 1;
    [Space(10)]
    public float _speed = 1.0f;


    [Header("キャラクターの装備見た目変更する最低個数")]
    public int _rightArm_BorderNumber = 5;
    public int _leftArm_BorderNumber = 5;
    public int _leg_BorderNumber = 5;

    [Header("キャラクターの装備見た目を切り替える個数")]
    public int _rightArm_SwitchNumber = 5;
    public int _leftArm_SwitchNumber = 5;
    public int _leg_SwitchNumber = 5;

    [System.NonSerialized] public Weapon.Attack_State _rightArm_AttackState = Weapon.Attack_State.NULL;
    [System.NonSerialized] public Weapon.Attack_State _leftArm_AttackState = Weapon.Attack_State.NULL;
    [System.NonSerialized] public Weapon.Attack_State _leg_AttackState = Weapon.Attack_State.NULL;
}

public class CharaBase : MonoBehaviour
{
    public enum Parts
    {
        Body = 0,
        RightArm,
        LeftArm,
        Leg,
        Booster,
        WeakPoint,
    }

    public CharaParameter _charaPara;

    #region 装備
    [Header("キャラクターベース")]
    [SerializeField] private List<Armor> _bodyList = new List<Armor>();
    [SerializeField] private List<Armor> _rightArmList = new List<Armor>();
    [SerializeField] private List<Armor> _leftArmList = new List<Armor>();
    [SerializeField] private List<Armor> _legList = new List<Armor>();
    [SerializeField] private List<Armor> _boosterList = new List<Armor>();

    [Space(10)]
    [SerializeField] private List<Armor> _legPartsPair = new List<Armor>();
    List<Parts> _allPartsList = new List<Parts>(); 
    private int partsMax = 5;
    private Parts _parts;

    [Space(10)]
    [SerializeField] private GameObject[] _partsLocation;
    #endregion

    protected Action _deadAction = null;


    #region 装備のプロパティ

    public List<Armor> BodyArmorList
    {
        get { return _bodyList; }
    }

    public List<Armor> RightArmArmorList
    {
        get { return _rightArmList; }
    }

    public List<Armor> LeftArmArmorList
    {
        get { return _leftArmList; }
    }

    public List<Armor> LegArmorList
    {
        get { return _legList; }
    }

    public List<Armor> BoosterArmorList
    {
        get { return _boosterList; }
    }
    #endregion

    #region HPのプロパティ
    public int HP
    {
        get { return _charaPara._hp; }
    }
    public int BodyHP
    {
        get { return _charaPara._bodyHp; }
    }
    public int RightArmHP
    {
        get { return _charaPara._rightArmHp; }
    }
    public int LeftArmHP
    {
        get { return _charaPara._leftArmHp; }
    }
    public int LegHP
    {
        get { return _charaPara._legHp; }
    }
    public int BoosterHP
    {
        get { return _charaPara._boosterHp; }
    }

    #endregion

    #region 攻撃のプロパティ
    public int Attack
    {
        get { return _charaPara._attack; }
    }

    #endregion

    #region 移動のプロパティ
    public float Speed
    {
        get { return _charaPara._speed; }
    }
    #endregion

    // Use this for initialization
    protected virtual void Start ()
    {
        _allPartsList = new List<Parts> { Parts.Body, Parts.RightArm, Parts.LeftArm, Parts.Leg, Parts.Booster };
    }

    // Update is called once per frame
    protected virtual void Update ()
    {
    }

    protected List<Armor> GetPartsList(Parts partsCheck)
    {
        List<Armor> partsList = new List<Armor>();
        switch (partsCheck)
        {
            case Parts.Body:
                partsList = BodyArmorList;
                break;
            case Parts.RightArm:
                partsList = RightArmArmorList;
                break;
            case Parts.LeftArm:
                partsList = LeftArmArmorList;
                break;
            case Parts.Leg:
                partsList = LegArmorList;
                break;
            case Parts.Booster:
                partsList = BoosterArmorList;
                break;
            default:
                break;
        }
        return partsList;
    }

    //パーツの装着
    #region PartsAdd
    public void PartsAdd(Parts parts, Armor armor)
    {
        if (_charaPara._totalWeight >= _charaPara._maxWeight)
        {
            return;
        }
        armor.GetComponent<BoxCollider>().enabled = false;
        int _shootNumber = 0;
        switch (parts)
        {
            case Parts.Body:
                _bodyList.Add(armor);
                //装備のパラメータをプレイヤーに上乗せする
                _charaPara._bodyDefense += armor.ArmorDefPara;
                _charaPara._bodyHp += armor.ArmorHpPara;
                _charaPara._bodyWeight += armor.ArmorWeightPara;
                armor.gameObject.transform.SetParent(_partsLocation[0].transform);
                armor.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                break;
            case Parts.RightArm:
                _rightArmList.Add(armor);
                //装備のパラメータをプレイヤーに上乗せする
                _charaPara._rightArmDefense += armor.ArmorDefPara;
                _charaPara._rightArmHp += armor.ArmorHpPara;
                _charaPara._rightArmWeight += armor.ArmorWeightPara;
                armor.gameObject.transform.SetParent(_partsLocation[1].transform);
                armor.transform.localPosition = PartsAddPara.PlayerRightArmPosition[_rightArmList.Count - 1];
                armor.transform.localRotation = Quaternion.Euler(PartsAddPara.PlayerRightArmRotation[_rightArmList.Count - 1]);

                if(_rightArmList.Count < _charaPara._rightArm_BorderNumber)
                {
                    break;
                }
                //右腕が近接攻撃特化か、遠距離攻撃特化か見極める
                for(int i = 0; i < _rightArmList.Count; i++)
                {
                    if (_rightArmList[i].GetComponent<Weapon>() != null && _rightArmList[i].GetComponent<Weapon>().state == Weapon.Attack_State.shooting)
                    {
                        _shootNumber++;
                    }
                    else if (_rightArmList[i].GetComponent<Weapon>() != null && _rightArmList[i].GetComponent<Weapon>().state == Weapon.Attack_State.approach)
                    {
                        _shootNumber--;
                    }
                }
                if(_shootNumber >= _charaPara._rightArm_SwitchNumber)
                {
                    print("右腕を遠距離攻撃に切り替えた");
                    _charaPara._rightArm_AttackState = Weapon.Attack_State.shooting;
                }
                else if(_shootNumber <= -_charaPara._rightArm_SwitchNumber)
                {
                    print("右腕を近距離攻撃に切り替えた");
                    _charaPara._rightArm_AttackState = Weapon.Attack_State.approach;
                }
                else
                {
                    print("右腕をガラクタがくっついている状態に切り替えた");
                    _charaPara._rightArm_AttackState = Weapon.Attack_State.NULL;
                }
                break;
            case Parts.LeftArm:
                _leftArmList.Add(armor);
                //装備のパラメータをプレイヤーに上乗せする
                _charaPara._leftArmDefense += armor.ArmorDefPara;
                _charaPara._leftArmHp += armor.ArmorHpPara;
                _charaPara._leftArmWeight += armor.ArmorWeightPara;
                armor.gameObject.transform.SetParent(_partsLocation[2].transform);
                armor.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                
                if (_leftArmList.Count < _charaPara._leftArm_BorderNumber)
                {
                    break;
                }
                //右腕が近接攻撃特化か、遠距離攻撃特化か見極める
                for (int i = 0; i < _leftArmList.Count; i++)
                {
                    if (_leftArmList[i].GetComponent<Weapon>() != null && _leftArmList[i].GetComponent<Weapon>().state == Weapon.Attack_State.shooting)
                    {
                        _shootNumber++;
                    }
                    else if (_leftArmList[i].GetComponent<Weapon>() != null && _leftArmList[i].GetComponent<Weapon>().state == Weapon.Attack_State.approach)
                    {
                        _shootNumber--;
                    }
                }
                if (_shootNumber >= _charaPara._leftArm_SwitchNumber)
                {
                    print("左腕を遠距離攻撃に切り替えた");
                    _charaPara._leftArm_AttackState = Weapon.Attack_State.shooting;
                }
                else if (_shootNumber <= -_charaPara._leftArm_SwitchNumber)
                {
                    print("左腕を近距離攻撃に切り替えた");
                    _charaPara._leftArm_AttackState = Weapon.Attack_State.approach;
                }
                else
                {
                    print("左腕をガラクタがくっついている状態に切り替えた");
                    _charaPara._leg_AttackState = Weapon.Attack_State.NULL;
                }
                break;
            case Parts.Leg:
                _legList.Add(armor);
                //装備のパラメータをプレイヤーに上乗せする
                Armor pair = Instantiate(armor);
                _charaPara._legDefense += armor.ArmorDefPara;
                _charaPara._legHp += armor.ArmorHpPara;
                _charaPara._legWeight += armor.ArmorWeightPara;

                //足に装着する場合は、右足と左足両方に装着する
                armor.gameObject.transform.SetParent(_partsLocation[3].transform);
                armor.transform.localPosition = PartsAddPara.PlayerRightLegPosition[_legList.Count - 1];
                armor.transform.localRotation = Quaternion.Euler(PartsAddPara.PlayerRightLegRotation[_legList.Count - 1]);
                pair.gameObject.transform.SetParent(_partsLocation[4].transform);
                pair.transform.localPosition = PartsAddPara.PlayerLeftLegPosition[_legList.Count - 1];
                pair.transform.localRotation = Quaternion.Euler(PartsAddPara.PlayerLeftLegRotation[_legList.Count - 1]);
                _legPartsPair.Add(pair);

                if (_legList.Count < _charaPara._leg_BorderNumber)
                {
                    break;
                }
                //脚が近接攻撃特化か、遠距離攻撃特化か見極める
                for (int i = 0; i < _leftArmList.Count; i++)
                {
                    if (_legList[i].GetComponent<Weapon>() != null && _legList[i].GetComponent<Weapon>().state == Weapon.Attack_State.shooting)
                    {
                        _shootNumber++;
                    }
                    else if (_legList[i].GetComponent<Weapon>() != null && _legList[i].GetComponent<Weapon>().state == Weapon.Attack_State.approach)
                    {
                        _shootNumber--;
                    }
                }
                if (_shootNumber >= _charaPara._leg_SwitchNumber)
                {
                    print("脚を遠距離攻撃に切り替えた");
                    _charaPara._leg_AttackState = Weapon.Attack_State.shooting;
                }
                else if (_shootNumber <= -_charaPara._leg_SwitchNumber)
                {
                    print("脚を近距離攻撃に切り替えた");
                    _charaPara._leg_AttackState = Weapon.Attack_State.approach;
                }
                else
                {
                    print("脚をガラクタがくっついている状態に切り替えた");
                    _charaPara._leg_AttackState = Weapon.Attack_State.NULL;
                }
                break;
            case Parts.Booster:
                _boosterList.Add(armor);
                //装備のパラメータをプレイヤーに上乗せする
                _charaPara._boosterDefense += armor.ArmorDefPara;
                _charaPara._boosterHp += armor.ArmorHpPara;
                _charaPara._boosterWeight += armor.ArmorWeightPara;
                armor.gameObject.transform.SetParent(_partsLocation[5].transform);
                armor.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                break;
            default:
                break;
        }
        _charaPara._totalWeight += armor.ArmorWeightPara;
    }
    #endregion

    #region PartsCheck
    protected void PartsCheck()
    {

    }
    #endregion

    //部位ごとにパージする
    #region PartsPurge
    public void PartsPurge(Parts parts, Action action = null)
    {
        switch (parts)
        {
            case Parts.Body:
                if (_bodyList.Count <= 0) return;
                for (int i = 0; i < _bodyList.Count; i++)
                {
                    _bodyList[i].transform.parent = null;
                }
                if(action != null)
                {
                    action();
                }
                _bodyList.Clear();
                _charaPara._bodyDefense = 0;
                _charaPara._totalWeight -= _charaPara._bodyWeight;
                _charaPara._bodyWeight = 0;
                break;
            case Parts.RightArm:
                if (_rightArmList.Count <= 0) return;
                for (int i = 0; i < _rightArmList.Count; i++)
                {
                    _rightArmList[i].transform.parent = null;
                }
                if (action != null)
                {
                    action();
                }
                _rightArmList.Clear();
                _charaPara._rightArmDefense = 0;
                _charaPara._totalWeight -= _charaPara._rightArmWeight;
                _charaPara._rightArmWeight = 0;
                break;
            case Parts.LeftArm:
                if (_leftArmList.Count <= 0) return;
                for (int i = 0; i < _leftArmList.Count; i++)
                {
                    _leftArmList[i].transform.parent = null;
                }
                if (action != null)
                {
                    action();
                }
                _leftArmList.Clear();
                _charaPara._leftArmDefense = 0;
                _charaPara._totalWeight -= _charaPara._leftArmWeight;
                _charaPara._leftArmWeight = 0;
                break;
            case Parts.Leg:
                if (_legList.Count <= 0) return;
                for (int i = 0; i < _legList.Count; i++)
                {
                    _legList[i].transform.parent = null;
                }
                if (action != null)
                {
                    action();
                }
                _legList.Clear();
                _charaPara._legDefense = 0;
                _charaPara._totalWeight -= _charaPara._legWeight;
                _charaPara._legWeight = 0;
                for(int i = 0; i < _legPartsPair.Count; i++)
                {
                    Destroy(_legPartsPair[i].gameObject);
                }
                _legPartsPair.Clear();
                break;
            case Parts.Booster:
                if (_boosterList.Count <= 0) return;
                for (int i = 0; i < _boosterList.Count; i++)
                {
                    _boosterList[i].transform.parent = null;
                }
                if (action != null)
                {
                    action();
                }
                _boosterList.Clear();
                _charaPara._boosterDefense = 0;
                _charaPara._totalWeight -= _charaPara._boosterWeight;
                _charaPara._boosterWeight = 0;
                break;
            default:
                break;
        }
    }
    #endregion

    //全ての装備をパージする
    #region FullParge
    public void FullParge(Action action = null)
    {
        //何も装備していなかったら何もしない
        if(_bodyList.Count + _rightArmList.Count + _leftArmList.Count + _legList.Count + _boosterList.Count <= 0)
        {
            return;
        }
        if (action != null)
        {
            action();
        }
        for (int i = 0; i < _allPartsList.Count; i++)
        {
            PartsPurge(_allPartsList[i]);
        }
    }
    #endregion

    //右腕の射撃攻撃
    #region RighArmtShot
    protected void RighArmtShot()
    {
        if (_rightArmList.Count <= 0) return;
        for(int i = 0; i < _rightArmList.Count; i++)
        {
            Weapon _wepon = null;
            _wepon = _rightArmList[i].GetComponent<Weapon>();
            if(_wepon == null && _wepon.state != Weapon.Attack_State.shooting)
            {
                print("右腕の" + i + "この装備には射撃がない");
                continue;
            }
            _wepon.Shooting();
        }
    }
    #endregion

    //左腕の射撃攻撃
    #region LeftArmShot
    protected void LeftArmShot()
    {
        if (_leftArmList.Count <= 0) return;
        for (int i = 0; i < _leftArmList.Count; i++)
        {
            Weapon _wepon = null;
            _wepon = _leftArmList[i].GetComponent<Weapon>();
            if (_wepon == null && _wepon.state != Weapon.Attack_State.shooting)
            {
                print("左腕の" + i + "この装備には射撃がない");
                continue;
            }
            _wepon.Shooting();
        }
    }
    #endregion

    //脚の射撃攻撃
    #region LegShot
    protected void LegShot()
    {
        if (_legList.Count <= 0) return;
        for (int i = 0; i < _legList.Count; i++)
        {
            Weapon _wepon = null;
            _wepon = _legList[i].GetComponent<Weapon>();
            if (_wepon == null && _wepon.state != Weapon.Attack_State.shooting)
            {
                print("足の" + i + "この装備には射撃がない");
                continue;
            }
            _wepon.Shooting();
        }
    }
    #endregion

    //右腕の射撃攻撃
    #region EnemyRighArmtShot
    protected void EnemyRighArmtShot()
    {
        if (_rightArmList.Count <= 0) return;
        for (int i = 0; i < _rightArmList.Count; i++)
        {
            Weapon _wepon = null;
            _wepon = _rightArmList[i].GetComponent<Weapon>();
            if (_wepon == null && _wepon.state != Weapon.Attack_State.shooting)
            {
                print("右腕の" + i + "この装備には射撃がない");
                continue;
            }
            _wepon.Shooting();
        }
    }
    #endregion

    //左腕の射撃攻撃
    #region EnemyLeftArmShot
    protected void EnemyLeftArmShot()
    {
        if (_leftArmList.Count <= 0) return;
        for (int i = 0; i < _leftArmList.Count; i++)
        {
            Weapon _wepon = null;
            _wepon = _leftArmList[i].GetComponent<Weapon>();
            if (_wepon == null && _wepon.state != Weapon.Attack_State.shooting)
            {
                print("左腕の" + i + "この装備には射撃がない");
                continue;
            }
            _wepon.Shooting();
        }
    }
    #endregion

    //脚の射撃攻撃
    #region EnemyLegShot
    protected void EnemyLegShot()
    {
        if (_legList.Count <= 0) return;
        for (int i = 0; i < _legList.Count; i++)
        {
            Weapon _wepon = null;
            _wepon = _legList[i].GetComponent<Weapon>();
            if (_wepon == null && _wepon.state != Weapon.Attack_State.shooting)
            {
                print("足の" + i + "この装備には射撃がない");
                continue;
            }
            _wepon.Shooting();
        }
    }
    #endregion

    //部位に攻撃が当たった時のダメージ計算
    #region PartsDamage
    public void PartsDamage(int attackPower, Parts parts, Action action = null)
    {
        switch (parts)
        {
            case Parts.Body:
                //パーツに何もついてなければ本体にダメージが入る
                if (_charaPara._bodyHp <= 0)
                {
                    Damage(attackPower);
                    break;
                }

                attackPower -= _charaPara._bodyDefense;
                if (attackPower <= 1)
                {
                    attackPower = 1;
                }
                _charaPara._bodyHp -= attackPower;
                if (_charaPara._bodyHp <= 0)
                {
                    _charaPara._bodyHp = 0;
                    PartsPurge(parts, action);
                }
                Damage(attackPower);
                break;
            case Parts.RightArm:
                //パーツに何もついてなければ本体にダメージが入る
                if (_charaPara._rightArmHp <= 0)
                {
                    Damage(attackPower);
                    break;
                }

                if (attackPower <= 1)
                {
                    attackPower = 1;
                }
                _charaPara._rightArmHp -= attackPower;
                if (_charaPara._rightArmHp <= 0)
                {
                    _charaPara._rightArmHp = 0;
                    PartsPurge(parts, action);
                }
                break;
            case Parts.LeftArm:
                //パーツに何もついてなければ本体にダメージが入る
                if (_charaPara._leftArmHp <= 0)
                {
                    Damage(attackPower);
                    break;
                }

                if (attackPower <= 1)
                {
                    attackPower = 1;
                }
                _charaPara._leftArmHp -= attackPower;
                if (_charaPara._leftArmHp <= 0)
                {
                    _charaPara._leftArmHp = 0;
                    PartsPurge(parts, action);
                }
                break;
            case Parts.Leg:
                //パーツに何もついてなければ本体にダメージが入る
                if (_charaPara._legHp <= 0)
                {
                    Damage(attackPower);
                    break;
                }

                if (attackPower <= 1)
                {
                    attackPower = 1;
                }
                _charaPara._legHp -= attackPower;
                if (_charaPara._legHp <= 0)
                {
                    _charaPara._legHp = 0;
                    PartsPurge(parts, action);
                }
                break;
            case Parts.Booster:
                //パーツに何もついてなければ本体にダメージが入る
                if (_charaPara._boosterHp <= 0)
                {
                    Damage(attackPower);
                    break;
                }

                if (attackPower <= 1)
                {
                    attackPower = 1;
                }
                _charaPara._boosterHp -= attackPower;
                if (_charaPara._boosterHp <= 0)
                {
                    _charaPara._boosterHp = 0;
                    PartsPurge(parts, action);
                }
                break;
            default:
                break;
        }

    }
    #endregion

    //ダメージを受けた時の計算
    #region Damage
    /// <summary>
    /// Damageを受けたときの処理
    /// </summary>
    /// <param name="attackPower">攻撃力</param>
    public virtual void Damage(int attackPower)
    {
        attackPower -= _charaPara._defense;
        if (attackPower <= 1)
        {
            attackPower = 1;
        }

        _charaPara._hp -= attackPower;
        if(_charaPara._hp <= 0)
        {
            Dead();
        }
    }
    #endregion

    #region Dead
    /// <summary>
    /// キャラクターの死亡処理
    /// </summary>
    public virtual void Dead()
    {
        if (_deadAction != null)
        {
            _deadAction();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

}
