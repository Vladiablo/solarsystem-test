#version 330 core

uniform mat4 projection;
uniform mat4 view;

layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

void main()
{
	TexCoords = aPos;
	gl_Position = projection * view * vec4(aPos, 1.0);
}
