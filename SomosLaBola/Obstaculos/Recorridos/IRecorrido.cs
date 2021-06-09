using Microsoft.Xna.Framework;

namespace SomosLaBola.Obstaculos.Recorridos
{
    interface IRecorrido
    {
        Matrix ObtenerMovimiento(float tiempoTranscurrido);
    }
}