#version 330 core

uniform sampler2D baseColor;
uniform sampler2D specularMap;
uniform vec3 viewPos;
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

	float specularStrength = 0.5;
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 2);
	vec3 specular = lightColor * spec * vec3(texture(specularMap, TexCoord));

	vec3 baseColor = vec3(texture(baseColor, TexCoord));

	vec3 result = (ambient + diffuse + specular) * baseColor;

	FragColor = vec4(result, 1.0);
}
