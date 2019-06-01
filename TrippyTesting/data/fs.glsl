﻿#version 400

uniform sampler2D tex;

in vec4 fColor;
in vec2 fTexCoords;

out vec4 FragColor;

void main() {
    FragColor = fColor * texture(tex, fTexCoords);
	//FragColor = vec4(fTexCoords, 1.0, 1.0);
}