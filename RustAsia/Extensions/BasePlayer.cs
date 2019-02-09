using Network;
using RustAsia.Admin;
using RustAsia.System;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RustAsia.Extensions
{
    public static class BasePlayerExt
    {
        public static void Disappear(BasePlayer ply)
        {
            if (!ply.IsConnected || ply.IsInvisible || ply.HasPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot))
            {
                return;
            }
            List<Connection> list = new List<Connection>();
            foreach (BasePlayer basePlayer in BasePlayer.activePlayerList)
            {
                if (!(ply == basePlayer) && basePlayer.IsConnected && !basePlayer.IsAdmin)
                {
                    list.Add(basePlayer.net.connection);
                }
            }
            HeldEntity heldEntity = ply.GetHeldEntity();
            if (heldEntity != null)
            {
                heldEntity.SetHeld(false);
                heldEntity.UpdateVisiblity_Invis();
                heldEntity.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            }
            var sv = Network.Net.sv;
            if (sv.write.Start())
            {
                sv.write.PacketID(Message.Type.EntityDestroy);
                sv.write.EntityID(ply.net.ID);
                sv.write.UInt8(0);
                sv.write.Send(new SendInfo(list));
            }
            ply.UpdatePlayerCollider(false);
            CommunityEntity.ServerInstance.ClientRPCEx<string>(new SendInfo
            {
                connection = ply.net.connection
            }, null, "AddUI", "[{\"name\":\"InvisibleIndicator\",\"parent\":\"Hud\",\"components\":[{\"type\":\"UnityEngine.UI.RawImage\",\"url\":\"https://rustasia.com/fa-eye.png\",\"color\":\"1 1 1 0.5\"},{\"type\":\"RectTransform\",\"anchormin\":\"0.175 0.017\",\"anchormax\":\"0.22 0.08\"}]}]");
            ply.IsInvisible = true;
        }

        public static void Reappear(BasePlayer ply)
        {
            if (!ply.IsInvisible)
            {
                return;
            }
            ply.IsInvisible = false;
            ply.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            HeldEntity heldEntity = ply.GetHeldEntity();
            if (heldEntity != null)
            {
                heldEntity.UpdateVisibility_Hand();
                heldEntity.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            }
            ply.UpdatePlayerCollider(true);
            CommunityEntity.ServerInstance.ClientRPCEx<string>(new SendInfo
            {
                connection = ply.net.connection
            }, null, "DestroyUI", "InvisibleIndicator");
        }

        public static void ESPText(BasePlayer adminPly, BasePlayer ply)
        {
            adminPly.SendConsoleCommand("ddraw.text", new object[]
            {
                Variables.ESPTickRate + 0.05f,
                Color.white,
                ply.transform.position + new Vector3(0f, 2f, 0f),
                string.Format("{0} - <color=#ff0000>HP {1}</color> - <color=#ffa500>{2} M</color>{3}", new object[]
                {
                    ply.displayName ?? ply.userID.ToString(),
                    (int)ply.health,
                    Math.Floor((double)Vector3.Distance(ply.transform.position, adminPly.transform.position)),
                    ply.IsInvisible ? " - [INVS]" : string.Empty
                })
            });
        }

        public static void ESPText(BasePlayer adminPly, BuildingPrivlidge bp)
        {
            adminPly.SendConsoleCommand("ddraw.text", new object[]
            {
                Variables.ESPSlowTickRate + 0.05f,
                Color.white,
                bp.transform.position + new Vector3(0f, 2f, 0f),
                string.Format("TC - <color=#ffa500>{0} M</color>{1}", new object[]
                {
                    Math.Floor((double)Vector3.Distance(bp.transform.position, adminPly.transform.position)),
                    bp.isFreeUpkeep ? " - [ADMIN_TC]" : string.Empty
                })
            });
        }

        public static void ESPText(BasePlayer adminPly, StashContainer sc)
        {
            adminPly.SendConsoleCommand("ddraw.text", new object[]
            {
                Variables.ESPSlowTickRate + 0.05f,
                Color.white,
                sc.transform.position,
                string.Format("Stash - <color=#ffa500>{0} M</color> - {1}", new object[]
                {
                    Math.Floor((double)Vector3.Distance(sc.transform.position, adminPly.transform.position)),
                    sc.IsHidden() ? "HIDDEN" : "VISIBLE"
                })
            });
        }

        public static void ESPText(BasePlayer adminPly, BaseAnimalNPC an)
        {
            adminPly.SendConsoleCommand("ddraw.text", new object[]
            {
                Variables.ESPSlowTickRate + 0.05f,
                Color.white,
                an.transform.position,
                string.Format("{0} - <color=#ffa500>{1} M</color> - {2} HP", new object[]
                {
                    an.ShortPrefabName,
                    Math.Floor((double)Vector3.Distance(an.transform.position, adminPly.transform.position)),
                    (int)an.health
                })
            });
        }

        public static void TickESP(BasePlayer ply)
        {
            if (ply.lastESP + Variables.ESPTickRate < (double)Time.time)
            {
                var pf = (ESPFlags)ply.ESPFlags;
                ply.lastESP = Time.time;

                if (pf.HasFlag(ESPFlags.Players))
                {
                    foreach (BasePlayer basePlayer in BasePlayer.activePlayerList)
                    {
                        if (basePlayer != null && basePlayer.IsConnected)
                        {
                            ESPText(ply, basePlayer);
                        }
                    }
                }

                if (pf.HasFlag(ESPFlags.Sleepers))
                {
                    foreach (BasePlayer basePlayer in BasePlayer.sleepingPlayerList)
                    {
                        if (basePlayer != null)
                        {
                            ESPText(ply, basePlayer);
                        }
                    }
                }
            }

            if (ply.lastSlowESP + Variables.ESPSlowTickRate < (double)Time.time)
            {
                var pf = (ESPFlags)ply.ESPFlags;
                ply.lastSlowESP = Time.time;

                if (pf.HasFlag(ESPFlags.Stashes) || pf.HasFlag(ESPFlags.ToolCupboards) || pf.HasFlag(ESPFlags.Animals))
                {
                    foreach (var baseEntity in BaseEntity.serverEntities)
                    {
                        if (baseEntity is BuildingPrivlidge bp && pf.HasFlag(ESPFlags.ToolCupboards))
                        {
                            ESPText(ply, bp);
                        }
                        if (baseEntity is StashContainer sc && pf.HasFlag(ESPFlags.Stashes))
                        {
                            ESPText(ply, sc);
                        }
                        if (baseEntity is BaseAnimalNPC animal && pf.HasFlag(ESPFlags.Animals))
                        {
                            ESPText(ply, animal);
                        }
                    }
                }
            }
        }
    }
}
