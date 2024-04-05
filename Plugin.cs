using BepInEx;
using HarmonyLib;
using SteamJoinFix.Patches;

namespace SteamJoinFix;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	private void Awake()
	{
	    Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

	    Harmony.CreateAndPatchAll(typeof(PlayerControllerBPatches));
	}
}
