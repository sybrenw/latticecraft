using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lattice.Core
{
    public class AppLogging
    {
        private static ILoggerFactory _factory = null;
        
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = new LoggerFactory();
                }
                return _factory;
            }
            set { _factory = value; }
        }

        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}
