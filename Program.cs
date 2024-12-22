using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hampter2
{
    public static class Program
    {
        public static int MaxDelay = 1000 / 2;
        public static int MinDelay = 1000 / 2;
        public static int Wait = 1000 * 10;
        public static void Main(string[] args)
        {
            Random RNG = new Random((int)DateTime.Now.Ticks);
            HampterSoundPlayer player = new HampterSoundPlayer();
            while (true)
            {
                player.Hampter();

                int sleepLength = RNG.Next(MinDelay, MaxDelay + 1);
                Console.WriteLine($"Hamping in {sleepLength / 1000.0} seconds.");

                Thread.Sleep(sleepLength);
            }
        }
    }
}
