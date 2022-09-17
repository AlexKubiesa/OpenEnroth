#version 410 core

in vec4 vertexColour;
in vec2 texuv;
flat in vec2 olayer;
in vec3 vsPos;
in vec3 vsNorm;
flat in int vsAttrib;
in vec4 viewspace;

out vec4 FragColour;

struct Sunlight {
	vec3 direction;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight {

    float type;  // 0- off, 1- stat, 2 - mob

    vec3 position;
    float radius;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct FogParam {
    vec3 color;
    float fogstart;
    float fogmiddle;
    float fogend;
};

uniform int waterframe;
uniform Sunlight sun;
uniform vec3 CameraPos;
uniform int flowtimer;
uniform int watertiles;

#define num_point_lights 20
uniform PointLight fspointlights[num_point_lights];

uniform sampler2DArray textureArray0;
uniform FogParam fog;

// funcs
vec3 CalcSunLight(Sunlight light, vec3 normal, vec3 viewDir, vec3 thisfragcol);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
float getFogRatio(FogParam fogpar, float dist);

void main() {

	vec3 fragnorm = normalize(vsNorm);
    vec3 fragviewdir = normalize(CameraPos - vsPos);

	vec4 fragcol = vec4(1.0);
    vec2 texcoords = vec2(1.0);
    vec2 texuvmod = vec2(0.0);
    vec2 deltas = vec2(0.0);

    // texture flow mods
    if (abs(vsNorm.z) >= 0.9) {
        if ((vsAttrib & 0x400) > 0) texuvmod.y = 1;
        if ((vsAttrib & 0x800) > 0) texuvmod.y = -1;
    } else {
        if ((vsAttrib & 0x400) > 0) texuvmod.y = -1;
        if ((vsAttrib & 0x800) > 0) texuvmod.y = 1;
    }

    if ((vsAttrib & 0x1000) > 0) {
        texuvmod.x = -1;    
    } else if ((vsAttrib & 0x2000) > 0) {
        texuvmod.x = 1;    
    }

    deltas.x = texuvmod.x * (flowtimer & int(textureSize(textureArray0,0).x-1));
    deltas.y = texuvmod.y * (flowtimer & int(textureSize(textureArray0,0).y-1));
    texcoords.x = (deltas.x + texuv.x) / textureSize(textureArray0,0).x;
    texcoords.y = (deltas.y + texuv.y) / textureSize(textureArray0,0).y; 
    fragcol = texture(textureArray0, vec3(texcoords.x,texcoords.y,olayer.y));

    vec4 toplayer = texture(textureArray0, vec3(texcoords.x,texcoords.y,0));
    vec4 watercol = texture(textureArray0, vec3(texcoords.x,texcoords.y,waterframe));

    if ((watertiles == 1) && (olayer.y == 0)){
        if ((vsAttrib & 0x3C00) != 0){ // water anim disabled
            fragcol = toplayer;
        } else {
            fragcol = watercol;
        }
    }

    // sunlight
	vec3 result = CalcSunLight(sun, fragnorm, fragviewdir, vec3(1)); //fragcol.rgb);
    result = clamp(result, 0.0, 0.85);

    result += CalcPointLight(fspointlights[0], fragnorm, vsPos, fragviewdir);

    // stack stationary
    for(int i = 1; i < num_point_lights; i++) {
        if (fspointlights[i].type == 1)
            result += CalcPointLight(fspointlights[i], fragnorm, vsPos, fragviewdir);
    }

    result *= fragcol.rgb;

    // stack mobile

    for(int i = 1; i < num_point_lights; i++) {
        if (fspointlights[i].type == 2)
            result += CalcPointLight(fspointlights[i], fragnorm, vsPos, fragviewdir);
    }

    vec3 clamps = result; // fragcol.rgb *  // clamp(result,0,1) * ; 

    vec3 dull;

    // percpetion red fade
    if ((vsAttrib & 0x10000) > 0) {
        float ss = (sin(flowtimer/30.0)+ 1.0) / 2.0;
        dull = vec3(1, ss, ss);
    } else {
        dull = vec3(1,1,1);
    }

    if (fog.fogstart == fog.fogend) {
        FragColour = vec4(clamps, vertexColour.a);
        return;
    }

    float dist = abs(viewspace.z / viewspace.w);
    float alpha = 0.0;

    if (fog.fogmiddle > fog.fogstart) {
        if (fog.fogmiddle / 2.0 > dist) {
            alpha = 1.0;
        } else {
            alpha = (fog.fogmiddle - dist) / (fog.fogmiddle / 2.0);
        }
    }

	vec4 inter = vec4(clamps, vertexColour.a) * vec4(dull,1); // result, 1.0);
	float fograt = getFogRatio(fog, abs(viewspace.z/ viewspace.w));
	FragColour = mix(inter, vec4(fog.color, alpha), fograt);

}

float getFogRatio(FogParam fogpar, float dist) {
    float result = 0.0;
    if (fogpar.fogstart < fogpar.fogmiddle) {
        result = smoothstep(fogpar.fogstart, fogpar.fogmiddle, dist);
    } else {
        result = smoothstep(fogpar.fogstart, fogpar.fogend, dist);
    }
    return result;
}

// calculates the color when using a directional light.
vec3 CalcSunLight(Sunlight light, vec3 normal, vec3 viewDir, vec3 thisfragcol) {
    vec3 lightDir = normalize(light.direction);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), 128.0 ); //material.shininess
    // combine results
    vec3 ambient = light.ambient * thisfragcol;
    vec3 diffuse = light.diffuse * diff * thisfragcol;
    vec3 specular = light.specular * spec * thisfragcol;  // should be spec map

	vec3 clamped = clamp((light.ambient + (light.diffuse * diff) + (light.specular * spec)), 0, 1) * thisfragcol;

    return clamped; //(ambient + diffuse + specular);
}

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {
    if (light.diffuse.r == 0 && light.diffuse.g == 0 && light.diffuse.b == 0) return vec3(0);    

    vec3 lightDir = normalize(light.position - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 128); //material.shininess
    // attenuation
    float distance = length(light.position - fragPos);

    //float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    float attenuation = clamp(1.0 - ((distance * distance)/(light.radius * light.radius)), 0.0, 1.0);
    attenuation *= attenuation;



    // combine results
    vec3 ambient = light.ambient ;//* vec3(texture(material.diffuse, TexCoords));
    vec3 diffuse = light.diffuse * diff ;//* vec3(texture(material.diffuse, TexCoords));
    vec3 specular = light.specular * spec ;//* vec3(texture(material.specular, TexCoords));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
} 