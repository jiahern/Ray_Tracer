using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            //Console.WriteLine(this.options.AAMultiplier);
            // Begin writing your code here...
            int width = outputImage.Width;
            int height = outputImage.Height;

            //Origin Camera Position
            Vector3 origin = new Vector3(0, 0, 0);
            //given fixed field of view
            double hfov = (Math.PI) / 3;
            // Console.WriteLine(hfov);
            double aspect_Ratio = width / height;
            
            int depth = 0;
            double bias = 0.001;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

           

            //Shoot the ray to the scene
            for (int x = 0; x < width; x++)
            {
                
                System.Threading.Thread.Sleep(10);

                 Color newColor = new Color(0,0,0);
                
                
                for (int y = 0; y < height; y++)
                {
                    if(this.options.AAMultiplier < 2){

                        Ray newRay = new Ray(origin, convert_pix(x,y,width,height,hfov).Normalized());

                            newColor += castRay(this.lights, this.entities, newRay, bias, depth);


                    }else{
                    double AAMul = this.options.AAMultiplier;
                    //Console.WriteLine(AAMul);
                    //Console.WriteLine(AAMul);
                    double AA = 1/ (AAMul * AAMul);
                     
                    for(double i = AA; i<1;){
                        
                        for(double j= AA; j<1;){
                            //Console.WriteLine(j);
                            //double AA = 1/ (this.options.AAMultiplier * this.options.AAMultiplier);
                            double AAx = x+ i;
                            double AAy = y+ j;
                            
                            Ray newRay = new Ray(origin, convert_pix(AAx,AAy,width,height,hfov).Normalized());

                            newColor += castRay(this.lights, this.entities, newRay, bias, depth);

                            j+= 1/AAMul; 
                        }    
                        i+= 1/AAMul;  
                    }
                 
                    newColor /= (AAMul*AAMul);}
                    outputImage.SetPixel(x, y, newColor);

                    //Revert to default Color
                    newColor = new Color(0,0,0);
                 
                    
                }

            }
            // Stop.
            stopwatch.Stop();
        
            // Write hours, minutes and seconds.
            Console.WriteLine("Time elapsed: {0:hh\\:mm\\:ss}", stopwatch.Elapsed);
        }
        
        //Make the pixel point become Left-Handed Coordinate System
        private Vector3 convert_pix (double x, double y, int width, int height, double hfov){

            double aspect_Ratio = width / height;
            double new_loc_X = (x) / width;
            double new_loc_Y = (y) / height;

            new_loc_X = (new_loc_X * 2) - 1;
            new_loc_Y = 1 - (new_loc_Y * 2);

            new_loc_X = new_loc_X * Math.Tan(hfov / 2);
            new_loc_Y = (new_loc_Y * (Math.Tan(hfov / 2)) / aspect_Ratio);

        return new Vector3(new_loc_X, new_loc_Y,1);
        }


        private Color castRay(ISet<PointLight> lights, ISet<SceneEntity> entities, Ray newRay, double bias, double depth)
        {

            // Declare the Max Value for Point
            double borderDistance = double.MaxValue;

            //default color background is black
            Color newColor = new Color(0, 0, 0);
            RayHit borderHit = null;
            SceneEntity borderSceneEntity = null;

            if (depth > 6)
            {
                return newColor;
            }

            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(newRay);
                if (hit != null && ((hit.Position - newRay.Origin).LengthSq() < borderDistance))
                {
                    // We got a hit with this entity!
                    borderDistance = (hit.Position - newRay.Origin).LengthSq();
                    borderSceneEntity = entity;
                    borderHit = hit;
                }
            }

            if (borderHit != null)
            {
                
                // Apply Diffuse and Shadow Effects 
                if (borderSceneEntity.Material.Type == Material.MaterialType.Diffuse)
                {

                    foreach (PointLight light in this.lights)
                    {
                        Vector3 biasShadow = (light.Position - borderHit.Position).Normalized() * bias;
                        //Find the light direction of hit surface from light source
                        Vector3 light_Direction = (light.Position - (borderHit.Position + biasShadow));

                        double NdotL = Math.Max(0.0, (borderHit.Normal).Dot((light.Position - borderHit.Position).Normalized()));

                        //declare light distance for Shadow Calculation
                        double source_to_Light_Distance = (light.Position - borderHit.Position).LengthSq();

                        bool show_shadow = true;

                        foreach (SceneEntity Shadow in this.entities)
                        {
                            Ray shadowRay = new Ray(borderHit.Position + biasShadow, light_Direction.Normalized());
                            RayHit shadowArea = Shadow.Intersect(shadowRay);
                            if (shadowArea != null && (((shadowArea.Position - (shadowRay.Origin)).LengthSq()) < source_to_Light_Distance))
                            {
                                show_shadow = false;
                                break;
                            }
                        }
                        if (show_shadow)
                        {
                            newColor += (borderSceneEntity.Material.Color) * (NdotL) * (light.Color);
                        }
                    }
                }
                //Apply Reflective effect
                else if (borderSceneEntity.Material.Type == Material.MaterialType.Reflective)
                {
                    Vector3 reflect_Direction = Reflection(borderHit);
                    Vector3 reflect_bias = borderHit.Normal * bias;
                    Ray reflect_Ray = new Ray(borderHit.Position + reflect_bias, reflect_Direction);
                    newColor += castRay(lights, entities, reflect_Ray, bias, depth + 1);
                }

                //Apply Refraction & Reflection
                else if (borderSceneEntity.Material.Type == Material.MaterialType.Refractive){

                    Color refractionColor = new Color(0,0,0);
                    Color reflectionColor = new Color(0,0,0);

                    double kr = Fresnel(borderHit.Incident, borderHit.Normal, borderSceneEntity.Material.RefractiveIndex);
                    bool outside = borderHit.Incident.Dot(borderHit.Normal) < 0;
                    //Console.WriteLine(outside);
                    Vector3 newBias = bias * borderHit.Normal;

                    if(kr<1){
                        Vector3 refractionDirection = Refract(borderHit.Incident,borderHit.Normal,borderSceneEntity.Material.RefractiveIndex).Normalized();
                        Vector3 refractionRayOrigin;
                        if (outside){
                            refractionRayOrigin = borderHit.Position - newBias; 
                        }else{
                            refractionRayOrigin = borderHit.Position + newBias;
                        }
                        Ray refractionRay = new Ray(refractionRayOrigin,refractionDirection);
                        refractionColor = castRay(lights, entities, refractionRay,bias,depth+1);
                        
                    } 

                    Vector3 reflectionDirection = Reflection(borderHit).Normalized();
                    Vector3 reflectionRayOrigin;
                        if (outside){
                            reflectionRayOrigin = borderHit.Position + newBias; 
                        }else{
                            reflectionRayOrigin = borderHit.Position - newBias;
                        }
                         Ray reflectionRay = new Ray(reflectionRayOrigin,reflectionDirection);
                        reflectionColor = castRay(lights, entities, reflectionRay,bias,depth+1);   

                        newColor += reflectionColor * kr + refractionColor *(1-kr);  
                        
                }

                else if (borderSceneEntity.Material.Type == Material.MaterialType.Glossy){
                    //for Diffuse
                    double Kd = 0.4;

                    //for Specular
                    double Ks = 0.8;
                    
                    //the power for specular
                    int n = 2;

                    foreach (PointLight light in this.lights)
                    {
                        Vector3 biasShadow = (light.Position - borderHit.Position).Normalized() * bias;
                        //Find the light direction of hit surface from light source
                        Vector3 light_Direction = (light.Position - (borderHit.Position + biasShadow));

                        double NdotL = Math.Max(0.0, (borderHit.Normal).Dot((light.Position - borderHit.Position).Normalized()));

                        //declare light distance for Shadow Calculation
                        double source_to_Light_Distance = (light.Position - borderHit.Position).LengthSq();

                        bool show_shadow = true;

                        foreach (SceneEntity Shadow in this.entities)
                        {
                            Ray shadowRay = new Ray(borderHit.Position + biasShadow, light_Direction.Normalized());
                            RayHit shadowArea = Shadow.Intersect(shadowRay);
                            if (shadowArea != null && (((shadowArea.Position - (shadowRay.Origin)).LengthSq()) < source_to_Light_Distance))
                            {
                                show_shadow = false;
                                break;
                            }
                        }
                        if (show_shadow)
                        {
                            newColor += (borderSceneEntity.Material.Color) * (NdotL) * (light.Color)*Kd;
                        }
                    }
                    
                    Vector3 reflect_Direction = Reflection(borderHit);
                    Vector3 reflect_bias = borderHit.Normal * bias;
                    double specular = Math.Pow(Math.Max(0.0f,reflect_Direction.Dot(-borderHit.Incident)),n) ;
                    Ray reflect_Ray = new Ray(borderHit.Position + reflect_bias, reflect_Direction);

                    


                    newColor += castRay(lights, entities, reflect_Ray, bias, depth + 1)*specular*Ks;



                }
            }
            return newColor;
        }

        private Vector3 Reflection(RayHit borderHit)
        {
            Vector3 reflect_Direction = borderHit.Incident - 2 * borderHit.Incident.Dot(borderHit.Normal) * borderHit.Normal;
            return reflect_Direction.Normalized();

            //code referenced from https://www.scratchapixel.com/code.php?id=13&origin=/lessons/3d-basic-rendering/introduction-to-shading
        }

        // Refraction 
        private Vector3 Refract(Vector3 I, Vector3 N, double refractiveIndex){

            double cosI = I.Dot(N);
            //Console.WriteLine(cosI);
            double etai = 1;
            double etat = refractiveIndex;
            Vector3 n = N;
            if(cosI<0){ 
                cosI = -cosI;
            }else{
                double temp = etai;
                etai = etat;
                etat = temp;
                n = -N;
            }
            double eta = etai/etat;
            double k = 1 - eta*eta*(1-cosI*cosI);
            Vector3 result = eta * I + (eta*cosI - Math.Sqrt(k))*n;
            return result;


           //Code referenced from https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel

        }
        //Fresnel Effect to determine when to use Reflection or Refraction
        private double Fresnel(Vector3 I, Vector3 N, double refractiveIndex){
            double kr;
            double cosI = I.Dot(N);
            //Console.WriteLine(cosI);
            double etai = 1;
            double etat = refractiveIndex;
            Vector3 normal = N;
            if(cosI>=0){
                double temp= etai;
                etai = etat;
                etat = temp;
            }

            double sint = etai/etat * Math.Sqrt(Math.Max(0.0f,1-cosI*cosI));

            if(sint >= 1){
                kr = 1;
            }else{

                double cost = Math.Sqrt(Math.Max(0.0f,1-sint*sint));
                cosI = Math.Abs(cosI);
                double Rs = ((etat * cosI)-(etai*cost))/((etat*cosI)+(etai*cost));
                double Rp = ((etai * cosI)-(etat*cost))/((etai*cosI)+(etat*cost));
                kr = (Rs*Rs + Rp*Rp)/2;
            }

            return kr;

        //code referenced from https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel 
        }
    }
}



