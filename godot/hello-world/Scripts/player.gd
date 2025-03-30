extends CharacterBody3D
class_name Player

@export var base_ground_max_speed = 7
@export var base_acceleration = 9
@export var fall_acceleration = 75
@export var turn_speed = deg_to_rad(540)

var camera = null
var ground_check_ray_cast : RayCast3D = null

var hspeed = 0
var vspeed = 0

func _ready() -> void:
	camera = get_viewport().get_camera_3d()
	ground_check_ray_cast = get_node("GroundCheckRayCast")
	floor_max_angle = deg_to_rad(88)
	floor_snap_length = 0.5

func _physics_process(delta):
	pass

func get_requested_move_direction() -> Vector3:
	var requested_move_direction = Vector3.ZERO
	requested_move_direction.z += Input.get_axis("move_back", "move_forward")
	requested_move_direction.x += Input.get_axis("move_right", "move_left")
	requested_move_direction = requested_move_direction.rotated(Vector3.UP, camera.global_rotation.y)
	
	return requested_move_direction
	
func get_requested_move_magnitude() -> float:
	return get_requested_move_direction().length()
	
func get_current_max_ground_speed() -> float:
	return base_ground_max_speed * get_requested_move_magnitude()
	
func is_on_ground() -> bool:
	var query = PhysicsRayQueryParameters3D.create(global_position + Vector3.UP, global_position - Vector3.UP * floor_snap_length)
	var space_state = get_world_3d().direct_space_state
	var result = space_state.intersect_ray(query)
	if result:
		return true
	else:
		return false
		
func ground_snap():
	var query = PhysicsRayQueryParameters3D.create(global_position + Vector3.UP, global_position - Vector3.UP * floor_snap_length, collision_mask, [self])
	var space_state = get_world_3d().direct_space_state
	var result = space_state.intersect_ray(query)
	#print("old method: ")
	#print(result.position)
	#print("new method: ")
	#print(ground_check_ray_cast.get_collision_point())
	#ground_check_ray_cast.global_position = global_position
	#ground_check_ray_cast.target_position = global_position - Vector3.UP * 0.5
	if(ground_check_ray_cast.is_colliding()):
		ground_check_ray_cast.force_raycast_update()
		#print(ground_check_ray_cast.target_position)
		global_position = ground_check_ray_cast.get_collision_point()
