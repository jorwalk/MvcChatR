using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcChatR.Controllers
{
    public class DemoAsyncAwaitController : Controller
    {
        //
        // GET: /DemoAsyncAwait/
        // The above behavior can be also achieved by using Task.Wait or Task.ContinueWith, so how do they differ? 
        // I am leaving this question as a home work for you.
        public static void Index(string[] args)
        {
            Method();
        }

        public static async void Method()
        {
            await Task.Run(new Action(LongTask));
            Console.WriteLine("New Thread");
        }

        public static void LongTask()
        {
            Thread.Sleep(10000);
        }

    }
}
