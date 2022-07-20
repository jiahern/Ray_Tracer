using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Plane : SceneEntity
    {
        private Vector3 center;
        private Vector3 normal;
        private Material material;

        /// <summary>
        /// Construct an infinite plane object.
        /// </summary>
        /// <param name="center">Position of the center of the plane</param>
        /// <param name="normal">Direction that the plane faces</param>
        /// <param name="material">Material assigned to the plane</param>
        public Plane(Vector3 center, Vector3 normal, Material material)
        {
            this.center = center;
            this.normal = normal.Normalized();
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the plane, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            //Vector3 
            // Write your code here...

            double denom = (ray.Direction).Dot(this.normal);


            // Console.WriteLine(Epsilon);

            double t = 0.0;
            Vector3 planPoint = new Vector3(0.0, 0.0, 0.0);
            if (Math.Abs(denom) > double.Epsilon)
            {
                // Console.WriteLine(Math.Abs(denom));
                planPoint = this.center - ray.Origin;
                t = planPoint.Dot(this.normal) / denom;
                if (t <= 0)
                {
                    return null;
                }
                Vector3 position = ray.Origin + (t * ray.Direction);
                Vector3 incident = position - ray.Origin;

                RayHit hit = new RayHit(position, this.normal.Normalized(), incident.Normalized(), this.material);


                return hit;

                //line 44- 52 is reference code from https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
            }

            return null;






        }

        /// <summary>
        /// The material of the plane.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
