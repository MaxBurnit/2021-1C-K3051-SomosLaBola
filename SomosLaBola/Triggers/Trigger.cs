using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SomosLaBola.Geometries;
using SomosLaBola.Obstaculos.Recorridos;
using SomosLaBola.PlayerInfo;
using static SomosLaBola.PlayerInfo.Player;

namespace SomosLaBola.Powerups
{
    abstract class Trigger
    {
        public BoundingSphere BoundingSphere;
        public static float Radio = 5;

        public Trigger(Vector3 centro)
        {
            BoundingSphere = new BoundingSphere(centro, Radio);
        }

        public abstract void AplicarEfecto();

        public abstract Color Color();

    }

    class Planear : Trigger
    {
        public Planear(Vector3 centro) : base(centro)
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
        //public static Vector3 CurrentCheckpoint = new Vector3(0, 50, -20);
        public static Vector3 CurrentCheckpoint = new Vector3(6100, -650, -9150);

        public Checkpoint(Vector3 centro) : base(centro)
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
