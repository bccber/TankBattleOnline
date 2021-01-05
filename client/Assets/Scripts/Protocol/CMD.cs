namespace Protocol
{
    public class CMD
    {
        // 登录消息号
        public const short LoginRequest = 0;

        // 匹配消息号
        public const short MatchCompleteBroadCast = 1;

        // 心跳消息号
        public const short HeartbeatRequest = 2;

        // 离线消息号
        public const short OfflineBroadCast = 3;

        // 玩家移动消息号
        public const short PlayerMoveBroadCast = 4;

        // 玩家攻击消息号
        public const short AttackBroadCast = 5;
    }
}