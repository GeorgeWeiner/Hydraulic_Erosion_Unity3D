using UnityEngine;

namespace Terrain
{
    /*Implementation of Paper written by Hans Beyer:
    https://www.firespark.de/resources/downloads/implementation%20of%20a%20methode%20for%20hydraulic%20erosion.pdf*/
    
    public class HydraulicErosion : MonoBehaviour
    {
        [SerializeField] [Range(100, 1000000)] private int numberOfDroplets = 10000;
        [SerializeField] [Range(0,1)] private float dropletInertia;
        [SerializeField] [Range(0,1)] private float dropletDepositionSpeed;
        [SerializeField] [Range(0,1)] private float dropletErosionSpeed;
        [SerializeField] [Range(0,1)] private float dropletEvaporationSpeed;
        [SerializeField] private float dropletSedimentCapacity;
        [SerializeField] private int maxDropletPath;
        [SerializeField] private int erosionRadius;
        [SerializeField] private float minimumSlope;
        [SerializeField] private float gravity;

        public float[,] erosionMask { get; private set; }
        public float[,] depositionMask { get; private set; }
    

        public float[,] Erode(float[,] heightField, int mapSize)
        {
            var iterations = 0;
            erosionMask = new float[mapSize, mapSize];
            depositionMask = new float[mapSize, mapSize];
            
            for (var dropletIndex = 0; dropletIndex < numberOfDroplets; dropletIndex++)
            {
                //Define the droplets properties
                Vector2 pos = Vector2.zero;
                Vector2 dir = Vector2.zero;
                var speed = 1.0f;
                var water = 1.0f;
                var sediment = 0.0f;

                //Create a new droplet at a random point of the map.
                pos = new Vector2(Random.Range(0f, mapSize - 1), Random.Range(0f, mapSize - 1));

                for (var dropletIteration = 0; dropletIteration < maxDropletPath; dropletIteration++)
                {
                    var posX = (int) pos.x;
                    var posY = (int) pos.y;
                    float alpha = pos.x - posX;
                    float beta = pos.y - posY;

                    //Cache the height of surrounding grid-cells
                    float heightNW = heightField[posX, posY];
                    float heightNE = heightField[posX + 1, posY];
                    float heightSW = heightField[posX, posY + 1];
                    float heightSE = heightField[posX + 1, posY + 1];
                
                    //Use the surrounding heights to perform simple bilinear interpolation to get gradient at current position
                    float gradientX = (heightNE - heightNW) * (1 - beta) + (heightSE - heightSW) * beta;
                    float gradientY = (heightSW - heightNW) * (1 - alpha) + (heightSE - heightNE) * alpha;

                    Vector2 currentGradient = new Vector2(gradientX, gradientY);

                    dir = (dir * dropletInertia - currentGradient * (1 - dropletInertia)).normalized;
                    pos += dir;

                    float heightDelta = heightField[(int) pos.x, (int) pos.y] - heightField[posX, posY];
                
                    //Is the droplet outside of the map bounds? If it is break out of the loop.
                    if ((dir.x == 0 && dir.y == 0) || pos.x < 0 || pos.x >= mapSize - 1
                        || pos.y < 0 || pos.y >= mapSize - 1) break;

                    float currentCarryCapacity = Mathf.Max(-heightDelta, minimumSlope) * speed * water * dropletSedimentCapacity;

                    //Is the droplet moving up a slope or is it at full capacity?
                    if (heightDelta > 0 || sediment > currentCarryCapacity) 
                    {
                        float amountToDeposit = (heightDelta > 0) ? Mathf.Min (heightDelta, sediment) : (sediment - currentCarryCapacity) * dropletDepositionSpeed;
                        sediment -= amountToDeposit;
                        
                        float amount = amountToDeposit * (1 - alpha) * (1 - beta);
                        
                        heightField[posX, posY] += amount;
                        depositionMask[posX, posY] += amount;
                    }
                    else
                    {
                       /*Use the pre-computed erosion brush to remove sediment within the radius of the brush,
                        proportional to the distance from the center*/
                       
                       float amountToErode = Mathf.Min ((currentCarryCapacity - sediment) * dropletErosionSpeed, -heightDelta);

                       if (posY <= erosionRadius || posY >= mapSize - erosionRadius || posX <= erosionRadius + 1 ||
                           posX >= mapSize - erosionRadius) break;
                       erosionMask[posX, posY] += amountToErode;
                       
                       for (int x = -erosionRadius; x < erosionRadius; x++)
                       {
                           for (int y = -erosionRadius; y < erosionRadius; y++)
                           {
                               float squaredDistance = Mathf.Pow(x,2) + Mathf.Pow(y,2);
                               if (squaredDistance < Mathf.Pow(erosionRadius, 2))
                               {
                                   int coordX = posX + x;
                                   int coordY = posY + y;
                                   if (coordX >= 0 && coordX < mapSize && coordY >= 0 && coordY < mapSize)
                                   {
                                       float weight = 1 - Mathf.Sqrt (squaredDistance) / erosionRadius;
                                       float weighedErodeAmount = amountToErode * weight;
                                       float deltaSediment = heightField[coordX, coordY] < weighedErodeAmount ? heightField[coordX, coordY] : weighedErodeAmount;
                                       heightField[coordX, coordY] -= deltaSediment;
                                       sediment += deltaSediment;
                                   }
                               }
                               
                           }
                       }
                    }
                    speed = Mathf.Sqrt (speed * speed + heightDelta * gravity);
                    water *= 1 - dropletEvaporationSpeed;
                    iterations++;
                }
            }
            print(iterations);
            return heightField;
        }
    }
}
