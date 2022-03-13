using UnityEngine;

namespace Game
{
  public static class PlayerManager
  {
    public static Transform PlayerKeyHolder { get; set; }
    public static KeyBehavior CurrentKey { get; set; }
    public static float KeySpeed { get; internal set; }
    public static Vector3 PlayerVelocity { get; set; }
    public const int PlayerPickupLayer = 7;
    public const int DeadLayer = 6;
    public const int PlayerLayer = 3;
    public const int DefaultLayer = 1;
    public static float NormalizeF(float val, float min, float max)
    {
      return (val - min) / (max - min);
    }
    public static Transform CurrentRespawn { get; set; }
    public static PlayerBehavior CurrentPlayer { get; internal set; }
  }
}