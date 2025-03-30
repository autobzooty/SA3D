extends State
class_name ST_PlayerDrop

@export var player : Player
	
func Enter():
	print("starting drop")
	
func Exit():
	print("done dropping")

func Physics_Update(delta):
	var query = PhysicsRayQueryParameters3D.create(player.global_position + Vector3.UP, player.global_position - Vector3.UP * player.floor_snap_length, 1)
	var space_state = player.get_world_3d().direct_space_state
	var result = space_state.intersect_ray(query)
	if(result):
		player.vspeed = 0
		player.global_position = result.position
		if player.get_requested_move_magnitude() == 0:
			Transitioned.emit(self, "ST_PlayerIdle")
		else:
			Transitioned.emit(self, "ST_PlayerRun")
	else:
		player.vspeed -= player.fall_acceleration * delta
		var moveVec = -player.transform.basis.z * player.hspeed * delta
		moveVec += Vector3.UP * player.vspeed * delta
		player.global_position += moveVec
	#if(player.is_on_floor()):
		#if(player.get_requested_move_direction() == Vector3.ZERO):
			#Transitioned.emit(self, "ST_PlayerIdle")
		#else:
			#Transitioned.emit(self, "ST_PlayerRun")
	#player.vspeed -= player.fall_acceleration * delta
	
	var moveVec = -(player.transform.basis.z) * player.hspeed
	moveVec += Vector3.UP * player.vspeed
	player.velocity = moveVec
	player.move_and_slide()
