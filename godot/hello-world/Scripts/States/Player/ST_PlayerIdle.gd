extends State
class_name ST_PlayerIdle

@export var player : Player

func Enter():
	print("entering idle")
	player.hspeed = 0
	player.vspeed = 0
	
func Physics_Update(delta):
	if !player.is_on_ground():
		Transitioned.emit(self, "ST_PlayerDrop")
		
	if player.get_requested_move_magnitude() != 0:
			player.look_at(player.global_position - player.get_requested_move_direction())
			Transitioned.emit(self, "ST_PlayerRun")
