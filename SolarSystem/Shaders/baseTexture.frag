#version 330 core

uniform sampler2D diffuse;

in vec2 texCoord;

out vec4 FragColor;

void main()
{
	FragColor = texture(diffuse, texCoord);
}
