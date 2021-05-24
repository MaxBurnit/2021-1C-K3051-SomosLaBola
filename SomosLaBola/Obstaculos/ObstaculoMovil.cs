using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SomosLaBola.Obstaculos.Recorridos;

namespace SomosLaBola.Obstaculos
{
    class ObstaculoMovil
    {
        public ObstaculoMovil(Model model, Matrix transformaciones, IRecorrido recorrido)
        {         
            this.model = model;
            this.recorrido = recorrido;
            this.transformaciones = transformaciones;
        }

        private Matrix transformaciones;
        private Model model;
        private IRecorrido recorrido;

        public void Draw(float tiempoTranscurrido, Matrix vista, Matrix proyeccion)
        {
            var matrixMundoTrasladada = transformaciones * recorrido.ObtenerMovimiento(tiempoTranscurrido);
            model.Draw(matrixMundoTrasladada, vista, proyeccion);
        }

        public static ObstaculoMovil CrearObstaculoRecorridoCircular(Model model, Matrix transformaciones)
        {
            var recorrido = new RecorridoCircular(13, 7);
            return new ObstaculoMovil(model, transformaciones, recorrido);
        }
        public static ObstaculoMovil CrearObstaculoRecorridoOnda(Model model, Matrix transformaciones)
        {
            var recorrido = new RecorridoOnda(5, 3, 2);
            return new ObstaculoMovil(model, transformaciones, recorrido);
        }
        public static ObstaculoMovil CrearObstaculoRecorridoVaiven(Model model, Matrix transformaciones)
        {
            var recorrido = new Vaiven(4, 0.5f, 2);
            return new ObstaculoMovil(model, transformaciones, recorrido);
        }

    }
}
