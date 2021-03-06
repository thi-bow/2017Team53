﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneManagerScript : MonoBehaviour
{
    protected static SceneManagerScript Change;
    #region シーン移動時に使うオブジェクト
    [SerializeField]
    private GameObject _fade_Object;
    private RectTransform _fade;
    [SerializeField]
    private GameObject _lord;
    #endregion
    #region フェードの管理
    [SerializeField]
    private bool _scene_Fade;
    //フェードインの時間
    [SerializeField]
    private float _intime;
    //フェードアウトの時間
    [SerializeField]
    private float _outtime;
    //シーン移動開始のフラグ
    private bool FadeStart;
    private EventSystem _eventSystem;
    #endregion
    #region 何かボタンを押してシーン移動
    [SerializeField]
    private bool _anyButtonMove;
    [SerializeField]
    private string _anyButtonMoveName;
    #endregion
    #region 放置でシーン移行
    [SerializeField]
    private bool _leave_Alone;
    [SerializeField]
    private float _waiting_Time;
    [SerializeField]
    private string _waiting_Scene;
    private float _wTime_Count;
    [SerializeField]
    private bool _leave_Alone_Quit;
    [SerializeField]
    private float _quitTime;
    private static float _quitCount;
    #endregion
    #region BGMを流す
    [SerializeField]
    private bool _bgmMade;
    [SerializeField]
    private int _BGM_Number;
    [SerializeField]
    private bool _sound_FadeIN;
    [SerializeField]
    private bool _sound_FadeOUT;
    #endregion
    #region ゲーム終了の処理
    [SerializeField]
    private GameObject _endDialog;
    #endregion


    //どこでも参照可
    public static SceneManagerScript sceneManager
    {
        get
        {
            if (Change == null)
            {
                Change = (SceneManagerScript)FindObjectOfType(typeof(SceneManagerScript));
                if (Change == null)
                {
                    Debug.LogError("SceneChange Instance Error");
                }
            }

            return Change;
        }
    }

    void Awake()
    {
        //カーソル非表示
        Cursor.visible = false;
        _eventSystem = GameObject.FindObjectOfType<EventSystem>();
        _fade = _fade_Object.GetComponent<RectTransform>();
        if (_lord != null)
        {
            _lord.SetActive(false);
        }
        FadeStart = false;

        if (_scene_Fade == true)
        {
            FadeIn();
        }

        //フェード時間が0秒以下なら0.1を入れる
        if (_intime <= 0)  _intime  = 0.1f;
        if (_outtime <= 0) _outtime = 0.1f;
        _wTime_Count = 0;
    }

    void Start()
    {
        if (_bgmMade == true)
        {
            if (_sound_FadeIN == true) SoundManger.Instance.FadeInBGM(_BGM_Number);
            else SoundManger.Instance.PlayBGM(_BGM_Number);
        }
    }
    public void Update()
    {
        //何かボタンを押して移動するなら下の処理を行う
        if (_anyButtonMove == true && Input.anyKeyDown)
        {
            SoundManger.Instance.PlaySE(0);
            FadeOut(_anyButtonMoveName);
            return;
        }

        //Escキーをおしたらゲーム終了
        if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetButton("BackButton") && Input.GetButton("Pause")))
        {
            QuitCheck();
            return;
        }

        //放置でシーン移動するならこの下の処理を行う
        if (_leave_Alone == true)
        {
            _wTime_Count += Time.deltaTime;
            if (_wTime_Count >= _waiting_Time)
            {
                SceneOut(_waiting_Scene);
                _wTime_Count = 0;
            }
            if (Input.anyKeyDown ||
                Input.GetAxis("Horizontal") != 0 ||
                Input.GetAxis("Vertical") != 0 ||
                Input.GetAxis("Lock") != 0 ||
                Input.GetAxis("RightStick") != 0)
            {
                _wTime_Count = 0;
            }
        }
        if(_leave_Alone_Quit == true)
        {
            _quitCount += Time.deltaTime;
            if(_quitCount >= _quitTime)
            {
                Quit();
            }
            if (Input.anyKeyDown ||
                Input.GetAxis("Horizontal") != 0 ||
                Input.GetAxis("Vertical")  != 0 ||
                Input.GetAxis("Lock") != 0 || 
                Input.GetAxis("RightStick") != 0)
            {
                _quitCount = 0;
            }
        }
    }

    /// <summary>
    ///フェードイン 
    /// </summary>
    public void FadeIn()
    {
        FadeStart = true;
        //フェードイン中は入力不可にする
        _eventSystem.enabled = false;
        _fade.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        _fade_Object.SetActive(true);
        LeanTween.alpha(_fade, 0.0f, _intime)
            .setOnComplete(() =>
            {
                _fade_Object.SetActive(false);
                //フェードが終わったFlagを立てる
                FadeStart = false;
                //入力を可能にする
                _eventSystem.enabled = true;
            });
    }

    /// <summary>
    ///フェードアウトによるシーン移動(番号参照) 
    /// </summary>
    /// <param name="number">移動先のシーンの番号</param>
    public void FadeOut(int number)
    {
        if (FadeStart == true)
        {
            return;
        }
        FadeStart = true;
        _fade_Object.SetActive(true);
        _fade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        //BGMをフェードアウトさせる
        if (_bgmMade == true)
        {
            if (_sound_FadeOUT == true) SoundManger.Instance.FadeOutBGM();
        }
        LeanTween.alpha(_fade, 1, _outtime)
            .setOnComplete(() =>
            {
                SceneOut(number);
            });
    }

    /// <summary>
    ///フェードアウトによるシーン移動(名前参照) 
    /// </summary>
    /// <param name="name">移動先のシーンの名前</param>
    public void FadeOut(string name)
    {
        TimeStart();
        if (FadeStart == true)
        {
            return;
        }
        FadeStart = true;
        _fade_Object.SetActive(true);
        _fade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        if (_bgmMade == true)
        {
            //BGMをフェードアウトさせる
            if (_sound_FadeOUT == true) SoundManger.Instance.FadeOutBGM();
        }
        LeanTween.alpha(_fade, 1, _outtime)
            .setOnComplete(() =>
            {
                SceneOut(name);
            });
    }

    /// <summary>
    /// シーン移動(番号参照)
    /// </summary>
    /// <param name="number">移動先のシーンの番号</param>
    public void SceneOut(int number)
    {
        TimeStart();
        SceneManager.LoadSceneAsync(number);
        SoundManger.Instance.StopSE();
        //ロード中に動かす画像があったら表示させる
        if (_lord != null)
        {
            _lord.SetActive(true);
        }
    }

    /// <summary>
    /// シーン移動(名前参照)
    /// </summary>
    /// <param name="name">移動先のシーンの名前</param>
    public void SceneOut(string name)
    {
        SceneManager.LoadSceneAsync(name);
        SoundManger.Instance.StopSE();
        //ロード中に動かす画像があったら表示させる
        if (_lord != null)
        {
            _lord.SetActive(true);
        }
    }

    /// <summary>
    /// 現在のシーンをフェードアウトしてから新しく読み込む
    /// </summary>
    public void FadeNowScene()
    {
        FadeOut(SceneName());
    }

    /// <summary>
    /// 現在のシーンを新しく読み込む
    /// </summary>
    public void NowScene()
    {
        SceneOut(SceneName());
    }

    /// <summary>
    /// シーン名を取得
    /// </summary>
    /// <returns></returns>
    public string SceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// ゲームを終了する
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// ゲームを止め、終了を確認するダイアログを表示させる
    /// </summary>
    public void QuitCheck()
    {
        //終了確認をするダイアログがなければ、確認しないでゲームを終了させる
        if(_endDialog == null)
        {
            Quit();
            return;
        }
        //すでにダイアログが出ていたら、ダイアログを消してゲームを再開する
        if(_endDialog.activeSelf == true)
        {
            QuitCancel();
        }
        FadeBlack();
        _endDialog.SetActive(true);
    }

    /// <summary>
    /// ダイアログを消して、ゲームを再開させる
    /// </summary>
    public void QuitCancel()
    {
        //終了確認をするダイアログがない、もしくは表示されていないなら、確認しないでゲームを終了させる
        if (_endDialog == null || _endDialog.activeSelf == false) return;
        _endDialog.SetActive(false);
        FadeWhite();
    }

    /// <summary>
    /// シーン移動せずに画面全体を薄暗くする
    /// </summary>
    public void Black()
    {
        if (FadeStart == true)
        {
            return;
        }
        _fade_Object.SetActive(true);
        FadeStart = true;
        LeanTween.alpha(_fade, 0.5f, 0.1f)
            .setOnComplete(() =>
            {
                FadeStart = false;
            });
    }

    /// <summary>
    /// シーン移動せずに画面全体を薄暗くする
    /// </summary>
    public void Black(float value)
    {
        if (FadeStart == true)
        {
            return;
        }
        _fade_Object.SetActive(true);
        FadeStart = true;
        LeanTween.alpha(_fade, value, 0.1f)
            .setOnComplete(() =>
            {
                FadeStart = false;
            });
    }

    /// <summary>
    /// シーン移動せずに画面全体を薄暗くして時間を止める
    /// </summary>
    public void FadeBlack()
    {
        if (FadeStart == true)
        {
            return;
        }
        _fade_Object.SetActive(true);
        FadeStart = true;
        LeanTween.alpha(_fade, 0.5f, 0.1f)
            .setOnComplete(() =>
            {
                FadeStart = false;
                TimeStop();
            });
    }

    /// <summary>
    /// シーン移動せずに画面全体を薄暗くして時間を止める(値を自分で決める)
    /// </summary>
    /// <param name="blacknumber">黒くなる値
    /// /param>
    public void FadeBlack(float value)
    {
        if (FadeStart == true)
        {
            return;
        }
        _fade_Object.SetActive(true);
        FadeStart = true;
        LeanTween.alpha(_fade, value, 0.1f)
            .setOnComplete(() =>
            {
                FadeStart = false;
                TimeStop();
            });
    }

    /// <summary>
    /// 暗くなっている画面を明るくする
    /// </summary>
    public void White()
    {
        if (FadeStart == true)
        {
            return;
        }

        FadeStart = true;
        LeanTween.alpha(_fade, 0, 0.1f)
            .setOnComplete(() =>
            {
                _fade_Object.SetActive(false);
                FadeStart = false;
            });
    }

    /// <summary>
    /// 暗くなっている画面を明るくして時間を動かす
    /// </summary>
    public void FadeWhite()
    {
        if (FadeStart == true)
        {
            return;
        }

        FadeStart = true;
        TimeStart();
        LeanTween.alpha(_fade, 0, 0.1f)
            .setOnComplete(() =>
            {
                _fade_Object.SetActive(false);
                FadeStart = false;
            });
    }

    /// <summary>
    /// 時間を止める
    /// </summary>
    public void TimeStop()
    {
        Time.timeScale = 0;
    }

    /// <summary>
    /// 時間を動かす
    /// </summary>
    public void TimeStart()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// 任意の速さにする
    /// </summary>
    /// <param name="speed"></param>
    public void TimeSet(float speed)
    {
        Time.timeScale = speed;
    }

    /// <summary>
    /// 何かボタンを押したらシーン移動のフラグを変更する
    /// </summary>
    /// <param name="flag">trueにするかfalseにするか</param>
    /// <param name="stageName">移動するシーン名</param>
    public void AnyButtonOn(bool flag, string stageName)
    {
        _anyButtonMoveName = stageName;
        _anyButtonMove = flag;
    }

    
#if UNITY_EDITOR
    [CustomEditor(typeof(SceneManagerScript))]
    public class SceneManagerEditor : Editor
    {
        SerializedProperty Fade_Object;
        SerializedProperty Lord;
        SerializedProperty Scene_Fade;
        SerializedProperty Intime;
        SerializedProperty Outtime;
        SerializedProperty Leave_Alone;
        SerializedProperty Waiting_Time;
        SerializedProperty Waiting_Scene;
        SerializedProperty BgmMade;
        SerializedProperty BGM_Number;
        SerializedProperty Fade_IN_Sound;
        SerializedProperty Fade_OUT_Sound;
        SerializedProperty EndDialog;
        SerializedProperty AnyButtonMove;
        SerializedProperty AnyButtonMoveName;
        SerializedProperty LeaveAloneQuit;
        SerializedProperty QuitTime;

        public void OnEnable()
        {
            Fade_Object = serializedObject.FindProperty("_fade_Object");
            Lord = serializedObject.FindProperty("_lord");
            Intime = serializedObject.FindProperty("_intime");
            Outtime = serializedObject.FindProperty("_outtime");
            Leave_Alone = serializedObject.FindProperty("_leave_Alone");
            Waiting_Time = serializedObject.FindProperty("_waiting_Time");
            Waiting_Scene = serializedObject.FindProperty("_waiting_Scene");
            BgmMade = serializedObject.FindProperty("_bgmMade");
            BGM_Number = serializedObject.FindProperty("_BGM_Number");
            Fade_IN_Sound = serializedObject.FindProperty("_sound_FadeIN");
            Fade_OUT_Sound = serializedObject.FindProperty("_sound_FadeOUT");
            Scene_Fade = serializedObject.FindProperty("_scene_Fade");
            EndDialog = serializedObject.FindProperty("_endDialog");
            AnyButtonMove = serializedObject.FindProperty("_anyButtonMove");
            AnyButtonMoveName = serializedObject.FindProperty("_anyButtonMoveName");
            LeaveAloneQuit = serializedObject.FindProperty("_leave_Alone_Quit");
            QuitTime = serializedObject.FindProperty("_quitTime");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SceneManagerScript scene = target as SceneManagerScript;

            Fade_Object.objectReferenceValue = EditorGUILayout.ObjectField("フェードさせる画像", scene._fade_Object, typeof(GameObject), true) as GameObject;
            Lord.objectReferenceValue = EditorGUILayout.ObjectField("ロード中に出す画像", scene._lord, typeof(GameObject), true) as GameObject;

            EditorGUILayout.Space();
            Scene_Fade.boolValue = EditorGUILayout.Toggle("フェードインをさせる", scene._scene_Fade);
            EditorGUILayout.LabelField("フェード時間( IN / OUT )");
            EditorGUILayout.BeginHorizontal();
            Intime.floatValue = EditorGUILayout.FloatField(scene._intime, GUILayout.Width(32));
            Outtime.floatValue = EditorGUILayout.FloatField(scene._outtime, GUILayout.Width(32));
            EditorGUILayout.EndHorizontal();

            EndDialog.objectReferenceValue = EditorGUILayout.ObjectField("終了確認ダイアログ", scene._endDialog, typeof(GameObject), true) as GameObject;

            Leave_Alone.boolValue = EditorGUILayout.Toggle("放置したらシーン移動", scene._leave_Alone);
            if(scene._leave_Alone == true)
            {
                Waiting_Time.floatValue = EditorGUILayout.FloatField("放置時間", scene._waiting_Time);
                Waiting_Scene.stringValue = EditorGUILayout.TextField("移動するシーン", scene._waiting_Scene);
            }
            
            LeaveAloneQuit.boolValue = EditorGUILayout.Toggle("放置したらゲーム終了", scene._leave_Alone_Quit);
            if (scene._leave_Alone_Quit == true)
            {
                QuitTime.floatValue = EditorGUILayout.FloatField("放置時間", scene._quitTime);
            }

            AnyButtonMove.boolValue = EditorGUILayout.Toggle("何かボタンを押したらシーン移動", scene._anyButtonMove);
            if(scene._anyButtonMove == true)
            {
                AnyButtonMoveName.stringValue = EditorGUILayout.TextField("移動するシーン", scene._anyButtonMoveName);
            }

            EditorGUILayout.Space();
            BgmMade.boolValue = EditorGUILayout.Toggle("BGMを流す", scene._bgmMade);
            if (scene._bgmMade == true)
            {
                BGM_Number.intValue = EditorGUILayout.IntField("BGM番号", scene._BGM_Number);
                Fade_IN_Sound.boolValue = EditorGUILayout.Toggle("BGMフェードイン", scene._sound_FadeIN);
                Fade_OUT_Sound.boolValue = EditorGUILayout.Toggle("BGMフェードアウト", scene._sound_FadeOUT);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
