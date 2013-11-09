using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib;
using Midi;

namespace Drums
{
    class Program
    {
        public static void Main(string[] args)
        {
            MidiDrums md = new MidiDrums();
            DrumSet wd = new DrumSet();
            wd.DrumHit = md.PlayDrum;
            wd.Start();
            md.PlayDrum(Drum.Pedal, 4);
            Console.WriteLine("Sleep.");
            Console.Read();
        }
    }

    class MidiDrums
    {
        OutputDevice outputDevice;
        public MidiDrums()
        {
            outputDevice = OutputDevice.InstalledDevices[0];
            outputDevice.Open();
        }

        public void PlayDrum(Drum d, int velocity)
        {
            outputDevice.SendPercussion(DrumToMidiPercusssion[(int)d], GhToMidiVelocity[velocity]);
        }

        //Blue = 0,
        //Green = 1,
        //Orange = 2,
        //Pedal = 3,
        //Red = 4,
        //Yellow = 5,
        static readonly Percussion[] DrumToMidiPercusssion =
        {
            Percussion.MidTom2,
            Percussion.LowTom2,
            Percussion.CrashCymbal1,//or 52
            Percussion.BassDrum1,
            Percussion.SnareDrum1,
            Percussion.ClosedHiHat,
            Percussion.Cowbell
        };
        static readonly int[] GhToMidiVelocity = { 127, 126, 125, 124, 123, 122, 121 };
    }

    delegate void DrumHit(Drum d, int velocity);

    class DrumSet
    {
        Wiimote wiimote;
        public DrumSet()
        {
            wiimote = new Wiimote();
            // setup the event to handle state changes
            wiimote.WiimoteChanged += GotWiimoteData;
            
        }

        public void Start(){
            wiimote.Connect();
            wiimote.SetLEDs(false, true, false, true);
        }

        public readonly bool[] States = new bool[6];

        void GotWiimoteData(object sender, WiimoteChangedEventArgs e)
        {
            //Console.WriteLine("Data!");
            var s = e.WiimoteState.DrumsState;
            HandleDrum(Drum.Blue, s.Blue, s.BlueVelocity);
            HandleDrum(Drum.Green, s.Green, s.GreenVelocity);
            HandleDrum(Drum.Orange, s.Orange, s.OrangeVelocity);
            HandleDrum(Drum.Pedal, s.Pedal, s.PedalVelocity);
            HandleDrum(Drum.Red, s.Red, s.RedVelocity);
            HandleDrum(Drum.Yellow, s.Yellow, s.YellowVelocity);

            counter++;
            if (counter % 100 == 0)
            {
                wiimote.SetLEDs(light==0,light==1,light==2,light==3);
                light = ++light % 4;
            }
        }
        
        int counter;
        int light = 0;

        public DrumHit DrumHit;

        void HandleDrum(Drum drum, bool down, int velocity)
        {
            if (!down)
            {
                States[(int)drum] = false;
                return;
            }
            var old = States[(int)drum];
            if(down!=old){
                Console.WriteLine("Hit the {0}!", drum);
                States[(int)drum]=true;
                DrumHit(drum,velocity);
            }
        }
    }

    enum Drum
    {
        Blue = 0,
        Green = 1,
        Orange = 2,
        Pedal = 3,
        Red = 4,
        Yellow = 5,

        //Tom,CrashCymbal,

        //Snare = Red,
        //HiHat = Yellow,
        //Tom = Blue,
        //CrashCymbal = Green,
        //BaseDrum = Pedal
    }

    
    struct DrumState
    {
        public Drum drum;
        public int velocity;
        public bool down;
        public DateTime timestamp;

        public override bool Equals(object obj)
        {
            if (!(obj is DrumState)) return false;
            var o = (DrumState)obj;
            return drum == o.drum && o.velocity == velocity && down == o.down;
        }
    }
}
