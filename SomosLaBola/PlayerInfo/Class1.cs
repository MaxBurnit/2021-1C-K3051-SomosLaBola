using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SomosLaBola.PlayerInfo
{
    static class Player
    {
        public static PlayerStatus PlayerStatus = new PlayerStatus();

        //Segundo a activar y efecto a aplicar
        public static readonly List<(float, Func<PlayerStatus, PlayerStatus>)> EfectosPostergardos = 
            new List<(float, Func<PlayerStatus, PlayerStatus>)>();

        public static float PlayedTime;

        public static void AddEfectoPostergardo(float retardo, Func<PlayerStatus, PlayerStatus> effect)
        {
            EfectosPostergardos.Add((retardo + PlayedTime, effect));
        }

        public static void AplicarEfecto(Func<PlayerStatus, PlayerStatus> effect)
        {
            PlayerStatus = effect(PlayerStatus);
        }

        public static void Update(GameTime gameTime)
        {
            PlayedTime = (float)gameTime.TotalGameTime.TotalSeconds;

            var efectosActivados = new List<(float, Func<PlayerStatus, PlayerStatus>)>();

            foreach (var efectoPostergardo in EfectosPostergardos)
            {
                var (triggerTime, efecto) = efectoPostergardo;
                if (triggerTime < PlayedTime)
                {
                    AplicarEfecto(efecto);
                    efectosActivados.Add(efectoPostergardo);
                }
            }

            EfectosPostergardos.RemoveAll(efectosActivados.Contains);

        }

        public static void resetStatus()
        {
            PlayerStatus = new PlayerStatus();
        }
    }

    class PlayerStatus
    {
        public float MaxFallingSpeed = 5000f;
        
    }
}
