package msg

import (
	"github.com/name5566/leaf/network/protobuf"
)

var Processor = protobuf.NewProcessor()

func init() {
	Processor.SetByteOrder(true)
	Processor.Register(&LoginRequest{})
	Processor.Register(&MatchCompleteBroadCast{})
	Processor.Register(&HeartbeatRequest{})
	Processor.Register(&OfflineBroadCast{})
	Processor.Register(&PlayerMoveBroadCast{})
	Processor.Register(&AttackBroadCast{})
}
