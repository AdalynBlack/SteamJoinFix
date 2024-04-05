using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace SteamJoinFix.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatches
{
	[HarmonyDebug]
	[HarmonyPatch("SendNewPlayerValuesServerRpc")]
	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> SyncUnlockablesCrashPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		return new CodeMatcher(instructions, generator)
			.MatchForward(false,
					new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(NetworkManager), "DisconnectClient", parameters: new Type[] {typeof(ulong)})))
			.InsertAndAdvance(
					new CodeInstruction(OpCodes.Ldarg_0), // this
					new CodeInstruction(OpCodes.Ldarg_1), // newPlayerSteamId
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerControllerBPatches), "WaitForSteamID")))
			.InstructionEnumeration();
	}

	public static void WaitForSteamID(PlayerControllerB player, ulong newPlayerSteamId)
	{
		player.StartCoroutine(WaitForSteamIDCoroutine(player, newPlayerSteamId));
	}

	private static IEnumerator WaitForSteamIDCoroutine(PlayerControllerB player, ulong newPlayerSteamId)
	{
		float timer = 0f;
		while(timer < 10f)
		{
			timer += Time.deltaTime;
			if (GameNetworkManager.Instance.steamIdsInLobby.Contains(newPlayerSteamId))
			{
				AccessTools.Method(typeof(PlayerControllerB), "SendNewPlayerValuesServerRpc").Invoke(player, new object[] {(object) newPlayerSteamId});
				yield break;
			}
			NetworkManager.Singleton.DisconnectClient(player.actualClientId);
			yield return null;
		}
	}
}
