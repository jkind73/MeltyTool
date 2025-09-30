#version 460
precision highp float;

out vec4 fragColor;

in vec4 vertexColor0;

void main() {
  fragColor = vertexColor0;
}