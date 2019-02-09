Features
===

 * AdminTC - TC's set with `ent admin_tc` will not consume resources from the container.
 * Incognito Admin Chat - Admin's have the same chat color as normal players.
 * No Give Messages - `inventory.give` and others do not send broadcast messages in chat.
 * Admin No Target - Admins are not targeted by traps (Landmine, AutoTurret, Shotgun Trap, Flame Turret) or heli.
 * Admin ESP - Admins can enable ESP to see where all players are in-game.
 * Admin FreeBuild - Admins can build with not cost.
 * Admin Instant Upgrade - Admins can build with a different building grade (Metal is default).
 * Admin Free Upgrade - Admins can upgrade buildings with no cost.
 * Invisible Admin - Admins can go invisible (only other admins can see them).
 * Un-Lootable Admins - Admins cant be looted while wounded or sleeping, admin corpses are empty.
 
Command List
===
```
ent admin_tc
global.freebuild
global.invisible
global.esp
global.admin_no_target
global.default_building_grade
global.lootable_admins
```

How to apply the mod
===

*Step 1:* Add fields to rust classes.

BasePlayer:
```csharp
public bool IsInvisible { get; set; }
public bool ESPOn { get; set; }
public float lastESP { get; set; }
public BuildingGrade.Enum FreeBuildType { get; set; } = BuildingGrade.Enum.Metal;
```

BuildingPrivlidge:
```csharp
public bool isFreeUpkeep { get; set; }
```

*Step 2:* Save the assembly.
*Step 3:* Compile the RustAsia assembly.
*Step 4:* Add the following hooks with dnSpy

ConVar.Admin:
```csharp
if (@string == "admin_tc")
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
	return;
}
```

ConVar.Chat:
`Remove admin chat color from say()`

ConVar.Inventory:
`Remove all chat broadcasts for give()/giveto()/giveid()/givearm()`

AutoTurret:
```csharp
//Ignore()
return player.IsAdmin && RustAsia.System.Variables.AdminNoTarget;

//IsEntityHostile()
global::BasePlayer basePlayer = ent as global::BasePlayer;
return (!basePlayer || !basePlayer.IsAdmin || !RustAsia.AdminMods.AdminNoTarget) && ent.IsHostile();

//OnAttacked(), edit basePlayer checks like so.
if (!basePlayer || ((!basePlayer.IsAdmin || !RustAsia.System.Variables.AdminNoTarget) && !this.IsAuthed(basePlayer)))
```

BaseNetworkable:
```csharp
//The following edit should be done (as IL edit)
/*
 * ldarg.0
 * ldarg.1
 * call RustAsia.Extensions.BaseNetworkableExt.ShouldNetworkToInvisible
 * ret
*/
public virtual bool ShouldNetworkTo(global::BasePlayer player)
{
	return RustAsia.Extensions.BaseNetworkableExt.ShouldNetworkToInvisible(this, player);
}
```

BasePlayer:
```csharp
//CanBeLooted()
&& (!this.IsAdmin || !RustAsia.System.Variables.LootableAdmins)

//CreateCorpse()
if (!this.IsAdmin || RustAsia.System.Variables.LootableAdmins) { /*lootableCorpse.TakeFrom...*/ } 

//Hurt()
if (this.IsAdmin && !RustAsia.System.Variables.LootableAdmins && (this.IsSleeping() || this.IsWounded()))
{
	return;
}

//OnReceiveTick(PlayerTick msg, bool wasPlayerStalled) (as IL edit)
/*
 * ldarg.0
 * call RustAsia.Extensions.BasePlayerExt.TickESP
*/
RustAsia.Extensions.BasePlayerExt.TickESP(this);
```

Bootstrap:
```csharp
//StartServer() - before "Server startup complete" log
RustAsia.System.Core.Init(new Action<string>(Bootstrap.WriteToLog));
```

BuildingBlock:
```csharp
//CanAffordUpgrade()
if (player.IsAdmin && RustAsia.System.Variables.FreeBuild)
{
	return true;
}

//PayForUpgrade()
if (player.IsAdmin && RustAsia.System.Variables.FreeBuild)
{
	return;
}
```

BuildingPrivlidge:
```csharp
//ApplyUpkeepPayment()
if (this.isFreeUpkeep)
{
	return;
}
```

FlameTurret:
```csharp
//CheckTrigger()
&& (!component.IsAdmin || !RustAsia.System.Variables.AdminNoTarget)
```

GunTrap:
```csharp
//CheckTrigger()
&& (!component.IsAdmin || !RustAsia.System.Variables.AdminNoTarget)
```

Landmine:
```csharp
public void Trigger(BasePlayer ply = null)
{
	if (ply && (!ply.IsAdmin || !RustAsia.System.Variables.AdminNoTarget))
	{
		this.triggerPlayerID = ply.userID;
	}
	if (!ply || !ply.IsAdmin || !RustAsia.System.Variables.AdminNoTarget)
	{
		base.SetFlag(BaseEntity.Flags.Open, true, false, true);
		base.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
	}
}
```

PatrolHelicopterAI:
```csharp
//UpdateTargetList()
&& (!basePlayer.IsAdmin || !RustAsia.System.Variables.AdminNoTarget)

//ValidStrafeTarget()
return (!ply.IsAdmin || !RustAsia.System.Variables.AdminNoTarget) && !ply.IsNearEnemyBase();

//WasAttacked()
if (basePlayer != null && (!basePlayer.IsAdmin || !RustAsia.System.Variables.AdminNoTarget))
```

Planner:
```csharp
//CanAffordToPlace()
if (ownerPlayer.IsAdmin && RustAsia.System.Variables.FreeBuild)
{
	return true;
}

//DoPlacement()
if (RustAsia.System.Variables.FreeBuild && ownerPlayer.IsAdmin)
{
	buildingBlock.SetGrade(ownerPlayer.FreeBuildType);
}
else
{
	buildingBlock.SetGrade(buildingBlock.blockDefinition.defaultGrade.gradeBase.type);
}

//PayForPlacement()
if (player.IsAdmin && RustAsia.System.Variables.FreeBuild)
{
	return;
}
```

*Step 5:* Re-compile HTNPlayer

*Step 6:* Save the assembly, you're done!