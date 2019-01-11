﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using static vMenuServer.DebugLog;
using static vMenuShared.ConfigManager;

namespace vMenuServer
{

    public static class DebugLog
    {
        public enum LogLevel
        {
            error = 1,
            success = 2,
            info = 4,
            warning = 3,
            none = 0
        }

        /// <summary>
        /// Global log data function, only logs when debugging is enabled.
        /// </summary>
        /// <param name="data"></param>
        public static void Log(dynamic data, LogLevel level = LogLevel.none)
        {
            if (MainServer.DebugMode)
            {
                string prefix = "[vMenu] ";
                if (level == LogLevel.error)
                {
                    prefix = "^1[vMenu] [ERROR]^7 ";
                }
                else if (level == LogLevel.info)
                {
                    prefix = "^5[vMenu] [INFO]^7 ";
                }
                else if (level == LogLevel.success)
                {
                    prefix = "^2[vMenu] [SUCCESS]^7 ";
                }
                else if (level == LogLevel.warning)
                {
                    prefix = "^3[vMenu] [WARNING]^7 ";
                }
                Debug.WriteLine($"{prefix}[DEBUG LOG] {data.ToString()}");
            }
        }
    }

    public class MainServer : BaseScript
    {
        public static bool UpToDate = true;
        // Debug shows more information when doing certain things. Leave it off to improve performance!
        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "server_debug_mode", 0) == "true" ? true : false;

        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        private int currentHours = GetSettingsInt(Setting.vmenu_default_time_hour);
        private int currentMinutes = GetSettingsInt(Setting.vmenu_default_time_min);
        private int minuteClockSpeed = GetSettingsInt(Setting.vmenu_ingame_minute_duration);
        private long minuteTimer = GetGameTimer();
        private long timeSyncCooldown = GetGameTimer();
        private string currentWeather = GetSettingsString(Setting.vmenu_default_weather);
        private bool dynamicWeather = GetSettingsBool(Setting.vmenu_enable_dynamic_weather);
        private bool blackout = false;
        private bool resetBlackout = false;
        private bool freezeTime = GetSettingsBool(Setting.vmenu_freeze_time);
        private int dynamicWeatherMinutes = GetSettingsInt(Setting.vmenu_dynamic_weather_timer);
        private long gameTimer = GetGameTimer();
        private long weatherTimer = GetGameTimer();
        private List<string> CloudTypes = new List<string>()
        {
            "Cloudy 01",
            "RAIN",
            "horizonband1",
            "horizonband2",
            "Puffs",
            "Wispy",
            "Horizon",
            "Stormy 01",
            "Clear 01",
            "Snowy 01",
            "Contrails",
            "altostratus",
            "Nimbus",
            "Cirrus",
            "cirrocumulus",
            "stratoscumulus",
            "horizonband3",
            "Stripey",
            "horsey",
            "shower",
        };
        private List<string> weatherTypes = new List<string>()
        {
            "EXTRASUNNY",
            "CLEAR",
            "NEUTRAL",
            "SMOG",
            "FOGGY",
            "CLOUDS",
            "OVERCAST",
            "CLEARING",
            "RAIN",
            "THUNDER",
            "BLIZZARD",
            "SNOW",
            "SNOWLIGHT",
            "XMAS",
            "HALLOWEEN"
        };

        //public List<string> aceNames = new List<string>()
        //{
        //    // Global
        //    "Everything",
        //    "DontKickMe",
        //    "DontBanMe",
        //    "NoClip",
        //    "Staff",

        //    // Online Players
        //    "OPMenu",
        //    "OPAll",
        //    "OPTeleport",
        //    "OPWaypoint",
        //    "OPSpectate",
        //    "OPIdentifiers",
        //    "OPSummon",
        //    "OPKill",
        //    "OPKick",
        //    "OPPermBan",
        //    "OPTempBan",
        //    "OPUnban",
        //    "OPViewBannedPlayers",

        //    // Player Options
        //    "POMenu",
        //    "POAll",
        //    "POGod",
        //    "POInvisible",
        //    "POFastRun",
        //    "POFastSwim",
        //    "POSuperjump",
        //    "PONoRagdoll",
        //    "PONeverWanted",
        //    "POSetWanted",
        //    "POIgnored",
        //    "POMaxHealth",
        //    "POMaxArmor",
        //    "POCleanPlayer",
        //    "PODryPlayer",
        //    "POWetPlayer",
        //    "POVehicleAutoPilotMenu",
        //    "POFreeze",
        //    "POScenarios",
        //    "POUnlimitedStamina",

        //    // Vehicle Options
        //    "VOMenu",
        //    "VOAll",
        //    "VOGod",
        //    "VOSpecialGod",
        //    "VOKeepClean",
        //    "VORepair",
        //    "VOWash",
        //    "VOEngine",
        //    "VOSpeedLimiter",
        //    "VOChangePlate",
        //    "VOMod",
        //    "VOColors",
        //    "VOLiveries",
        //    "VOComponents",
        //    "VODoors",
        //    "VOWindows",
        //    "VOFreeze",
        //    "VOTorqueMultiplier",
        //    "VOPowerMultiplier",
        //    "VOFlip",
        //    "VOAlarm",
        //    "VOCycleSeats",
        //    "VOEngineAlwaysOn",
        //    "VONoSiren",
        //    "VONoHelmet",
        //    "VOLights",
        //    "VODelete",
        //    "VOUnderglow",
        //    "VOFlashHighbeamsOnHonk",
        //    "VODisableTurbulence",
        //    "VOFlares",
        //    "VOPlaneBombs",

        //    // Vehicle Spawner
        //    "VSMenu",
        //    "VSAll",
        //    "VSDisableReplacePrevious",
        //    "VSSpawnByName",
        //    "VSAddon",
        //    "VSCompacts",
        //    "VSSedans",
        //    "VSSUVs",
        //    "VSCoupes",
        //    "VSMuscle",
        //    "VSSportsClassic",
        //    "VSSports",
        //    "VSSuper",
        //    "VSMotorcycles",
        //    "VSOffRoad",
        //    "VSIndustrial",
        //    "VSUtility",
        //    "VSVans",
        //    "VSCycles",
        //    "VSBoats",
        //    "VSHelicopters",
        //    "VSPlanes",
        //    "VSService",
        //    "VSEmergency",
        //    "VSMilitary",
        //    "VSCommercial",
        //    "VSTrains",

        //    // Saved Vehicles
        //    "SVMenu",
        //    "SVAll",
        //    "SVSpawn",

        //    // Player Appearance
        //    "PAMenu",
        //    "PAAll",
        //    "PACustomize",
        //    "PASpawnSaved",
        //    "PASpawnNew",

        //    // Time Options
        //    "TOMenu",
        //    "TOAll",
        //    "TOFreezeTime",
        //    "TOSetTime",

        //    // Weather Options
        //    "WOMenu",
        //    "WOAll",
        //    "WODynamic",
        //    "WOBlackout",
        //    "WOSetWeather",
        //    "WORemoveClouds",
        //    "WORandomizeClouds",

        //    // Weapon Options
        //    "WPMenu",
        //    "WPAll",
        //    "WPGetAll",
        //    "WPRemoveAll",
        //    "WPUnlimitedAmmo",
        //    "WPNoReload",
        //    "WPSpawn",
        //    "WPSpawnByName",
        //    "WPSetAllAmmo",

        //    // Weapons Permissions
        //    "WPSniperRifle",
        //    "WPFireExtinguisher",
        //    "WPCompactGrenadeLauncher",
        //    "WPSnowball",
        //    "WPVintagePistol",
        //    "WPCombatPDW",
        //    "WPHeavySniperMk2",
        //    "WPHeavySniper",
        //    "WPSweeperShotgun",
        //    "WPMicroSMG",
        //    "WPWrench",
        //    "WPPistol",
        //    "WPPumpShotgun",
        //    "WPAPPistol",
        //    "WPBall",
        //    "WPMolotov",
        //    "WPSMG",
        //    "WPStickyBomb",
        //    "WPPetrolCan",
        //    "WPStunGun",
        //    "WPAssaultRifleMk2",
        //    "WPHeavyShotgun",
        //    "WPMinigun",
        //    "WPGolfClub",
        //    "WPFlareGun",
        //    "WPFlare",
        //    "WPGrenadeLauncherSmoke",
        //    "WPHammer",
        //    "WPCombatPistol",
        //    "WPGusenberg",
        //    "WPCompactRifle",
        //    "WPHomingLauncher",
        //    "WPNightstick",
        //    "WPRailgun",
        //    "WPSawnOffShotgun",
        //    "WPSMGMk2",
        //    "WPBullpupRifle",
        //    "WPFirework",
        //    "WPCombatMG",
        //    "WPCarbineRifle",
        //    "WPCrowbar",
        //    "WPFlashlight",
        //    "WPDagger",
        //    "WPGrenade",
        //    "WPPoolCue",
        //    "WPBat",
        //    "WPPistol50",
        //    "WPKnife",
        //    "WPMG",
        //    "WPBullpupShotgun",
        //    "WPBZGas",
        //    "WPUnarmed",
        //    "WPGrenadeLauncher",
        //    "WPNightVision",
        //    "WPMusket",
        //    "WPProximityMine",
        //    "WPAdvancedRifle",
        //    "WPRPG",
        //    "WPPipeBomb",
        //    "WPMiniSMG",
        //    "WPSNSPistol",
        //    "WPPistolMk2",
        //    "WPAssaultRifle",
        //    "WPSpecialCarbine",
        //    "WPRevolver",
        //    "WPMarksmanRifle",
        //    "WPBattleAxe",
        //    "WPHeavyPistol",
        //    "WPKnuckleDuster",
        //    "WPMachinePistol",
        //    "WPCombatMGMk2",
        //    "WPMarksmanPistol",
        //    "WPMachete",
        //    "WPSwitchBlade",
        //    "WPAssaultShotgun",
        //    "WPDoubleBarrelShotgun",
        //    "WPAssaultSMG",
        //    "WPHatchet",
        //    "WPBottle",
        //    "WPCarbineRifleMk2",
        //    "WPParachute",
        //    "WPSmokeGrenade",

        //    // Weapon Loadouts Menu
        //    "WLMenu",
        //    "WLAll",
        //    "WLEquip",
        //    "WLEquipOnRespawn",

        //    // Misc Settings
        //    "MSAll",
        //    "MSClearArea",
        //    "MSTeleportToWp",
        //    "MSShowCoordinates",
        //    "MSShowLocation",
        //    "MSJoinQuitNotifs",
        //    "MSDeathNotifs",
        //    "MSNightVision",
        //    "MSThermalVision",
        //    "MSLocationBlips",
        //    "MSPlayerBlips",
        //    "MSTeleportLocations",
        //    "MSConnectionMenu",
        //    "MSRestoreAppearance",
        //    "MSRestoreWeapons",
        //    "MSDriftMode",

        //    // Voice Chat
        //    "VCMenu",
        //    "VCAll",
        //    "VCEnable",
        //    "VCShowSpeaker",
        //    "VCStaffChannel",
        //};

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainServer()
        {
 
            // Add event handlers.
            EventHandlers.Add("vMenu:SummonPlayer", new Action<Player, int>(SummonPlayer));
            EventHandlers.Add("vMenu:KillPlayer", new Action<Player, int>(KillPlayer));
            EventHandlers.Add("vMenu:KickPlayer", new Action<Player, int, string>(KickPlayer));
            EventHandlers.Add("vMenu:RequestPermissions", new Action<Player>(vMenuShared.PermissionsManager.SetPermissionsForPlayer));
            EventHandlers.Add("vMenu:UpdateServerWeather", new Action<string, bool, bool>(UpdateWeather));
            EventHandlers.Add("vMenu:UpdateServerWeatherCloudsType", new Action<bool>(UpdateWeatherCloudsType));
            EventHandlers.Add("vMenu:UpdateServerTime", new Action<int, int, bool>(UpdateTime));
            EventHandlers.Add("vMenu:DisconnectSelf", new Action<Player>(DisconnectSource));
            EventHandlers.Add("vMenu:ClearArea", new Action<float, float, float>(ClearAreaNearPos));
            EventHandlers.Add("vMenu:GetPlayerIdentifiers", new Action<int, NetworkCallbackDelegate>((TargetPlayer, CallbackFunction) => { CallbackFunction(JsonConvert.SerializeObject(Players[TargetPlayer].Identifiers)); }));

            string addons = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
            try
            {
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(addons);
                // If the above crashes, then the json is invalid and it'll throw warnings in the console.
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine($"\n\n^1[vMenu] [ERROR] ^7Your addons.json file contains a problem! Error details: {ex.Message}\n\n");
            }


            dynamicWeather = GetSettingsBool(Setting.vmenu_enable_dynamic_weather);
            if (GetSettingsInt(Setting.vmenu_dynamic_weather_timer) != -1)
            {
                dynamicWeatherMinutes = GetSettingsInt(Setting.vmenu_dynamic_weather_timer);
            }

            string defaultWeather = GetSettingsString(Setting.vmenu_default_weather);

            if (!string.IsNullOrEmpty(defaultWeather))
            {
                if (weatherTypes.Contains(defaultWeather))
                {
                    currentWeather = defaultWeather;
                }
            }

            currentHours = GetSettingsInt(Setting.vmenu_default_time_hour);
            currentHours = (currentHours >= 0 && currentHours < 24) ? currentHours : 9;
            currentMinutes = GetSettingsInt(Setting.vmenu_default_time_min);
            currentMinutes = (currentMinutes >= 0 && currentMinutes < 60) ? currentMinutes : 0;

            minuteClockSpeed = GetSettingsInt(Setting.vmenu_ingame_minute_duration);
            minuteClockSpeed = (minuteClockSpeed > 0) ? minuteClockSpeed : 2000;

            Tick += WeatherLoop;
            Tick += TimeLoop;
        }
        #endregion

        /// <summary>
        /// Clear the area near this point for all players.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void ClearAreaNearPos(float x, float y, float z)
        {
            TriggerClientEvent("vMenu:ClearArea", x, y, z);
        }


        /// <summary>
        /// Disconnect the source player because they used the disconnect menu button.
        /// </summary>
        /// <param name="src"></param>
        private void DisconnectSource([FromSource] Player src)
        {
            src.Drop("You disconnected yourself.");
        }
        #region Manage weather and time changes.
        /// <summary>
        /// Loop used for syncing and keeping track of the time in-game.
        /// </summary>
        /// <returns></returns>
        private async Task TimeLoop()
        {
            if (GetSettingsBool(Setting.vmenu_enable_time_sync))
            {
                if (minuteClockSpeed > 2000)
                {
                    await Delay(2000);
                }
                else
                {
                    await Delay(minuteClockSpeed);
                }
                if (!freezeTime)
                {
                    // only add a minute if the timer has reached the configured duration (2000ms (2s) by default).
                    if (GetGameTimer() - minuteTimer > minuteClockSpeed)
                    {
                        currentMinutes++;
                        minuteTimer = GetGameTimer();
                    }

                    if (currentMinutes > 59)
                    {
                        currentMinutes = 0;
                        currentHours++;
                    }
                    if (currentHours > 23)
                    {
                        currentHours = 0;
                    }
                }

                if (GetGameTimer() - timeSyncCooldown > 6000)
                {
                    TriggerClientEvent("vMenu:SetTime", currentHours, currentMinutes, freezeTime);
                    timeSyncCooldown = GetGameTimer();
                }
            }

        }

        /// <summary>
        /// Task used for syncing and changing weather dynamically.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherLoop()
        {
            await Delay(5000);

            if (GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                if (dynamicWeather)
                {
                    if (resetBlackout && GetGameTimer() - weatherTimer > 60000) // if 1 minute has passed since last change, and resetblackout is true, disable blackout and reset it.
                    {
                        resetBlackout = false;
                        blackout = false;
                    }
                    if (GetGameTimer() - weatherTimer > (dynamicWeatherMinutes * 60000))
                    {
                        RefreshWeather();
                        if (DebugMode)
                        {
                            long gameTimer2 = GetGameTimer();
                            Log($"Changing weather, last weather duration: {(int)((GetGameTimer() - weatherTimer) / 60000)} minutes. New Weather Type: {currentWeather}");
                            gameTimer = gameTimer2;
                        }
                        weatherTimer = GetGameTimer();
                    }
                }
                else
                {
                    weatherTimer = GetGameTimer();
                }
                if (GetSettingsBool(Setting.vmenu_allow_random_blackout) && (currentWeather == "THUNDER" || currentWeather == "HALLOWHEEN") && new Random().Next(5) == 1 && !blackout && !resetBlackout)
                {
                    blackout = true;
                    resetBlackout = true;
                }
                if (blackout == false && resetBlackout)
                {
                    resetBlackout = false;
                }
                TriggerClientEvent("vMenu:SetWeather", currentWeather, blackout, dynamicWeather);
            }
        }

        /// <summary>
        /// Select a new random weather type, based on the current weather and some patterns.
        /// </summary>
        private void RefreshWeather()
        {
            var random = new Random().Next(20);
            if (currentWeather == "RAIN" || currentWeather == "THUNDER")
            {
                currentWeather = "CLEARING";
            }
            else if (currentWeather == "CLEARING")
            {
                currentWeather = "CLOUDS";
            }
            else
            {
                switch (random)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        currentWeather = (currentWeather == "EXTRASUNNY" ? "CLEAR" : "EXTRASUNNY");
                        break;
                    case 6:
                    case 7:
                    case 8:
                        currentWeather = (currentWeather == "SMOG" ? "FOGGY" : "SMOG");
                        break;
                    case 9:
                    case 10:
                    case 11:
                        currentWeather = (currentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS");
                        break;
                    case 12:
                    case 13:
                    case 14:
                        currentWeather = (currentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS");
                        break;
                    case 15:
                        currentWeather = (currentWeather == "OVERCAST" ? "THUNDER" : "OVERCAST");
                        break;
                    case 16:
                        currentWeather = (currentWeather == "CLOUDS" ? "EXTRASUNNY" : "RAIN");
                        break;
                    case 17:
                    case 18:
                    case 19:
                    default:
                        currentWeather = (currentWeather == "FOGGY" ? "SMOG" : "FOGGY");
                        break;
                }
            }

        }
        #endregion

        #region Sync weather & time with clients
        /// <summary>
        /// Update the weather for all clients.
        /// </summary>
        /// <param name="newWeather"></param>
        /// <param name="blackoutNew"></param>
        /// <param name="dynamicWeatherNew"></param>
        private void UpdateWeather(string newWeather, bool blackoutNew, bool dynamicWeatherNew)
        {
            currentWeather = newWeather;
            blackout = blackoutNew;
            dynamicWeather = dynamicWeatherNew;
            TriggerClientEvent("vMenu:SetWeather", currentWeather, blackout, dynamicWeather);
        }

        /// <summary>
        /// Set a new random clouds type and opacity for all clients.
        /// </summary>
        /// <param name="removeClouds"></param>
        private void UpdateWeatherCloudsType(bool removeClouds)
        {
            if (removeClouds)
            {
                TriggerClientEvent("vMenu:SetClouds", 0f, "removed");
            }
            else
            {
                float opacity = float.Parse(new Random().NextDouble().ToString());
                string type = CloudTypes[new Random().Next(0, CloudTypes.Count)];
                TriggerClientEvent("vMenu:SetClouds", opacity, type);
            }
        }

        /// <summary>
        /// Set and sync the time to all clients.
        /// </summary>
        /// <param name="newHours"></param>
        /// <param name="newMinutes"></param>
        /// <param name="freezeTimeNew"></param>
        private void UpdateTime(int newHours, int newMinutes, bool freezeTimeNew)
        {
            currentHours = newHours;
            currentMinutes = newMinutes;
            freezeTime = freezeTimeNew;
            TriggerClientEvent("vMenu:SetTime", currentHours, currentMinutes, freezeTime);
        }
        #endregion

        #region Online Players Menu Actions
        /// <summary>
        /// Kick a specific player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="kickReason"></param>
        private void KickPlayer([FromSource] Player source, int target, string kickReason = "You have been kicked from the server.")
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Kick") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                // If the player is allowed to be kicked.
                Player targetPlayer = Players[target];
                if (targetPlayer != null)
                {
                    if (!IsPlayerAceAllowed(targetPlayer.Handle, "vMenu.DontKickMe"))
                    {
                        TriggerEvent("vMenu:KickSuccessful", source.Name, kickReason, targetPlayer.Name);

                        KickLog($"Player: {source.Name} has kicked: {targetPlayer.Name} for: {kickReason}.");
                        TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: $"The target player (<C>{targetPlayer.Name}</C>) has been kicked.");

                        // Kick the player from the server using the specified reason.
                        DropPlayer(targetPlayer.Handle, kickReason);
                        return;
                    }
                    // Trigger the client event on the source player to let them know that kicking this player is not allowed.
                    TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "Sorry, this player can ~r~not ~w~be kicked.");
                    return;
                }
                TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "An unknown error occurred. Report it here: vespura.com/vmenu");
            }
            else
            {
                BanManager.BanCheater(source);
            }
        }

        /// <summary>
        /// Kill a specific player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void KillPlayer([FromSource] Player source, int target)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Kill") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Player targetPlayer = Players[target];
                if (targetPlayer != null)
                {
                    // Trigger the client event on the target player to make them kill themselves. R.I.P.
                    TriggerClientEvent(player: targetPlayer, eventName: "vMenu:KillMe", args: source.Name);
                    return;
                }
                TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "An unknown error occurred. Report it here: vespura.com/vmenu");
            }
            else
            {
                BanManager.BanCheater(source);
            }
        }

        /// <summary>
        /// Teleport a specific player to another player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void SummonPlayer([FromSource] Player source, int target)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Summon") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                // Trigger the client event on the target player to make them teleport to the source player.
                Player targetPlayer = Players[target];
                if (targetPlayer != null)
                {
                    TriggerClientEvent(player: targetPlayer, eventName: "vMenu:GoToPlayer", args: source.Handle);
                    return;
                }
                TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "An unknown error occurred. Report it here: vespura.com/vmenu");
            }
            else
            {
                BanManager.BanCheater(source);
            }
        }
        #endregion


        /// <summary>
        /// If enabled using convars, will log all kick actions to the server console as well as an external file.
        /// </summary>
        /// <param name="kickLogMesage"></param>
        private static void KickLog(string kickLogMesage)
        {
            //if (GetConvar("vMenuLogKickActions", "true") == "true")
            if (GetSettingsBool(Setting.vmenu_log_kick_actions))
            {
                string file = LoadResourceFile(GetCurrentResourceName(), "vmenu.log") ?? "";
                DateTime date = DateTime.Now;
                string formattedDate = (date.Day < 10 ? "0" : "") + date.Day + "-" +
                    (date.Month < 10 ? "0" : "") + date.Month + "-" +
                    (date.Year < 10 ? "0" : "") + date.Year + " " +
                    (date.Hour < 10 ? "0" : "") + date.Hour + ":" +
                    (date.Minute < 10 ? "0" : "") + date.Minute + ":" +
                    (date.Second < 10 ? "0" : "") + date.Second;
                string outputFile = file + $"[\t{formattedDate}\t] [KICK ACTION] {kickLogMesage}\n";
                SaveResourceFile(GetCurrentResourceName(), "vmenu.log", outputFile, -1);
                Debug.WriteLine("^3[vMenu] [KICK]^7 " + kickLogMesage + "\n");
            }
        }
    }
}
