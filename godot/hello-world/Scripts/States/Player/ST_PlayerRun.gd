extends State
class_name ST_PlayerRun

@export var player : Player

func Enter():
	player.vspeed = 0

func Physics_Update(delta):
	if !player.is_on_ground():
		Transitioned.emit(self, "ST_PlayerDrop")
	
	var requested_move_direction = player.get_requested_move_direction()
	if requested_move_direction != Vector3.ZERO:
		if(player.hspeed < player.get_current_max_ground_speed()):
			player.hspeed += player.base_acceleration * delta
		else:
			player.hspeed -= player.base_acceleration * delta
		player.rotation.y = rotate_toward(player.rotation.y, atan2(requested_move_direction.x, requested_move_direction.z), player.turn_speed * delta)
	else:
		Transitioned.emit(self, "ST_PlayerIdle")

	var moveVec = -(player.transform.basis.z) * player.hspeed
	#player.velocity = moveVec
	#player.move_and_slide()
	var collision = player.move_and_collide(moveVec * delta, true)
	if collision:
		if(collision.get_angle() >= player.floor_max_angle):
			var requestedEndPosition = player.global_position + moveVec * delta
			var collisionDelta = collision.get_normal() * collision.get_depth()
			#collisionDelta.y = 0
			collisionDelta *= 60
			var modifiedEndPosition = requestedEndPosition + collisionDelta
			player.global_position = modifiedEndPosition
			player.ground_snap()
			var drawPoints : PackedVector3Array
			drawPoints.append(modifiedEndPosition)
			#DebugDraw3D.draw_points(drawPoints,DebugDraw3D.POINT_TYPE_SPHERE, 0.02, Color.CRIMSON, 10)
		else:
			#move and snap to gruond
			player.global_position += moveVec * delta
			if player.is_on_ground():
				player.ground_snap()
			else:
				#switch to falling
				Transitioned.emit(self, "ST_PlayerDrop")
	else:
		player.global_position = player.global_position + moveVec * delta
		player.ground_snap()
	#if(moveCollision):
		#var noWallsHit = true
		#for n in moveCollision.get_collision_count():
			#if(moveCollision.get_angle(n) >= floor_max_angle):
				#noWallsHit = false
		#if noWallsHit:
			#global_position += moveVec * delta
			#
			#var space_state = get_world_3d().direct_space_state
			#var query = PhysicsRayQueryParameters3D.create(global_position + Vector3.UP, global_position - Vector3.UP * floor_snap_length, collision_mask)
			#var result = space_state.intersect_ray(query)
			#if(result):
				#global_position = result.position
			#else:
				#vspeed -= fall_acceleration * delta
				#global_position += Vector3.UP * vspeed
		#else:
			#velocity = moveVec
			#move_and_slide()
			#
	#else:
		#global_position += moveVec * delta
	#
	#print(is_on_floor())
	#if(!is_on_floor()):
		#
		#velocity = Vector3.UP * vspeed
		#move_and_slide()
