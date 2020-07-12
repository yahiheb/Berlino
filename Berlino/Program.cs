using System;

namespace Berlino
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
			var p = new DescriptorParser();
			p.Parse("pk(sh)");
        }
    }
}
