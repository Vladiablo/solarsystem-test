#version 330 core

uniform sampler2D baseColor;
uniform sampler2D nightColor;
uniform sampler2D specularMap;

uniform vec3 viewPos;
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform float shininess;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

out vec4 FragColor;

void main()
{
	const float ambientStrength = 0.05;

	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);

	float dotProduct = dot(normal, lightDir);

	float diff = max(dotProduct, 0.0);

	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 halfwayDir = normalize(lightDir + viewDir);

	vec3 baseColor = vec3(texture(baseColor, TexCoord));
	vec3 ambient = ambientStrength * baseColor;

	float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
	vec3 specular = (lightColor / 3.0) * spec * vec3(texture(specularMap, TexCoord));

	if (dotProduct < 0.0)
	{
		vec3 diffuse = -dotProduct * vec3(texture(nightColor, TexCoord));
		FragColor = vec4(ambient + diffuse + specular, 1.0);
		return;
	}


	vec3 diffuse = diff * baseColor;	

	FragColor = vec4(ambient + diffuse + specular, 1.0);
}
