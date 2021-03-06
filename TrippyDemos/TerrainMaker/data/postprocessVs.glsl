﻿#version 330 core

uniform mat4 Projection;

in vec3 vPosition;
in vec4 vColor;
in vec2 vTexCoords;

out vec4 fColor;
out vec2 fTexCoords;

void main() {
	gl_Position = Projection * vec4(vPosition, 1.0);
	fColor = vColor;
	fTexCoords = vTexCoords;
}