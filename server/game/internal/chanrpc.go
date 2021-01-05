package internal

import (
	"github.com/name5566/leaf/gate"
)

func init() {
	skeleton.RegisterChanRPC("NewAgent", rpcNewAgent)
	skeleton.RegisterChanRPC("CloseAgent", rpcCloseAgent)
}

// 玩家连接
func rpcNewAgent(args []interface{}) {
}

// 玩家断线
func rpcCloseAgent(args []interface{}) {
	agent := args[0].(gate.Agent)
	if agent.UserData() == nil {
		return
	}

	name := agent.UserData().(string)
	if name == "" {
		return
	}

	// 删除玩家
	GameManagerObj.RemovePlayer(name)
}
