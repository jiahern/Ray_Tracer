using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...

            //Compute plane normal
            Vector3 v0v1 = this.v1 - this.v0;
            Vector3 v0v2 = this.v2 - this.v0;

            //dont need normalize
            Vector3 N = v0v1.Cross(v0v2);
            double area2 = N.Length();


            //Step 1: Finding P

            //check if ray and plane are parallel?
            double NdotRayDirection = N.Dot(ray.Direction);

            if (Math.Abs(NdotRayDirection) < double.Epsilon)
            {

                return null; //they are parellel so dont intersect
            }

            // compute d parameter
            // double d = N.Dot(this.v0);

            //compute t
            double t = (N.Dot(this.v0 - ray.Origin)) / NdotRayDirection;

            // check if the triangle is in behind the ray
            if (t <= 0)
            {
                return null; //the triangle is behind
            }

            // compute the intersection point
            Vector3 P = ray.Origin + (t * ray.Direction);

            // Step 2: inside-outside test
            Vector3 C; //Vector perpendicular to tirangle's plane

            //edge 0
            Vector3 edge0 = this.v1 - this.v0;
            Vector3 vp0 = P - this.v0;
            C = edge0.Cross(vp0);
            if (N.Dot(C) < 0)
            {
                return null; //P is on the right side
            }

            //edge 1
            Vector3 edge1 = this.v2 - this.v1;
            Vector3 vp1 = P - this.v1;
            C = edge1.Cross(vp1);
            if (N.Dot(C) < 0)
            {
                return null; //P is on the right side
            }

            //edge 2
            Vector3 edge2 = this.v0 - this.v2;
            Vector3 vp2 = P - this.v2;
            C = edge2.Cross(vp2);
            if (N.Dot(C) < 0)
            {
                return null; //P is on the right side
            }


            //Vector3 position = ray.Origin + (t * ray.Direction);

            Vector3 incident = P - ray.Origin;

            RayHit hit = new RayHit(P, N.Normalized(), incident.Normalized(), this.material);


            return hit;

            // line code 38-100 refrenced from https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
