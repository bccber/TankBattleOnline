package gate

import (
	"server/game"
	"server/msg"
)

func init() {
	msg.Processor.SetRouter(&msg.LoginRequest{}, game.ChanRPC)
	msg.Processor.SetRouter(&msg.HeartbeatRequest{}, game.ChanRPC)
	msg.Processor.SetRouter(&msg.OfflineBroadCast{}, game.ChanRPC)
	msg.Processor.SetRouter(&msg.PlayerMoveBroadCast{}, game.ChanRPC)
	msg.Processor.SetRouter(&msg.AttackBroadCast{}, game.ChanRPC)
}
