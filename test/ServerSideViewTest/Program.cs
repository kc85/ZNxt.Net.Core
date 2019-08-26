using System;
using ZNxt.Net.Core.Web.ContentHandler;

namespace ServerSideViewTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var rv = new RazorTemplateEngine();
            var data = rv.Compile($"Hello -- {DateTime.Now.ToString()} @Raw((1+1).ToString())", "aa", null);

            Console.WriteLine("Hello World!" + data);
            Console.Read();
        }
    }
}
