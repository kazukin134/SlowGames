﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    MainGame,
    Result
}

public class GameDirector : MonoBehaviour {

    [SerializeField]
    private PlayerShot[] _playerShot = null;
    [SerializeField]
    private GenerateManager _generateManager = null;

    private bool _gamePlay = false;

    [SerializeField]
    private float _gameStartTime = 0.0f;

    private PlayerHP _hp = null;

    [SerializeField]
    private Canvas _resultCanvas = null;

    //[SerializeField]
    //private Image _gameClearImage = null;

    [SerializeField]
    private GameObject _boss = null;

    [SerializeField, Tooltip("敵を殺した数でボイスを流す、ボイスタイミング")]
    private int _voiceTiming = 0;

    //[SerializeField]
    //private Image _gameStartImage = null;

    private Dictionary<GameState, Action> _update = null;

    private GameState _state = GameState.MainGame;

    private bool _isBossDestroy = false;

    /// <summary>
    /// ボスが死んだときに呼ぶ関数
    /// </summary>
    public void isBossDestroy()
    {
        _isBossDestroy = true;
    }

    /// <summary>
    /// ゲーム中かどうか
    /// trueならGameが動いてる状態
    /// falseならGameが止まってる状態
    /// </summary>
    public bool isPlayGame
    {
        get{ return _gamePlay; }
        set { _gamePlay = value; }
    }

    public int displayTime
    {
        get; private set;
    }

    /// <summary>
    /// instanceの所得
    /// </summary>
    public static GameDirector instance
    {
        get; private set;
    } 

    /// <summary>
    /// 時間計測する関数
    /// </summary>
    //void PlayTimeCount()
    //{
    //    if (!_gamePlay) return;
    //    ScoreManager.instance.GameTimeCount();
    //}

    //////////////////////////////////////////////////////////////////////

    void Awake()
    {
        instance = this;
        _update = new Dictionary<GameState, Action>();
        _update.Add(GameState.MainGame,MainGameUpdate);
        _update.Add(GameState.Result,() => { });
        GameSet();
        _hp = FindObjectOfType<PlayerHP>();

        VoiceNumberStorage.setVoice();
    }

    private void Start()
    { 
        StartCoroutine(GameStartCutIn());
    }

    [SerializeField]
    int _clearDeathCount = 30;

    public int clearEnemyKillCount
    {
        get { return _clearDeathCount; }
    }

    private void MainGameUpdate()
    {
        //PlayTimeCount();

        if (_hp.PlayerHp <= 0 && _gamePlay)
        {
            _gamePlay = false;
            GameSet();
            StartCoroutine(ResultChangeStage(_hp.gameOverImage));
        }
        else if(_generateManager._deathCount == _clearDeathCount)
        {
            _clearDeathCount--;
            Instantiate(_boss);
        }
        else if(_isBossDestroy && _gamePlay)
        {
            _gamePlay = false;
            StartCoroutine(GameEndAudio());
            //GameSet();
            //StartCoroutine(ResultChangeStage(_gameClearImage));
        }

    }

    private IEnumerator GameEndAudio()
    {
        foreach(var gunSlowButtons in FindObjectsOfType<GunSlowButton>())
        {
            gunSlowButtons.enabled = false;
        }
        foreach(var overHeat in _playerShot)
        {
            overHeat.gameObject.GetComponent<OverHeat>().FinishProduction();
        }
        AudioManager.instance.playVoice(AudioName.VoiceName.IV15);
        var time = 0.0f;
        while(time < 9.5f)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        SceneChange.ChangeScene(SceneName.Name.Result, Color.white);
    }

    /// <summary>
    /// フェーズが変わるときに流すセリフ
    /// </summary>
    /// <returns></returns>
    private IEnumerator AudioPlay()
    {
        AudioManager.instance.playVoice(AudioName.VoiceName.IV02);
        while(_generateManager._deathCount < _voiceTiming)
        {
            yield return null;
        }
        AudioManager.instance.playVoice(AudioName.VoiceName.IV03);
        yield return null;
    }

    private IEnumerator GameStartCutIn()
    {
        AudioManager.instance.playVoice(AudioName.VoiceName.IV01);
        yield return new WaitForSeconds(5.0f);
        //_gameStartImage.gameObject.SetActive(true);
        var time = 0.0f;
        while(time < _gameStartTime)
        {
            time += Time.deltaTime;
            displayTime = (int)(_gameStartTime - time + 1);
            yield return null;
        }
        _gamePlay = true;
        GameSet();
        StartCoroutine(AudioPlay());
    }

    void Update()
    {
        _update[_state]();
    }

    private IEnumerator ResultChangeStage(Image activeImage)
    {
        var time = 0.0f;
        activeImage.gameObject.SetActive(true);
        while(RenderSettings.ambientIntensity > 0)
        {
            time += Time.unscaledDeltaTime;
            RenderSettings.ambientIntensity = Mathf.Lerp(1, 0, time / 2.0f);
            yield return null;
        }

        _state = GameState.Result;
        _generateManager.DestroyAllEnemy();
        SceneChange.ChangeScene(SceneName.Name.Title);
    }

    void GameSet()
    {
        for (int i = 0; i < _playerShot.Length; i++)
        {
            _playerShot[i].isStart = _gamePlay;
        }
        _generateManager.isTutorial = _gamePlay;
    }

}
