package internal

import (
	"server/msg"
)

type Room struct {
	RoomId  int32   // 房间号
	Seed    int32   // 随机种子
	Player1 *Player // 每个房间只能有两个玩家
	Player2 *Player
}

// 广播消息
func (r *Room) broadCast(msg interface{}, filterName string) {
	// 通知玩家1
	if r.Player1 != nil && r.Player1.Name != filterName {
		(*r.Player1.Agent).WriteMsg(msg)
	}

	// 通知玩家2
	if r.Player2 != nil && r.Player2.Name != filterName {
		(*r.Player2.Agent).WriteMsg(msg)
	}
}

// 增加玩家到房间
func (r *Room) AddPlayers(players []*Player) {
	// 设置玩家房间号
	r.Player1 = players[0]
	r.Player1.RoomId = r.RoomId

	r.Player2 = players[1]
	r.Player2.RoomId = r.RoomId

	m := &msg.MatchCompleteBroadCast{
		MatchState: 1,
		Seed:       r.Seed,
		RoomId:     r.RoomId,
		Player1: &msg.Player{
			Name:      r.Player1.Name,
			RoomId:    r.Player1.RoomId,
			X:         r.Player1.X,
			Y:         r.Player1.Y,
			Direction: r.Player1.Direction,
		},
		Player2: &msg.Player{
			Name:      r.Player2.Name,
			RoomId:    r.Player2.RoomId,
			X:         r.Player2.X,
			Y:         r.Player2.Y,
			Direction: r.Player2.Direction,
		},
	}
	// 通知玩家1
	m.CurrentPlayerName = r.Player1.Name
	(*r.Player1.Agent).WriteMsg(m)

	// 通知玩家2
	m.CurrentPlayerName = r.Player2.Name
	(*r.Player2.Agent).WriteMsg(m)
}

// 玩家移动广播
func (r *Room) PlayerMoveBroadCast(info *msg.PlayerMoveBroadCast) {
	var p *Player
	if r.Player1 != nil && r.Player1.Name == info.Name {
		p = r.Player1
	} else if r.Player2 != nil && r.Player2.Name == info.Name {
		p = r.Player2
	}

	// 修改自己的数据
	if p != nil {
		p.X = info.X
		p.Y = info.Y
		p.Direction = info.Direction
	}

	r.broadCast(info, p.Name)
}

// 广播攻击
func (r *Room) AttackBroadCast(info *msg.AttackBroadCast) {
	r.broadCast(info, info.Name)
}

// 删除玩家
func (r *Room) RemovePlayer(player *Player) {
	// 广播通知另外一个玩家
	bc := &msg.OfflineBroadCast{
		RoomId: r.RoomId,
		Name:   player.Name,
	}
	r.broadCast(bc, "")

	// 从房间删除
	if r.Player1 != nil && r.Player1.Name == player.Name {
		(*r.Player1.Agent).Close()
		r.Player1 = nil
	} else if r.Player2 != nil && r.Player2.Name == player.Name {
		(*r.Player2.Agent).Close()
		r.Player2 = nil
	}
}
