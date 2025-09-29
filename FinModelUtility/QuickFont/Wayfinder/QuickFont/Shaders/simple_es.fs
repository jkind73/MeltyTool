#version 310 es
precision mediump float;

uniform sampler2D tex_object;

out vec4 fragColor;

in vec2 tc;
in vec4 colour;

void main(void) {
	fragColor = texture(tex_object, tc) * vec4(colour);
}
