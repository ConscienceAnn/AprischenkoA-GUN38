using System;
using Zenject;

namespace Models.Interfaces
{
    public interface ITimeModel  :  IInitializable
    {
        IObservable<int> GameTime { get; }
    }
}