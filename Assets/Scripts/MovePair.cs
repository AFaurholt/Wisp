using System;
using UnityEngine;

namespace Game
{
  [Serializable]
  public class MovePair
  {
    public Transform from;
    public Transform to;
    public float time;
  }
}