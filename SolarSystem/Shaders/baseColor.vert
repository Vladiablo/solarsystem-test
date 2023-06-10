#version 330 core

//#extension GL_ARB_gpu_shader_fp64 : enable
//#extension GL_ARB_vertex_attrib_64bit : enable

uniform mat4 mvp;

layout (location = 0) in vec3 aPos;

void main()
{
	gl_Position = vec4(aPos, 1.0);
}
