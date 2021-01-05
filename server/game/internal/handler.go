package internal

import (
	"github.com/name5566/leaf/gate"
	"reflect"
	"server/msg"
)

func init() {
	// 玩家登录
	handler(&msg.LoginRequest{}, LoginRequestHandler)
	// 心跳请求
	handler(&msg.HeartbeatRequest{}, HeartbeatRequestHandler)
	// 玩家下线
	handler(&msg.OfflineBroadCast{}, OfflineBroadCastHandler)
	// 玩家移动广播
	handler(&msg.PlayerMoveBroadCast{}, PlayerMoveBroadCastHandler)
	// 攻击广播
	handler(&msg.AttackBroadCast{}, AttackBroadCastHandler)
}

func handler(m interface{}, h interface{}) {
	skeleton.RegisterChanRPC(reflect.TypeOf(m), h)
}

// 玩家登录 匹配玩家
func LoginRequestHandler(args []interface{}) {
	agent := args[1].(gate.Agent)
	GameManagerObj.Login(agent)
}

// 心跳请求
func HeartbeatRequestHandler(args []interface{}) {
	req := args[0].(*msg.HeartbeatRequest)
	GameManagerObj.Heartbeat(req)
}

// 玩家下线广播
func OfflineBroadCastHandler(args []interface{}) {
}

// 玩家移动广播
func PlayerMoveBroadCastHandler(args []interface{}) {
	req := args[0].(*msg.PlayerMoveBroadCast)
	GameManagerObj.PlayerMoveBroadCast(req)
}

// 攻击广播
func AttackBroadCastHandler(args []interface{}) {
	req := args[0].(*msg.AttackBroadCast)
	GameManagerObj.AttackBroadCast(req)
}
