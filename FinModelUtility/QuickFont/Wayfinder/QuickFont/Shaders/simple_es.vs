#version 310 es
precision mediump float;

uniform mat4 proj_matrix;
uniform mat4 modelview_matrix;

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_tc;
layout(location = 2) in vec4 in_colour;

out vec2 tc;
out vec4 colour;

void main(void) {
	tc = in_tc;
	colour = in_colour;
	gl_Position = proj_matrix * modelview_matrix * vec4(in_position, 1.);
}
