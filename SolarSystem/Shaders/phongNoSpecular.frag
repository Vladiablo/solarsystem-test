#version 330 core

uniform sampler2D baseColor;
uniform vec3 lightPos;
uniform vec3 lightColor;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

out vec4 FragColor;

void main()
{
	float ambientStrength = 0.25;
	vec3 ambient = lightColor * ambientStrength;

	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);

	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;

	vec3 baseColor = vec3(texture(baseColor, TexCoord));

	vec3 result = clamp(ambient + diffuse, 0.0, 1.0) * baseColor;

	FragColor = vec4(result, 1.0);
}
