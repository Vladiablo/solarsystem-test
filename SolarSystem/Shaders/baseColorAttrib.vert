#version 330 core

//#extension GL_ARB_gpu_shader_fp64 : enable
//#extension GL_ARB_vertex_attrib_64bit : enable

uniform mat4 mvp;

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;

out vec4 vertexColor;

void main()
{
	vertexColor = aColor;

	gl_Position = mvp * vec4(aPos, 1.0);
}
