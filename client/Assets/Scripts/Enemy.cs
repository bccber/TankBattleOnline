using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Sprite[] TankSprite; // 上右下左   
    public GameObject BulletPrefab;
    public GameObject ExplonsionPrefab;

    private float _MoveSpeed = 4.5f;
    private SpriteRenderer _Sr;
    private Vector3 _BulletEulerAngles;
    private int _V = -1;
    private int _H = 0;
    private System.Random _R = new System.Random();

    private void Awake()
    {
        _Sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 定时开火
        InvokeRepeating("Fire", 1, 1);

        // 定时改变方向
        InvokeRepeating("ChangeDirection", 4, 4);
    }

    private void FixedUpdate()
    {
        TankMove();
    }

    private void TankMove()
    {
        if (_V != 0)
        {
            transform.Translate(Vector3.up * _V * _MoveSpeed * Time.deltaTime, Space.World);
            if (_V < 0)
            {
                _Sr.sprite = TankSprite[2];
                _BulletEulerAngles = new Vector3(0, 0, -180);
            }
            else if (_V > 0)
            {
                _Sr.sprite = TankSprite[0];
                _BulletEulerAngles = new Vector3(0, 0);
            }

            return;
        }

        if (_H != 0)
        {
            transform.Translate(Vector3.right * _H * _MoveSpeed * Time.deltaTime, Space.World);
            if (_H < 0)
            {
                _Sr.sprite = TankSprite[3];
                _BulletEulerAngles = new Vector3(0, 0, 90);
            }
            else if (_H > 0)
            {
                _Sr.sprite = TankSprite[1];
                _BulletEulerAngles = new Vector3(0, 0, -90);
            }
        }
    }

    private void ChangeDirection()
    {
        int t = _R.Next(0, 8);
        if (t > 5)
        {
            _V = -1;
            _H = 0;
        }
        else if (t == 0)
        {
            _V = 1;
            _H = 0;
        }
        else if (t > 0 && t <= 2)
        {
            _H = -1;
            _V = 0;
        }
        else if (t > 2 && t <= 4)
        {
            _H = 1;
            _V = 0;
        }
    }

    // 开火
    private void Fire()
    {
        var t = _R.Next(10000);
        if (t % 2 == 0 || t % 3 == 0)
        {
            Instantiate(BulletPrefab, transform.position, Quaternion.Euler(transform.eulerAngles + _BulletEulerAngles), transform.parent);
        }
    }

    private void Die()
    {
        Instantiate(ExplonsionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void SetSeed(int seed)
    {
        _R = new System.Random(seed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag.ToLower();
        if (tag == "barrier" || tag == "enemy" || tag == "river")
        {
            ChangeDirection();
        }
    }
}