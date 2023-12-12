#version 330 core

uniform mat4 mvp;
uniform mat4 model;
uniform mat3 normalMat;

uniform vec3 viewPos;
uniform vec3 lightPos;

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;

out vec3 TangentViewPos;
out vec3 TangentLightPos;
out vec3 TangentFragPos;

void main()
{
	TexCoord = aTexCoord;
	vec3 Normal = normalMat * aNormal;
	FragPos = vec3(model * vec4(aPos, 1.0));

	vec3 T = normalize(vec3(model * vec4(aTangent, 0.0)));
	vec3 N = normalize(vec3(model * vec4(aNormal, 0.0)));
	T = normalize(T - dot(T, N) * N);
	vec3 B = cross(N, T);

	mat3 TBN = transpose(mat3(T, B, N));

	TangentViewPos = TBN * viewPos;
	TangentLightPos = TBN * lightPos;
	TangentFragPos = TBN * FragPos;

	gl_Position = mvp * vec4(aPos, 1.0);
}
