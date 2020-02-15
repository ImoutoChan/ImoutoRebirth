using System;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    public struct Rate
    {
        public int Rating { get; }

        public Rate(int value)
        {
            if (value < 0 || value > 5)
                throw new ArgumentOutOfRangeException(nameof(value));

            Rating = value;
        }
    }
}