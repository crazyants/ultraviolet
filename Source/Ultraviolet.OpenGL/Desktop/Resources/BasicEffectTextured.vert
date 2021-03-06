﻿#version 140

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform vec4 DiffuseColor;

in  vec4 uv_Position0;
in  vec2 uv_TextureCoordinate0;

out vec4 vColor;
out vec2 vTextureCoordinate;

void main()
{
	gl_Position        = uv_Position0 * World * View * Projection;
	vColor             = DiffuseColor;
	vTextureCoordinate = uv_TextureCoordinate0;
}