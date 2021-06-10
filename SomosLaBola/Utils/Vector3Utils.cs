using NumericVector3 = System.Numerics.Vector3;
using Microsoft.Xna.Framework;

namespace SomosLaBola.Utils
{
    class Vector3Utils
    {
        public static NumericVector3 toNumeric(Vector3 vector3)
        {
            return new NumericVector3(vector3.X, vector3.Y, vector3.Z);
        } 
    }
}
