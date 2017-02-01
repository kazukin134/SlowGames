﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{

    public enum State
    {
        GunPut,
        End,
        Wait
    }
    private Dictionary<State, Action> _stateUpdate = null;
    private State _state = State.Wait;

    [SerializeField]
    private PutGunStand[] _put = null;

    [SerializeField]
    private Image[] _logo = null;

    [SerializeField]
    private TextMessage _thankyouText = null;

    [SerializeField, Range(2.0f, 6.0f)]
    private float _logoMoveEndTime = 2.0f;

    [SerializeField]
    private GameObject[] _desk = null;

    [SerializeField]
    private GameObject[] _stand = null;

    [SerializeField]
    private GameObject[] _gun = null;

    [SerializeField]
    private float _deskMoveSpeed = 0.0f;

    [SerializeField]
    private GameObject[] _deskSpotLight = null;

    private Material[] _gunMaterial = null;

    void Start()
    {
        _stateUpdate = new Dictionary<State, Action>();
        _stateUpdate.Add(State.GunPut, GunPutUpdate);
        _stateUpdate.Add(State.Wait, () => { });
        _stateUpdate.Add(State.End, EndUpdate);

        _gunMaterial = new Material[2];
        for (int i = 0; i < _gun.Length; i++)
        {
            _gunMaterial[i] = _gun[i].GetComponent<Renderer>().material;
        }

        VoiceNumberStorage.setVoice();
        AudioManager.instance.playVoice(AudioName.VoiceName.IV16);
        StartCoroutine(AudioMessage());
    }

    private IEnumerator AudioMessage()
    {
        var time = 0.0f;
        while (time < 4.0f)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        //_desk[0].SetActive(true);
        //_desk[1].SetActive(true);


        for (int i = 0; i < _desk.Length; i++)
        {
            iTween.ScaleTo(_desk[i], iTween.Hash(
                "y", 0.05f,
                "time", 1.0f,
                "easeType", iTween.EaseType.easeOutCubic
                ));
        }

        time = 0.0f;
        while (time < 1.0f)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        _stand[0].SetActive(true);
        _stand[1].SetActive(true);
        _deskSpotLight[0].SetActive(true);
        _deskSpotLight[1].SetActive(true);

        _state = State.GunPut;
    }

    void Update()
    {
        //デバッグ用:台座に置いてないときでも戻れるように
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            SceneChange.ChangeScene(SceneName.Name.Title, Color.white);
        }

        //デバッグ用:台座に置いてないときでも戻れるように
        if (Input.GetKeyDown(KeyCode.V))
        {
            _put[0].Test(_stand[0]);
            _put[1].Test(_stand[1]);
        }

        _stateUpdate[_state]();
    }

    /// <summary>
    /// 銃を置くまでのUpdate
    /// </summary>
    void GunPutUpdate()
    {
        if (!_put[0].isPutGun) return;
        if (!_put[1].isPutGun) return;

        _stand[0].SetActive(false);
        _stand[1].SetActive(false);


        //for(int i = 0; i < _desk.Length; i++)
        //{
        //    _gun[i].transform.parent = _desk[i].transform;
        //}

        _state = State.Wait;
        StartCoroutine(Production());
    }

    [SerializeField]
    private GameObject[] _particle = null;

    /// <summary>
    /// 最後の演出
    /// </summary>
    /// <returns></returns>
    private IEnumerator Production()
    {
        AudioManager.instance.playVoice(AudioName.VoiceName.IV17);
        var time = 0.0f;
        var fadeTime = 0.0f;

        //銃が光る演出
        while (time < 2.0f)
        {
            time += Time.unscaledDeltaTime;
            for (int i = 0; i < _gunMaterial.Length; i++)
            {
                //var emission = Mathf.Lerp(0, 4, time / 2.0f);
                var emission = (float)Easing.OutCubic(time, 2.0f, 4, 0);
                //var emission = (float)Easing.OutQuint(time, 2.0f, 4, 0);
                var color = new Color(emission, emission, emission);
                _gunMaterial[i].EnableKeyword("_EMISSION");
                _gunMaterial[i].SetColor("_EmissionColor", color);
            }
            yield return null;
        }
        _gun[0].GetComponent<Rigidbody>().useGravity = false;
        _gun[1].GetComponent<Rigidbody>().useGravity = false;
        //台座が消える演出
        for (int i = 0; i < _desk.Length; i++)
        {
            iTween.ScaleTo(_desk[i], iTween.Hash(
                "y", 0.0f,
                "time", 1.0f,
                "easeType", iTween.EaseType.easeOutCubic
                ));
        }

        for (int i = 0; i < _gun.Length; i++)
        {
            iTween.RotateTo(_gun[i], iTween.Hash(
                "x", 0.0f,
                "time", 3.0f,
                "easeType", iTween.EaseType.linear
                ));
        }

        time = 0.0f;
        //音声終わるの待つ
        while (time < 5.5f)
        {
            time += Time.unscaledDeltaTime;
            if (time > 3.0f)
            {
                fadeTime += Time.unscaledDeltaTime;
                for (int i = 0; i < _gun.Length; i++)
                {
                    if (!_particle[i].activeSelf)
                    {
                        iTween.RotateTo(_gun[i], iTween.Hash(
                            "z", 330.0f,
                            "time", 2.5f,
                            "easeType", iTween.EaseType.linear
                            ));

                        iTween.MoveTo(_gun[i], iTween.Hash(
                            "y", 1.0f,
                            "time", 2.5f,
                            "easeType", iTween.EaseType.linear
                            ));

                        _particle[i].SetActive(true); //ぽわぽわ出る

                        //銃のマテリアルの設定
                        var mat = _gun[i].GetComponent<Renderer>().material;

                        mat.SetFloat("_Mode", 2);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;

                        _gun[i].GetComponent<Renderer>().material = mat;
                    }
                    var a = (float)Easing.Linear(fadeTime, 5.5f - 3.0f, 0, 1);
                    _gunMaterial[i].color = new Color(_gunMaterial[i].color.r, _gunMaterial[i].color.g, _gunMaterial[i].color.b, a); //fadeしていく
                }
            }
            yield return null;
        }

        for (int i = 0; i < _desk.Length; i++)
        {
            _desk[i].SetActive(false);
            _gun[i].SetActive(false);
        }

        yield return new WaitForSecondsRealtime(1.0f);

        AudioManager.instance.playSe(AudioName.SeName.TitleLogoEnding);

        //thank you for playing演出
        _thankyouText.isMoveText = true;
        while (!_thankyouText.isPopText)
        {
            yield return null;
        }

        time = 0.0f;
        while(time < 1.0f)
        {
            time += Time.unscaledDeltaTime;
            var a = (float)Easing.InOutQuad(time, 1.0f, -1.0f, 1.0f);
            _thankyouText.setColor(new Color(_thankyouText.text.color.r, _thankyouText.text.color.g, _thankyouText.text.color.b, a));
            yield return null;
        }

        _thankyouText.gameObject.SetActive(false);

        //Logo演出

        //logoを出す
        time = 0.0f;
        while (time < _logoMoveEndTime)
        {
            time += Time.unscaledDeltaTime;
            _logo[0].fillAmount = (float)Easing.InOutQuad(time, _logoMoveEndTime, 1.0f * 2, 0.0f);
            yield return null;
        }        

        //logoの円の演出
        time = 0.0f;
        while(time < 1.0f)
        {
            time += Time.unscaledDeltaTime;
            _logo[1].fillAmount = (float)Easing.InOutQuad(time, 1.0f, 1.0f * 2, 0.0f);
            yield return null;
        }

        //logoの線の演出
        time = 0.0f;
        //while(time < 0.5f)
        //{
        //    time += Time.unscaledDeltaTime;
        //    _logo[2].fillAmount = Mathf.Lerp(_logo[2].fillAmount, 0.446f, time / 0.5f);
        //    yield return null;
        //}
        while (time < 1.0f)
        {
            time += Time.unscaledDeltaTime;
            var a = (float)Easing.InOutQuad(time, 1.0f, 1.0f * 2, 0.0f);
            _logo[2].color = new Color(_logo[2].color.r, _logo[2].color.g, _logo[2].color.b, a);
            yield return null;
        }

        //_logo[2].fillAmount = 0.547f;
        //yield return null;

        //time = 0.0f;
        //while (time < 0.5f)
        //{
        //    time += Time.unscaledDeltaTime;
        //    _logo[2].fillAmount = Mathf.Lerp(_logo[2].fillAmount, 1.0f, time / 0.5f);
        //    yield return null;
        //}

        //ざらざら演出
        NoiseSwitch.instance.noise.enabled = true;
        NoiseSwitch.instance.bloom.enabled = false;

        time = 0.0f;
        while (time < _logoMoveEndTime)
        {
            time += Time.unscaledDeltaTime;
            NoiseSwitch.instance.noise.intensityMultiplier = (float)Easing.OutCubic(time, _logoMoveEndTime * 2, 10.0f, 0.0f);
            yield return null;
        }
        yield return null;
        _state = State.End;
    }

    /// <summary>
    /// 演出が終わった後、タイトルに戻れるようにする処理
    /// </summary>
    void EndUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneChange.ChangeScene(SceneName.Name.Title, Color.white);
        }
    }


}
