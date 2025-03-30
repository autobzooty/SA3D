extends SpringArm3D

@export var camera_rotate_speed = deg_to_rad(360)
@export var camera_zoom_speed = 24
@export var camera_track_speed = 10

var player
var target_position
var target_yaw
var target_pitch
var target_spring_length

func _ready() -> void:
	player = get_parent().get_node("PlayerCharacter")
	target_position = player.global_position
	target_yaw = rotation.y
	target_pitch = rotation.x
	target_spring_length = spring_length

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("camera_right"):
		target_yaw += PI/4
		target_yaw = wrapf(target_yaw, 0.0, TAU)
	if event.is_action_pressed("camera_left"):
		target_yaw -= PI/4
		target_yaw = wrapf(target_yaw, 0.0, TAU)
	if event.is_action_pressed("camera_in"):
		target_pitch = deg_to_rad(-20)
		target_spring_length = 3
	if event.is_action_pressed("camera_out"):
		target_pitch = deg_to_rad(-40)
		target_spring_length = 6

func _physics_process(delta):
	target_position = player.global_position - player.get_global_transform().basis.z * 0.5
	target_position.y += 0.75
	#global_position = target_position
	global_position = global_position.move_toward(target_position, camera_track_speed * delta)
	rotation.y = rotate_toward(rotation.y, target_yaw, camera_rotate_speed * delta)
	rotation.x = rotate_toward(rotation.x, target_pitch, camera_rotate_speed * delta)
	spring_length = move_toward(spring_length, target_spring_length, camera_zoom_speed * delta)
