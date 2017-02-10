﻿using UnityEngine;
using System.Collections;

public class Shot : MonoBehaviour
{

    [SerializeField]
    float _bulletSpeed = 100.0f;

    [SerializeField]
    float destroytime = 5.0f;

    float time;

    Rigidbody _rigidbody;
    Vector3 _direction = Vector3.zero;

    AimAssist[] _aimAssist;

    public Vector3 direction
    {
        set { _direction = value; }
    }

    public void Start()
    {
        time = destroytime;
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        TimeOutdestroy();
        ShotTest();
    }


    public void ShotTest()
    {
        _rigidbody.velocity = _direction * Time.unscaledDeltaTime * SlowMotion._instance.RealSpeed() * _bulletSpeed;
    }

    public void OnCollisionEnter(Collision col) //子供のあたり判定のときも呼んでくれる
    {
        if (col.gameObject.tag == "Weapon" || col.gameObject.tag == "Bullet" || col.gameObject.tag == "Player" || col.gameObject.tag == "Boss") return;
        Destroy(gameObject);
    }

    void TimeOutdestroy()
    {
        time -= Time.unscaledDeltaTime;
        if(time<= 0)
        {
            Destroy(gameObject);
        }
    }
}
