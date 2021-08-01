using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SomosLaBola.PlayerInfo;

namespace SomosLaBola.Triggers
{
    abstract class Trigger
    {
        public BoundingSphere BoundingSphere;
        public static float Radio = 5;
        public Model Content;


        protected Trigger(Vector3 centro, Model content)
        {
            this.Content = content;
            BoundingSphere = new BoundingSphere(centro, Radio);
        }

        public abstract void AplicarEfecto();

        public abstract Color Color();

    }

    class Planear : Trigger
    {
        public Planear(Vector3 centro, Model content) : base(centro, content)
        {
        }

        public override void AplicarEfecto()
        {
            Player.AplicarEfecto(Efecto);
            Player.AddEfectoPostergardo(5, Deshacer);
        }

        public override Color Color()
        {
            return Microsoft.Xna.Framework.Color.Blue;
        }

        private PlayerStatus Efecto(PlayerStatus playerStatus)
        {
            playerStatus.MaxFallingSpeed = 50f;
            return playerStatus;
        }

        private PlayerStatus Deshacer(PlayerStatus playerStatus)
        {
            playerStatus.MaxFallingSpeed = 5000f;
            return playerStatus;
        }

    }

    class Checkpoint : Trigger
    {
        public static Vector3 CurrentCheckpoint = new Vector3(0, 50, -20);
        //public static Vector3 CurrentCheckpoint = new Vector3(6100, -650,-9150);

        public Checkpoint(Vector3 centro) : base(centro, null)
        {
        }

        public override void AplicarEfecto()
        {
            CurrentCheckpoint = BoundingSphere.Center;
        }

        public override Color Color()
        {
            return Microsoft.Xna.Framework.Color.LightGreen;
        }
    }

    

    //TODO efectos?
    /*
    class planearPowerUp : IPowerUp
    {
        public planearPowerUp(Vector3 centro, float radio) : base(centro, radio) { }
    }
    */
}
