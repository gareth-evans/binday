using System;

namespace BinDay
{
    public interface IDateProvider
    {
        DateTime Today { get; }
    }
}