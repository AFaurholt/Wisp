using UnityEngine;
using UnityEngine.Events;

namespace Game
{
  public static class PlayerManager
  {
    public static float VisibleRadius { get; set; }
    public static UnityEvent<bool> ZipHandler { get; set; }
    public static UnityEvent RespawnHandler { get; set; }
    public static UnityEvent<int> CollectibleHandler { get; set; }
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

    public static void SubscribeZip(UnityAction<bool> ua)
    {
      if (ZipHandler == null)
        ZipHandler = new UnityEvent<bool>();

      ZipHandler.AddListener(ua);
    }
    public static void SubscribeRespawn(UnityAction ua)
    {
      if (RespawnHandler == null)
        RespawnHandler = new UnityEvent();

      RespawnHandler.AddListener(ua);
    }
    public static void SubscribeCollectible(UnityAction<int> ua)
    {
      if (CollectibleHandler == null)
        CollectibleHandler = new UnityEvent<int>();

      CollectibleHandler.AddListener(ua);
    }
  }
}