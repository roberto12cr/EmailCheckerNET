using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckEmailTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = string.Empty;
            do
            {
                Console.WriteLine("Email to validate. Empty to exit.");
                input = Console.ReadLine();

                try
                {
                    bool result = EmailCheckerNET.EmailValidator.CheckMailBox(input);
                    Console.WriteLine("Result: " + result.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }
            }
            while (string.IsNullOrEmpty(input) == false);
            
        }
    }
}
