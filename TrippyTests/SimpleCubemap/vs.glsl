﻿#version 330 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;

out vec3 fPosition;

void main() {
	fPosition = vPosition;
	gl_Position = Projection * View * World * vec4(vPosition, 1.0);
}