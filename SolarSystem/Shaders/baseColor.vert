#version 330 core

#extension GL_ARB_gpu_shader_fp64 : enable
#extension GL_ARB_vertex_attrib_64bit : enable

uniform dmat4 mvp;

layout (location = 0) in dvec3 aPos;

void main()
{
	gl_Position = vec4(mvp * dvec4(aPos, 1.0));
}
