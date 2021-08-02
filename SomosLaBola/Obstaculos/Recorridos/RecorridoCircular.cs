using System;
using Microsoft.Xna.Framework;

namespace SomosLaBola.Obstaculos.Recorridos
{
    class RecorridoCircular : IRecorrido
    {
        private static Random random = new Random(0);
        public RecorridoCircular(float radio, float tiempoDeVuelta)
        {
            this.radio = radio;
            this.tiempoDeVuelta = tiempoDeVuelta;
            this.sentido = random.Next(2) * 2 - 1;
            this.desfasaje = random.Next(100);
        }

        private float radio;
        private float tiempoDeVuelta;
        private float sentido;
        private float desfasaje;

        public Matrix ObtenerMovimiento(float tiempoTranscurrido)
        {
            var porcientoVuelta = (desfasaje/100 + tiempoTranscurrido % tiempoDeVuelta / tiempoDeVuelta) % 1;
            var radianesVuelta = 2 * MathHelper.Pi * porcientoVuelta;
            var traslacionX = radio * MathF.Cos(radianesVuelta) * sentido;
            var traslacionY = radio * MathF.Sin(radianesVuelta);
            var traslacionDelCentro = Matrix.CreateTranslation(traslacionX, traslacionY, 0);
            return traslacionDelCentro;
        }
    }
}
