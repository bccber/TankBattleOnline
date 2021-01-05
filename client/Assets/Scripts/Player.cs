using Network;
using Protocol;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Sprite[] TankSprite; // 上右下左
    public GameObject BulletPrefab;
    public GameObject ExplonsionPrefab;
    public GameObject DefendEffectPrefab;
    public GameObject BronPrefab;

    private SpriteRenderer _Sr;
    private Vector3 _BulletEulerAngles;
    private float _TimeVal = 0;
    private float _DefendTimeVal = 3.5f;
    private GameObject _Bron;
    private bool _ShowBron = true;
    private float _MoveSpeed = 5;

    private void Awake()
    {
        _Sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (_ShowBron)
        {
            gameObject.SetActive(false);
            _Bron = Instantiate(BronPrefab, gameObject.transform.position, Quaternion.identity);
            _Bron.SetActive(true);
            Invoke("DeleteBron", 0.5f);
            DefendEffectPrefab.SetActive(true);
        }
        else
        {
            DefendEffectPrefab.SetActive(false);
        }
    }

    void Update()
    {
        if (_DefendTimeVal > 0)
        {
            _DefendTimeVal -= Time.deltaTime;
        }
        else
        {
            DefendEffectPrefab.SetActive(false);
        }

        Attack();
    }

    private void FixedUpdate()
    {
        TankMove();
    }

    private void TankMoveBroadCast(float x, float y, int direction)
    {
        PlayerMoveBroadCast move = new PlayerMoveBroadCast();
        move.Name = GameManager.Instance.PlayerName;
        move.RoomId = GameManager.Instance.RoomId;
        move.X = x;
        move.Y = y;
        move.Direction = direction;

        TCPClient.Instance.SendMessage<PlayerMoveBroadCast>(CMD.PlayerMoveBroadCast, move);
    }

    private void TankMove()
    {
        if (gameObject.name != GameManager.Instance.PlayerName)
        {
            return;
        }

        Int32 direction = 0;
        float v = Input.GetAxisRaw("Vertical");
        if (v != 0)
        {
            transform.Translate(Vector3.up * v * _MoveSpeed * Time.deltaTime, Space.World);
            if (v < 0)
            {
                _Sr.sprite = TankSprite[2];
                _BulletEulerAngles = new Vector3(0, 0, -180);
                direction = 2;
            }
            else if (v > 0)
            {
                _Sr.sprite = TankSprite[0];
                _BulletEulerAngles = new Vector3(0, 0, 0);
                direction = 0;
            }

            TankMoveBroadCast(transform.position.x, transform.position.y, direction);

            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            transform.Translate(Vector3.right * h * _MoveSpeed * Time.deltaTime, Space.World);
            if (h < 0)
            {
                _Sr.sprite = TankSprite[3];
                _BulletEulerAngles = new Vector3(0, 0, 90);
                direction = 3;
            }
            else if (h > 0)
            {
                _Sr.sprite = TankSprite[1];
                _BulletEulerAngles = new Vector3(0, 0, -90);
                direction = 1;
            }

            TankMoveBroadCast(transform.position.x, transform.position.y, direction);
        }
    }

    private void Attack()
    {
        if (gameObject.name != GameManager.Instance.PlayerName)
        {
            return;
        }

        if ((_TimeVal += Time.deltaTime) < 0.4f)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
            AttackBroadCast attack = new AttackBroadCast();
            attack.Name = GameManager.Instance.PlayerName;
            attack.RoomId = GameManager.Instance.RoomId;
            TCPClient.Instance.SendMessage<AttackBroadCast>(CMD.AttackBroadCast, attack);

            _TimeVal = 0;
        }
    }

    private void Die()
    {
        if (_DefendTimeVal > 0)
        {
            return;
        }

        if (gameObject.name != GameManager.Instance.PlayerName)
        {
            return;
        }

        Instantiate(ExplonsionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);

        GameManager.Instance.GameOver();
    }

    private void Fire()
    {
        Instantiate(BulletPrefab, transform.position, Quaternion.Euler(transform.eulerAngles + _BulletEulerAngles), transform.parent);
    }

    private void SetDirection(int direction)
    {
        switch (direction)
        {
            case 0:
                _Sr.sprite = TankSprite[0];
                _BulletEulerAngles = new Vector3(0, 0, 0);
                break;

            case 1:
                _Sr.sprite = TankSprite[1];
                _BulletEulerAngles = new Vector3(0, 0, -90);
                break;

            case 2:
                _Sr.sprite = TankSprite[2];
                _BulletEulerAngles = new Vector3(0, 0, -180);
                break;

            case 3:
                _Sr.sprite = TankSprite[3];
                _BulletEulerAngles = new Vector3(0, 0, 90);
                break;
        }
    }

    private void DeleteBron()
    {
        gameObject.SetActive(true);
        if (_Bron != null)
        {
            Destroy(_Bron);
        }
    }

    private void SetHideBron()
    {
        _ShowBron = false;
    }
}