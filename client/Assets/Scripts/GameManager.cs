using Google.Protobuf;
using Network;
using Protocol;
using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //用来装饰初始化地图所需物体的数组。
    //0.空白 1.墙 2.障碍 3.河流 4.草 5.老家 6.玩家  7.敌人1  8.敌人2
    public GameObject[] Item;

    public Canvas CanvasMatching;
    public InputField TxtIP;
    public InputField TxtPort;
    public Text TxtMatching;
    public Button BtnMatch;

    // 当前玩家名称
    public string PlayerName = "";
    // 当前玩家房间事情
    public int RoomId = 0;
    // 随机种子
    private int _Seed = 0;
    // 随机数
    private System.Random _R = new System.Random();

    // 消息队列
    private ConcurrentBag<Tuple<short, IMessage>> _MessageQueue = new ConcurrentBag<Tuple<short, IMessage>>();
    // 玩家列表
    private ConcurrentDictionary<string, GameObject> _PlayerDic = new ConcurrentDictionary<string, GameObject>();

    private bool _IsMatching = false;
    private float _MatchTime = 0;
    private float _MatchSeconds = 1;

    private void Awake()
    {
        Instance = this;

        ProtocolDecoder.Instance.OnDataArrival += OnDataArrival;

        CanvasMatching.gameObject.SetActive(true);
        TxtMatching.gameObject.SetActive(false);

        TxtIP.text = "127.0.0.1";
        TxtPort.text = "9527";
    }

    // 创建边界碰撞体
    private void CreateBoundsColliders()
    {
        Vector3 top = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height, 3f));
        Vector3 bottom = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, 0f, 3f));
        Vector3 left = Camera.main.ScreenToWorldPoint(new Vector3(0f, Screen.height / 2f, 3f));
        Vector3 right = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2f, 3f));

        float width = Vector3.Distance(left, right);
        float height = Vector3.Distance(bottom, top);

        CreateOnCollider("top", top, new Vector3(width, 0.05f, 0.05f));
        CreateOnCollider("bottom", bottom, new Vector3(width, 0.05f, 0.05f));
        CreateOnCollider("left", left, new Vector3(0.05f, height, 0.05f));
        CreateOnCollider("right", right, new Vector3(0.05f, height, 0.05f));
    }

    private void CreateOnCollider(string name, Vector3 offset, Vector3 size)
    {
        GameObject col = new GameObject(name);
        col.transform.position = Vector3.zero;
        col.transform.parent = this.transform;
        BoxCollider2D box = col.AddComponent<BoxCollider2D>();
        box.size = size;
        box.offset = offset;
        box.tag = "BoundsCollider";
    }

    private void Start()
    {
        CreateBoundsColliders();
    }

    // 匹配
    public void OnBtnMatchClick()
    {
        _MatchTime = 0;
        _MatchSeconds = 1;
        TxtMatching.text = $"匹配中 1";

        BtnMatch.interactable = false;
        TxtMatching.gameObject.SetActive(true);

        _IsMatching = true;  

        TCPClient.Instance.Connect(TxtIP.text, int.Parse(TxtPort.text));

        LoginRequest login = new LoginRequest();
        TCPClient.Instance.SendMessage<LoginRequest>(CMD.LoginRequest, login);
    }

    private void Update()
    {
        Matching();
        OnDataHandler();
    }

    private void Matching()
    {
        if (!_IsMatching)
        {
            return;
        }

        _MatchTime += Time.deltaTime;
        if (_MatchTime >= 1)
        {
            _MatchTime = 0;
            TxtMatching.text = $"匹配中 {_MatchSeconds++}";
        }
    }

    // 创建地图
    private void CreateMap()
    {
        // 心脏        
        CreateItem(5, 0, 16);

        // 心脏外墙
        CreateItem(1, 12, 16);
        CreateItem(1, 12, 15);
        CreateItem(1, 0, 15);
        CreateItem(1, 1, 15);
        CreateItem(1, 1, 16);

        // 地图        
        for (int x = 0; x < 23; x++)
        {
            for (int y = 0; y < 17; y++)
            {
                if ((y == 8 || y == 16) || (y == 15 && (x == 0 || x == 1 || x == 12)))
                {
                    continue;
                }

                // 随机 0-4
                int itemIdx = _R.Next(0, 5);
                if (itemIdx == 2 || itemIdx == 3)
                {
                    int rNum = _R.Next(0, 100);
                    if (rNum % 2 == 0 || rNum % 3 == 0)
                    {
                        itemIdx = _R.Next(0, 5);
                    }
                }
                CreateItem(itemIdx, x, y);
            }
        }
    }
    
    // 创建玩家
    private void CreatePlayer(Protocol.Player player1, Protocol.Player player2)
    {
        GameObject gPlayer1 = CreateItem(6, -2, -8);
        gPlayer1.name = player1.Name;
        _PlayerDic.TryAdd(gPlayer1.name, gPlayer1);

        Mathf.Clamp(10, 10, 10);

        GameObject gPlayer2 = CreateItem(6, 2, -8);
        gPlayer2.name = player2.Name;
        _PlayerDic.TryAdd(gPlayer2.name, gPlayer2);
    }

    // 创建敌人
    private void CreateEnemy()
    {
        int x = _R.Next(0, 23);
        int y = 8;
        int t = _R.Next(0, 10000);
        int itemType = t % 2 == 0 ? 7 : 8;

        GameObject gObj = CreateItem(itemType, x, y);
        gObj.name = $"enemy_{x}_{t}";
        gObj.SendMessage("SetSeed", t);
    }
    private GameObject CreateItem(int itemType, int x, int y)
    {
        if (itemType <= 0)
        {
            return null;
        }

        float newX = x > 11 ? -1 * (x - 11) : x;
        float newY = y > 8 ? -1 * (y - 8) : y;

        GameObject gObj = Instantiate(Item[itemType], new Vector3(newX, newY), Quaternion.identity, this.transform);
        return gObj;
    }

    // 处理匹配消息
    private void OnMatchCompleteBroadCast(MatchCompleteBroadCast msg)
    {
        _IsMatching = false;
        if (msg.MatchState != 1)
        {
            BtnMatch.interactable = true;
            TxtMatching.gameObject.SetActive(false);
            return;
        }

        CanvasMatching.gameObject.SetActive(false);
        _Seed = msg.Seed;
        _R = new System.Random(_Seed);

        PlayerName = msg.CurrentPlayerName;
        RoomId = msg.RoomId;

        // 创建玩家
        CreatePlayer(msg.Player1, msg.Player2);

        // 创建地图
        CreateMap();

        // 创建敌人
        InvokeRepeating("CreateEnemy", 1, 4);

        // 心跳
        InvokeRepeating("Heartbeat", 1, 3);
    }

    // 处理玩家离线消息
    private void OnOfflineBroadCast(OfflineBroadCast msg)
    {
        if (_PlayerDic.TryRemove(msg.Name, out var p) && p != null)
        {
            Destroy(p);
            if (p.name == PlayerName)
            {
                GameOver();
            }
        }
    }

    // 处理玩家移动消息
    private void OnPlayerMoveBroadCast(PlayerMoveBroadCast msg)
    {
        if (_PlayerDic.TryGetValue(msg.Name, out var p) && p != null)
        {
            p.transform.position = new Vector3(msg.X, msg.Y);
            p.SendMessage("SetDirection", msg.Direction);
        }
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public void GameOver()
    {
        CancelInvoke();
        TCPClient.Instance.Close();
        SceneManager.LoadScene(0);
    }

    // 处理玩家射击消息
    private void OnAttackBroadCast(AttackBroadCast msg)
    {
        if (_PlayerDic.TryGetValue(msg.Name, out var p) && p != null)
        {
            p.SendMessage("Fire");
        }
    }

    // 消息处理
    private void OnDataHandler()
    {
        while (_MessageQueue.TryTake(out var kv))
        {
            try
            {
                switch (kv.Item1)
                {
                    case CMD.MatchCompleteBroadCast:
                        OnMatchCompleteBroadCast(kv.Item2 as MatchCompleteBroadCast);
                        break;

                    case CMD.OfflineBroadCast:
                        OnOfflineBroadCast(kv.Item2 as OfflineBroadCast);
                        break;

                    case CMD.PlayerMoveBroadCast:
                        OnPlayerMoveBroadCast(kv.Item2 as PlayerMoveBroadCast);
                        break;

                    case CMD.AttackBroadCast:
                        OnAttackBroadCast(kv.Item2 as AttackBroadCast);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }

    // 消息回调
    private void OnDataArrival(short cmd, IMessage message)
    {
        _MessageQueue.Add(new Tuple<short, IMessage>(cmd, message));
    }

    // 心跳
    private void Heartbeat()
    {
        HeartbeatRequest req = new HeartbeatRequest();
        req.RoomId = RoomId;
        req.Name = PlayerName;

        TCPClient.Instance.SendMessage<HeartbeatRequest>(CMD.HeartbeatRequest, req);
    }
}
