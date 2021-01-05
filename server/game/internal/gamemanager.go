package internal

import (
	"fmt"
	"github.com/name5566/leaf/gate"
	"math/rand"
	"sync"
	"sync/atomic"
	"server/msg"
	"time"
)

type GameManager struct {
	rooms       sync.Map //map[int32]*Room    // 房间列表
	players     sync.Map //map[string]*Player // 玩家列表
	maxPlayerId int32    // 当前最大玩家号
	maxRoomId   int32    // 当前最大房间号
}

//提供一个对外的世界管理模块句柄
var GameManagerObj *GameManager

func init() {
	GameManagerObj = &GameManager{
		maxPlayerId: 0,
		maxRoomId:   0,
	}

	// 启动检查心跳协和
	go GameManagerObj.checkHeartbeat()
}

// 心跳检查，3秒一次
func (gm *GameManager) checkHeartbeat() {
	for {
		time.Sleep(3 * time.Second)

		gm.players.Range(func(key, value interface{}) bool {
			p := value.(*Player)

			second := time.Now().Unix() - p.HeartbeatTime
			if p.RoomId <= 0 && second > 60 {
				// 删除，并发送匹配失败消息
				gm.players.Delete(key)
				m := &msg.MatchCompleteBroadCast{MatchState: 0}
				(*p.Agent).WriteMsg(m)

			} else if p.RoomId > 0 && second > 10 {
				// 删除，并发送删除玩家消息
				gm.RemovePlayer(p.Name)
			}

			return true
		})
	}
}

// 通过roomId,找到房间
func (gm *GameManager) getRoomById(roomId int32) (room *Room, b bool) {
	v, b := GameManagerObj.rooms.Load(roomId)
	if !b {
		return nil, b
	}

	room = v.(*Room)

	return room, b
}

// 登录，匹配玩家
func (gm *GameManager) Login(agent gate.Agent) {
	// 把玩家增加到玩家列表
	atomic.AddInt32(&gm.maxPlayerId, 1)
	name := fmt.Sprintf("player_%d", gm.maxPlayerId)
	agent.SetUserData(name)

	p := &Player{
		Name:          name,
		RoomId:        0,
		HeartbeatTime: time.Now().Unix(),
		Agent:         &agent,
	}
	gm.players.Store(p.Name, p)

	// 从玩家列表里匹配
	pList := make([]*Player, 0)
	gm.players.Range(func(key, value interface{}) bool {
		p := value.(*Player)
		if p.RoomId > 0 {
			return true
		}

		pList = append(pList, p)
		if len(pList) >= 2 {
			return false
		}

		return true
	})

	// 把玩家增加到房间
	if len(pList) < 2 {
		return
	}

	// 创建房间
	atomic.AddInt32(&gm.maxRoomId, 1)
	rand.Seed(time.Now().UnixNano())
	room := &Room{
		RoomId: gm.maxRoomId,
		Seed:   rand.Int31n(99999999),
	}
	gm.rooms.Store(room.RoomId, room)

	// 把匹配好的玩家增加到房间
	room.AddPlayers(pList)
}

// 心跳
func (gm *GameManager) Heartbeat(info *msg.HeartbeatRequest) {
	// 从玩家列表找到玩家
	v, b := gm.players.Load(info.Name)
	if !b {
		return
	}
	v.(*Player).HeartbeatTime = time.Now().Unix()
}

// 删除玩家
func (gm *GameManager) RemovePlayer(name string) {
	// 从用户列表删除
	v1, b := gm.players.Load(name)
	if !b {
		return
	}
	gm.players.Delete(name)
	player := v1.(*Player)

	// 找到用户所在房间
	room, b := gm.getRoomById(player.RoomId)
	if !b {
		return
	}
	room.RemovePlayer(player)

	// 删除房间
	if room.Player1 == nil && room.Player2 == nil {
		gm.rooms.Delete(room.RoomId)
	}
}

// 玩家移动广播
func (gm *GameManager) PlayerMoveBroadCast(info *msg.PlayerMoveBroadCast) {
	if room, b := gm.getRoomById(info.RoomId); b {
		room.PlayerMoveBroadCast(info)
	}
}

// 广播攻击
func (gm *GameManager) AttackBroadCast(info *msg.AttackBroadCast) {
	if room, b := gm.getRoomById(info.RoomId); b {
		room.AttackBroadCast(info)
	}
}
