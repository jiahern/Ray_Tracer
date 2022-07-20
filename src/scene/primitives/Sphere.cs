using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            Vector3 L = this.center - ray.Origin;
            double tca = L.Dot(ray.Direction);

            // if (tca < 0)
            // {
            //     return null;
            // }

            double d2 = L.Dot(L) - (tca * tca);
            double sq_radius = this.radius * this.radius;
            if (d2 > sq_radius)
            {
                return null;
            }

            double thc = Math.Sqrt(sq_radius - d2);

            double t0 = tca - thc;
            double t1 = tca + thc;

            if (t0 > t1)
            {
                double temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1;
                if (t0 < 0)
                {
                    return null;
                }
            }

            double t = t0;

            Vector3 position = ray.Origin + (t * ray.Direction);
            Vector3 normal = position - this.center;
            Vector3 incident = position - ray.Origin;
            RayHit hit = new RayHit(position, normal.Normalized(), incident.Normalized(), this.material);


            return hit;

            //line 35-71 code is referenced from https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection

        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
