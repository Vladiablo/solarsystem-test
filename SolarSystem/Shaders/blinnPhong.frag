#version 330 core

uniform sampler2D baseColor;
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
	const float ambientStrength = 0.25;

	vec3 baseColor = texture(baseColor, TexCoord).rgb;
	vec3 ambient = baseColor * ambientStrength;

	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 halfwayDir = normalize(lightDir + viewDir);

	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * baseColor;

	float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
	vec3 specular = lightColor * spec;

	FragColor = vec4(ambient + diffuse + specular, 1.0);
}
