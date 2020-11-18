using System;
using System.Reflection;
using Transversal.Util.BaseDBUp;


namespace TestDBUp
{
    public class Program
    {
        static int Main(string[] args)
        {
            ResultMigration r = new ResultMigration();
            Migracion m;

            switch (args.LongLength)
            {
                case 0:
                    MsgInformativo(string.Format("FALTAN ARGUMENTOS"), 2);
                    return -1;

                case 1:
                    MsgInformativo(string.Format("FALTAN ARGUMENTOS"), 2);
                    return -1;

                case 2:
                    MsgInformativo(string.Format("TO DB: {0}",           args[0]), 1);
                    MsgInformativo(string.Format("FROM SCRIPTS: {0}",    args[1]), 1);
                    m = new Migracion(   args[0]
                                       , args[1]
                                       , "");
                    r = m.GenerateMigration();
                    break;

                case 3:
                    MsgInformativo(string.Format("TO DB: {0}",          args[0]), 1);
                    MsgInformativo(string.Format("FROM SCRIPTS: {0}",   args[1]), 1);
                    MsgInformativo(string.Format("PATTERN: {0}",        args[2]), 1);
                    m = new Migracion(   args[0]
                                       , args[1]
                                       , args[2]);
                    r = m.GenerateMigration();
                    break;
            }

            if (!r.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(r.Result);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(r.Result);
            Console.ResetColor();

            return 0;
        }

        public static void MsgInformativo(string mensaje, int tipo)
        {
            switch (tipo)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case 3:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }
    }


    public class Migracion : BaseDBUp
    {
        public Migracion(string con, string path, string pattern) : base(con,
                                                                         path, 
                                                                         pattern
                                                                         ,false
                                                                         ,true
                                                                         ,false
                                                                         ,true
                                                                         , BaseDBUp.DataBaseType.SqlServer) { }
    }

}
