using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAxisInput 
{
   IObservable<Vector3> AxisInput { get; }


}
