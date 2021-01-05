package conf

import (
	"log"
	"time"
)

var (
	LogFlag = log.LstdFlags

	PendingWriteNum        = 2000
	MaxMsgLen       uint32 = 20480
	HTTPTimeout            = 10 * time.Second
	LenMsgLen              = 2
	LittleEndian           = true // 使用小端

	GoLen              = 10000
	TimerDispatcherLen = 10000
	AsynCallLen        = 10000
	ChanRPCLen         = 10000
)
