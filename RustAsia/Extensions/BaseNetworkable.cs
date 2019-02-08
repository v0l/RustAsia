namespace RustAsia.Extensions
{
    public class BaseNetworkableExt
    {
        public static bool ShouldNetworkToInvisible(BaseNetworkable net, BasePlayer player)
        {
            BasePlayer basePlayer;
            if ((basePlayer = (net as BasePlayer)) == null)
            {
                HeldEntity heldEntity = net as HeldEntity;
                basePlayer = ((heldEntity != null) ? heldEntity.GetOwnerPlayer() : null);
            }
            BasePlayer basePlayer2 = basePlayer;
            return basePlayer2 == null || player == null || basePlayer2 == player || player.IsAdmin || !basePlayer2.IsInvisible;
        }
    }
}
