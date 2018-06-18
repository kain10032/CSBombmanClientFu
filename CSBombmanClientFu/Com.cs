using System;
using System.Collections.Generic;
using System.Text;

namespace CSBombmanClientFu
{
    class Com
    {
        private int myId = -1;

        public Com() { }

        public void Run()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            // player name
            Console.WriteLine("ふ", Console.OutputEncoding.CodePage);

            // get player id
            SetId(Console.ReadLine());

            while (true)
            {
                string data = Console.ReadLine();

                try
                {
                }
            }
        }

        private void SetId(string idStr)
        {
            if (int.TryParse(idStr, out int x))
            {
                myId = x;
            }
        }
    }
}
