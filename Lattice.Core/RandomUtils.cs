using System;
using System.Collections.Generic;
using System.Text;

namespace Lattice.Core
{
    public class RandomUtils
    {
        private static Random _random;

        public static Random Random
        {
            get
            {
                if (_random == null)
                    _random = new Random();

                return _random;
            }
        }

    }
}
