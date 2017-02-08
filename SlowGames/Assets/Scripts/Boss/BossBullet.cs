﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 匠くんのやつのほぼコピペ (ボスの高速弾が当たった時にボイスをならすため)

public class BossBullet : MonoBehaviour {

    [SerializeField]
    float _bulletSpeed = 5;
    [SerializeField]
    float _rotateSpeed = 100;

    [SerializeField]
    GameObject _deathEffect;

    public Vector3 _targetDirection;
    bool _isBlow = false;

    GameObject _player;

    float _standbyTime = 2.0f;
    float _standbySpeed = 1.0f;

    float _chargeTime = 0.0f;
    public float chargeTime {
        set { _chargeTime = value - _standbyTime; }
    }

    bool _destroyFlag = true;

    float _destroyCount = 5.0f;

    [SerializeField]
    float _slowStartTime = 0.01f;

    [SerializeField]
    float _angleSpeed = 5.0f;

    [SerializeField]
    GameObject _chargeEffect;

    GameObject _particle;

    void Start()
    {
        _particle = Instantiate(_chargeEffect);
        _particle.transform.position = transform.position;
        _particle.transform.parent = transform;
    }

    void Update()
    {
        if (_isBlow)
        {
            return;
        }

        if (_standbyTime > 0.0f)
        {
            _standbyTime -= Time.unscaledDeltaTime;
            transform.position += transform.TransformDirection(Vector3.forward) * _standbySpeed * Time.unscaledDeltaTime;
        }
        else if (_chargeTime > 0.0f)
        {
            _chargeTime -= Time.unscaledDeltaTime;

            if (_chargeTime <= 0.0f)
            {
                // 高速弾を撃つのを伝える
                AudioManager.instance.stopAllVoice();
                AudioManager.instance.playVoice(AudioName.VoiceName.IV06);
            }
        }
        else
        {
            if (_particle != null)
            {
                _particle.transform.parent = null;
            }

            transform.parent = null;

            if(_slowStartTime > 0.0f)
            {
                _slowStartTime -= Time.unscaledDeltaTime;
            }

            // スローじゃなかったらスローにする
            if (!SlowMotion._instance.isSlow && _slowStartTime <= 0.0f)
            {
                SlowMotion._instance.GameSpeed(0.1f);
                SlowMotion._instance.isLimit = false;
            }

            // 前進
            transform.position += transform.TransformDirection(Vector3.forward) * _bulletSpeed * Time.deltaTime;

            transform.Rotate(0, 0, _angleSpeed);

            _destroyCount -= Time.unscaledDeltaTime;
            if (_destroyCount <= 0.0f)
            {
                SlowMotion._instance.ResetSpeed();
                SlowMotion._instance.isLimit = true;
                Destroy(this.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //敵キャラ自信にあたってもスルー Todo : 見栄えが悪かったら調整
        //  if (other.gameObject.tag == TagName.Enemy || other.gameObject.tag == TagName.Finish || other.gameObject.tag == TagName.EnemyBullet)
        //  {
        //      return;
        //  }

        //玉にあった時は弾かせて消す

        if(_chargeTime > 0.0f || _standbyTime > 0.0f || _slowStartTime > 0.0f) { return; }

        if (other.gameObject.tag == TagName.Bullet)
        {
            //エフェクト
            var effect = Instantiate(_deathEffect);
            effect.transform.position = transform.position;
            // 音
            // ボスの弾をはじいたとき成功ボイスを流す
            AudioManager.instance.stopAllVoice();
            AudioManager.instance.playVoice(AudioName.VoiceName.IV07);

            //判定消す
            _isBlow = true;
            this.GetComponent<Collider>().enabled = false;
            StartCoroutine(RandomBlow());

            // スローだったら解除
            if (SlowMotion._instance.isSlow)
            {
                SlowMotion._instance.ResetSpeed();
                SlowMotion._instance.isLimit = true;
            }


            //プレイヤーの弾を消す
            Destroy(other.gameObject);
            return;

        }
        else if (other.gameObject.tag == TagName.Player)
        {
            // ボスの弾が当たったときに失敗ボイスを流す
            AudioManager.instance.stopAllVoice();
            AudioManager.instance.playVoice(AudioName.VoiceName.IV08);

            // スローだったら解除
            if (SlowMotion._instance.isSlow)
            {
                SlowMotion._instance.ResetSpeed();
                SlowMotion._instance.isLimit = true;
            }

            Destroy(gameObject);
        }

        //ScoreManager.instance.AddFlipEnemyBulletCount();
        //プレイヤーに当たった時は、そのまま消す
        //Destroy(gameObject);
    }



    //ランダムに弾けます
    IEnumerator RandomBlow()
    {
        Vector3 randomDirec = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
        float deathTime = 1.0f;
        float acceraition = 2.0f;
        float unacceration = 3.0f;
        float firstTime = 300.0f;

        //初速
        transform.position += randomDirec * (_bulletSpeed + firstTime) * Time.deltaTime;

        //加速
        while (true)
        {

            transform.position += randomDirec * _bulletSpeed * acceraition * Time.deltaTime;
            acceraition -= Time.deltaTime * unacceration;
            deathTime -= Time.deltaTime;

            if (acceraition < 0)
            {
                break;
            }

            yield return null;
        }

        //Todo :消す時にパッと消えるのいくないかも
        //消す
        Destroy(this.gameObject);

    }

}
