//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System.Collections.Generic;

namespace GABAK
{
    internal class socketservers
    {
        private List<socketclient> availableconcordeservers;

        public socketservers()
        {
            availableconcordeservers = new List<socketclient>();
        }

        public void checkAvailableConcordeServers()
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader("C:\\gabak\\availableservers.txt");
            while ((line = file.ReadLine()) != null)
            {
                string[] raw = line.Split(',');
                socketclient temp = new socketclient(raw[0], int.Parse(raw[1]));
                availableconcordeservers.Add(temp);
            }
            file.Close();
        }

        public List<socketclient> getAvialableServers()
        {
            return availableconcordeservers;
        }
    }
}