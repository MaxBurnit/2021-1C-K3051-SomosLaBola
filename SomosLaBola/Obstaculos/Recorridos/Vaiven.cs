using System;
using Microsoft.Xna.Framework;

namespace SomosLaBola.Obstaculos.Recorridos
{
    class Vaiven : IRecorrido
    {
        public Vaiven(float distancia, float tiempoRecorridoIda, float tiempoRecorridoVuelta)
        {
            this.distancia = distancia;
            this.tiempoRecorridoIda = tiempoRecorridoIda;
            this.tiempoRecorridoVuelta = tiempoRecorridoVuelta;
            this.tiempoCiclo = tiempoRecorridoIda + tiempoRecorridoVuelta;
        }

        private float distancia;
        private float tiempoRecorridoIda;
        private float tiempoRecorridoVuelta;
        private float tiempoCiclo;

        public Matrix ObtenerMovimiento(float tiempoTranscurrido)
        {
            var tiempoRecorridoCiclo = tiempoTranscurrido % tiempoCiclo;
            if (tiempoRecorridoCiclo < tiempoRecorridoIda)
            {
                var porcientoIdaRecorrido = tiempoRecorridoCiclo / tiempoRecorridoIda;
                var zPosition = distancia * porcientoIdaRecorrido;
                return Matrix.CreateTranslation(0, 0, zPosition);
            }
            else
            {
                var porcientoVueltaRecorrida = (tiempoRecorridoCiclo - tiempoRecorridoIda) / tiempoRecorridoVuelta;
                var zPosition = distancia - distancia * porcientoVueltaRecorrida;
                return Matrix.CreateTranslation(0, 0, zPosition);
            }
        }
    }
}