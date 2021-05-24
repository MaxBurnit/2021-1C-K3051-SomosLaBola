using System;
using Microsoft.Xna.Framework;

namespace SomosLaBola.Obstaculos.Recorridos
{
    class RecorridoOnda : IRecorrido
    {
        public RecorridoOnda(float amplitud, float periodo, float tiempoPeriodo)
        {
            this.amplitud = amplitud;
            this.periodo = periodo;
            this.tiempoPeriodo = tiempoPeriodo;
        }

        private float amplitud;
        private float periodo;
        private float tiempoPeriodo;

        public Matrix ObtenerMovimiento(float tiempoTranscurrido)
        {
            var a = amplitud;
            var b = 2 * MathHelper.Pi / periodo;
            var periodosRecorridos = tiempoTranscurrido / tiempoPeriodo;
            var traslacionX = periodo * periodosRecorridos;
            var traslacionY = a * MathF.Sin(b * periodosRecorridos);
            var traslacionDelCentro = Matrix.CreateTranslation(traslacionX, traslacionY, 0);
            return traslacionDelCentro;
        }
    }
}