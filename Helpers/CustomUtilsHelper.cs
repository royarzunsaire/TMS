using System;
using System.Text.RegularExpressions;

namespace SGC.Helpers
{
    public static class CustomUtilsHelper
    {
        // Generacion de codigo al azar
        public static string GeneracionCodigoCurso()
        {
            string numbers = "1234567890";

            string characters = numbers;
            int length = 10;
            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return "C" + otp;
        }

        // Generacion de codigo al azar
        public static string GeneracionCodigo(string prefijo, string ultimo)
        {
            var ultimoNumero = Regex.Match(ultimo, @"\d+").Value;
            var ultimoInt = Int32.Parse(ultimoNumero);
            return prefijo + (ultimoInt + 1);
        }

    }
}