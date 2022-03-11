using UnityEngine;

namespace Game
{
  public static class PlayerManager
  {
    public static Transform PlayerKeyHolder { get; set; }
    public static float KeySpeed { get; internal set; }
    public static Vector3 PlayerVelocity { get; set; }
    public const int PlayerLayer = 3;
    public static float NormalizeF(float val, float min, float max)
    {
      return (val - min) / (max - min);
    }
  }
}