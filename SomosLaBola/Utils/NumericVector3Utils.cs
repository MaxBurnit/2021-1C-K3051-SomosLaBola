using NumericVector3 = System.Numerics.Vector3;
using Microsoft.Xna.Framework;

namespace SomosLaBola.Utils
{
    class NumericVector3Utils
    {
        public static NumericVector3 Forward = Vector3Utils.toNumeric(Vector3.Forward);
        public static NumericVector3 Backward = Vector3Utils.toNumeric(Vector3.Backward);
        public static NumericVector3 Down = Vector3Utils.toNumeric(Vector3.Down);
        public static NumericVector3 Up = Vector3Utils.toNumeric(Vector3.Up);
        public static NumericVector3 Right = Vector3Utils.toNumeric(Vector3.Right);
        public static NumericVector3 Left = Vector3Utils.toNumeric(Vector3.Left);

        public static Vector3 ToXnaVector3(NumericVector3 vector3)
        {
            return new Vector3(vector3.X, vector3.Y, vector3.Z);
        }
    }
}