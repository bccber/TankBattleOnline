package internal

import (
	"github.com/name5566/leaf/gate"
)

// 玩家
type Player struct {
	Name          string      // 玩家名称
	RoomId        int32       // 玩家当前所在房间号
	X             float32     // 坐标X
	Y             float32     // 坐标Y
	Direction     int32       // 方向 0上 1下 2左 3右
	HeartbeatTime int64       // 上次心跳时间
	Agent         *gate.Agent // 玩家的代理信息
}
