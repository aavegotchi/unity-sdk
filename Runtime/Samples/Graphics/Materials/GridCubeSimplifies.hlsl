// Hash function to generate a pseudo-random value based on the grid position
float Hash(float2 p) {
    float h = dot(p, float2(127.1,311.7));    
    return frac(sin(h) * 43758.5453123);
}

// Main function to create the cube grid and assign a color that changes over time
void CreateCubeGrid_float(float2 uv, float Time, float cubeSize, float gridSpacing, out float4 Color)
{
    // Parameters for cube size and spacing
    //const float cubeSize = 0.1; // Size of each cube
    //const float gridSpacing = 0.05; // Spacing between the cubes

    // Calculate grid position
    float2 gridPosition = floor((uv - 0.5) / (cubeSize + gridSpacing));

    // Generate a random value based on the grid position
    float randValue = Hash(gridPosition);

    // Use the time and the random value to oscillate the color between black and white
    float colorValue = randValue * (sin(Time * (randValue * 6.28318530718)) * 0.5 + 0.5);

    // Determine if we're inside a cube or in the spacing
    float2 gridUV = frac((uv - 0.5) / (cubeSize + gridSpacing));
    float insideCube = step(gridSpacing / (cubeSize + gridSpacing), gridUV.x) * step(gridSpacing / (cubeSize + gridSpacing), gridUV.y);

    // Assign color based on whether we are inside a cube or not
    Color = lerp(float4(0, 0, 0, 1), float4(colorValue, colorValue, colorValue, 1), insideCube);
    
}

float SmoothEdge(float dist, float blurAmount)
{
    
    return saturate(1.0 - (dist / blurAmount));

}

float SmoothStep(float edge0, float edge1, float x)
{
    float t = saturate((x - edge0) / (edge1 - edge0));
    return t * t * (3.0 - 2.0 * t);
}

float RoundedRect(float2 uv, float2 size, float radius, float blurAmount)
{
    float2 dist = abs(uv - 0.5) * 2.0 - size;
    dist = max(dist, 0.0);

    float cornerDist = length(dist) - radius;
    return 1.0 - SmoothStep(0.0, blurAmount, cornerDist);
}

void Smooth_float(float2 uv, float blurAmount, float2 size, float radius, float alpha, out float4 Out)
{
   // float blurAmount = 0.02;  // Modifica questo valore per regolare l'intensità dello sfocatura.
   // float2 size = float2(0.4, 0.4);  // Dimensioni del rettangolo.
   // float radius = 0.1;  // Raggio degli angoli arrotondati.

    float mask = RoundedRect(uv, size, radius, blurAmount);

    Out = float4(mask, mask, mask, alpha);  // Uscita come maschera in bianco e nero.
}

float4 Grid(float2 uv, float size, float lineWidth, float4 gridColor, float4 bgColor)
{
    float _line = fmod(uv.x * size, 1.0) < lineWidth || fmod(uv.y * size, 1.0) < lineWidth ? 1.0 : 0.0;
    return lerp(bgColor, gridColor, _line);
}

void DoGrid_float(float2 uv, float size, float lineWidth, float4 gridColor, float4 bgColor, out float4 Out)
{
    //float size = 10.0;  // Numero di linee per asse. Puoi cambiarlo come desideri.
    //float lineWidth = 0.02;  // Spessore delle linee della griglia.
    //float4 gridColor = float4(0.0, 0.0, 0.0, 1.0);  // Colore delle linee della griglia.
    //float4 bgColor = float4(1.0, 1.0, 1.0, 1.0);  // Colore di sfondo.

    Out = Grid(uv, size, lineWidth, gridColor, bgColor);
}

void DoMultipleGrid_float(float2 uv, float size, float lineWidth, float4 gridColor, float4 bgColor, out float4 Out)
{

    Out = Grid(uv, size, lineWidth, gridColor, bgColor) + Grid(uv,  lineWidth +  size*4, lineWidth/2, gridColor, bgColor);
}