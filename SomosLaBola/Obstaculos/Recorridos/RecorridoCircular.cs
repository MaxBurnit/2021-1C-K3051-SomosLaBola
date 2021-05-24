using System;
using Microsoft.Xna.Framework;

namespace SomosLaBola.Obstaculos.Recorridos
{
    class RecorridoCircular : IRecorrido
    {
        public RecorridoCircular(float radio, float tiempoDeVuelta)
        {
            this.radio = radio;
            this.tiempoDeVuelta = tiempoDeVuelta;
        }

        private float radio;
        private float tiempoDeVuelta;

        public Matrix ObtenerMovimiento(float tiempoTranscurrido)
        {
            var porcientoVuelta = tiempoTranscurrido % tiempoDeVuelta / tiempoDeVuelta;
            var radianesVuelta = 2 * MathHelper.Pi * porcientoVuelta;
            var traslacionX = radio * MathF.Cos(radianesVuelta);
            var traslacionY = radio * MathF.Sin(radianesVuelta);
            var traslacionDelCentro = Matrix.CreateTranslation(traslacionX, traslacionY, 0);
            return traslacionDelCentro;
        }
    }
}