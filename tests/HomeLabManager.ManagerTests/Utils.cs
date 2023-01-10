using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using HomeLabManager.Manager;

namespace HomeLabManager.ManagerTests
{
    internal static class Utils
    {
        public static void RegisterTestServices()
        {
            Program.BuildTestApp();
        }
    }
}
