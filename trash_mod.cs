using System;
using System.Collections.Generic;
using Facepunch.Extend;
using Network;
using UnityEngine;

namespace Trash
{
	// Token: 0x02000B97 RID: 2967
	public static class TrashMod
	{
		// Token: 0x06003FAE RID: 16302 RVA: 0x0012C47C File Offset: 0x0012A67C
		public static void Disappear(this BasePlayer ply)
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
			Server sv = Net.sv;
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
			}, null, "AddUI", "[{\"name\":\"InvisibleIndicator\",\"parent\":\"Overlay\",\"components\":[{\"type\":\"UnityEngine.UI.RawImage\",\"url\":\"https://rustasia.com/fa-eye.png\"},{\"type\":\"RectTransform\",\"anchormin\":\"0 0\",\"anchormax\":\"1 1\"}]}]");
			ply.IsInvisible = true;
		}

		// Token: 0x06003FAF RID: 16303 RVA: 0x0012C5CC File Offset: 0x0012A7CC
		public static void Reappear(this BasePlayer ply)
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

		// Token: 0x06003FB0 RID: 16304 RVA: 0x0012C648 File Offset: 0x0012A848
		public static void ESPText(this BasePlayer ply, BasePlayer adminPly)
		{
			adminPly.SendConsoleCommand("ddraw.text", new object[]
			{
				1f,
				Color.white,
				ply.transform.position + new Vector3(0f, 2f, 0f),
				string.Format("{0} - <color=#ff0000>HP {1}</color> - <color=#ffa500>{2} M</color>{3}", new object[]
				{
					ply.displayName ?? ply.userID.ToString(),
					(int)ply.health,
					Math.Floor((double)Vector3.Distance(ply.transform.position, adminPly.transform.position)),
					ply.IsInvisible ? "Invisible" : string.Empty
				})
			});
		}

		// Token: 0x06003FB1 RID: 16305
		public static void TickESP(BasePlayer ply)
		{
			if (ply.ESPOn && ply.lastESP + 1.0 < (double)Time.time)
			{
				ply.lastESP = (double)Time.time;
				foreach (BasePlayer basePlayer in BasePlayer.activePlayerList)
				{
					if (basePlayer != null && basePlayer.IsConnected)
					{
						basePlayer.ESPText(ply);
					}
				}
			}
		}

		// Token: 0x06003FB2 RID: 16306 RVA: 0x0012C7B8 File Offset: 0x0012A9B8
		public static bool ShouldNetworkToTrash(BaseNetworkable net, BasePlayer player)
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

		// Token: 0x06003FB3 RID: 16307 RVA: 0x0012C820 File Offset: 0x0012AA20
		public static void ESPCommand(ConsoleSystem.Arg arg)
		{
			BasePlayer basePlayer = arg.Player();
			basePlayer.ESPOn = !basePlayer.ESPOn;
			basePlayer.ChatMessage(string.Format("ESP {0}", basePlayer.ESPOn ? "on" : "off"));
		}

		// Token: 0x06003FB4 RID: 16308 RVA: 0x0012C868 File Offset: 0x0012AA68
		public static void AdminTCCommand(ConsoleSystem.Arg arg, BaseEntity baseEntity)
		{
			BuildingPrivlidge buildingPrivlidge = baseEntity as BuildingPrivlidge;
			if (buildingPrivlidge != null)
			{
				buildingPrivlidge.isFreeUpkeep = !buildingPrivlidge.isFreeUpkeep;
				buildingPrivlidge.AddDelayedUpdate();
				arg.ReplyWith(string.Format("{0}: {1}", baseEntity.net.ID, buildingPrivlidge.isFreeUpkeep ? "True" : "False"));
				return;
			}
			arg.ReplyWith("Not a tool cupboard");
		}

		// Token: 0x04002F76 RID: 12150
		public static bool FreeBuild;

		// Token: 0x02000B98 RID: 2968
		public static class Commands
		{
			// Token: 0x170005F6 RID: 1526
			// (get) Token: 0x06003FB5 RID: 16309 RVA: 0x0012C8DC File Offset: 0x0012AADC
			public static ConsoleSystem.Command FreebuildCommand
			{
				get
				{
					ConsoleSystem.Command command = new ConsoleSystem.Command();
					command.Name = "freebuild";
					command.Parent = "global";
					command.FullName = "global.freebuild";
					command.ServerAdmin = true;
					command.Variable = true;
					command.GetOveride = (() => TrashMod.FreeBuild.ToString());
					command.SetOveride = delegate(string str)
					{
						TrashMod.FreeBuild = str.ToBool();
					};
					return command;
				}
			}

			// Token: 0x170005F7 RID: 1527
			// (get) Token: 0x06003FB6 RID: 16310 RVA: 0x0012C968 File Offset: 0x0012AB68
			public static ConsoleSystem.Command InvisibleCommand
			{
				get
				{
					ConsoleSystem.Command command = new ConsoleSystem.Command();
					command.Name = "invisible";
					command.Parent = "global";
					command.FullName = "global.invisible";
					command.ServerAdmin = true;
					command.Variable = false;
					command.Call = delegate(ConsoleSystem.Arg arg)
					{
						BasePlayer basePlayer = arg.Player();
						if (basePlayer != null)
						{
							if (!basePlayer.IsInvisible)
							{
								basePlayer.Disappear();
							}
							else
							{
								basePlayer.Reappear();
							}
							arg.ReplyWith(string.Format("You are now: {0}", basePlayer.IsInvisible ? "Invisible" : "Visible"));
						}
					};
					return command;
				}
			}

			// Token: 0x170005F8 RID: 1528
			// (get) Token: 0x06003FB7 RID: 16311 RVA: 0x0012C9D0 File Offset: 0x0012ABD0
			public static ConsoleSystem.Command AdminESPCommand
			{
				get
				{
					ConsoleSystem.Command command = new ConsoleSystem.Command();
					command.Name = "esp";
					command.Parent = "global";
					command.FullName = "global.esp";
					command.ServerAdmin = true;
					command.Variable = false;
					command.Call = delegate(ConsoleSystem.Arg arg)
					{
						TrashMod.ESPCommand(arg);
					};
					return command;
				}
			}
		}
	}
}
