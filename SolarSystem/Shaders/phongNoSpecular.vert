#version 330 core

uniform mat4 mvp;
uniform mat4 model;
uniform mat3 normalMat;

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;

void main()
{
	TexCoord = aTexCoord;
	Normal = normalMat * aNormal;
	FragPos = vec3(model * vec4(aPos, 1.0));

	gl_Position = mvp * vec4(aPos, 1.0);
}
