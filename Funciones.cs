using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instalador_personalizable
{
    internal class Logger
    {
        public static void Info(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Funciones.Escribir("[");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Funciones.Escribir("INFO");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Funciones.Escribir("] ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Funciones.Escribir(mensaje + "\n");
        }

        public static void Error(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Funciones.Escribir("[");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Funciones.Escribir("ERROR");
            Console.ForegroundColor = ConsoleColor.Red;
            Funciones.Escribir("] ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Funciones.Escribir(mensaje + "\n");
        }

        public static void Listar(string mensaje, int orden)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Funciones.Escribir("[");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Funciones.EscribirN(orden);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Funciones.Escribir("] ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Funciones.Escribir(mensaje + "\n");
        }

        public static void Ask(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Funciones.Escribir("\n[");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Funciones.Escribir("PREGUNTA");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Funciones.Escribir("] ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Funciones.Escribir(mensaje);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Funciones.Escribir("\n\n» ");
        }
    }

    internal class Funciones
    {
        public static void Escribir(string mensaje)
        {
            for (int i = 0; i < mensaje.Length; i++)
            {
                Console.Write(mensaje[i]);
                Thread.Sleep(18);
            }
        }

        public static void EscribirN(int num)
        {
            Console.Write(num);
            Thread.Sleep(18);
        }
    }
}
