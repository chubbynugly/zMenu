using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public static class CommonFunctions
    {
        #region Variables
        private static string _currentScenario = "";
        private static Vehicle _previousVehicle;

        public static bool DriveToWpTaskActive = false;
        public static bool DriveWanderTaskActive = false;
        #endregion

        #region some misc functions copied from base script
        /// <summary>
        /// Copy of <see cref="BaseScript.TriggerServerEvent(string, object[])"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public static void TriggerServerEvent(string eventName, params object[] args)
        {
            BaseScript.TriggerServerEvent(eventName, args);
        }

        /// <summary>
        /// Copy of <see cref="BaseScript.TriggerEvent(string, object[])"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public static void TriggerEvent(string eventName, params object[] args)
        {
            BaseScript.TriggerEvent(eventName, args);
        }

        /// <summary>
        /// Copy of <see cref="BaseScript.Delay(int)"/>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static async Task Delay(int time)
        {
            await BaseScript.Delay(time);
        }
        #endregion

        #region menu position
        public static bool RightAlignMenus() => UserDefaults.MiscRightAlignMenu;
        #endregion

        #region Get Localized Label Text
        /// <summary>
        /// Get the localized name from a text label (for classes that don't have BaseScript)
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static string GetLocalizedName(string label) => GetLabelText(label);
        #endregion

        #region Get Localized Vehicle Display Name
        /// <summary>
        /// Get the localized model name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetVehDisplayNameFromModel(string name) => GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(name)));
        #endregion

        #region DoesModelExist
        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelName">The model name</param>
        /// <returns></returns>
        public static bool DoesModelExist(string modelName) => DoesModelExist((uint)GetHashKey(modelName));

        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelHash">The model hash</param>
        /// <returns></returns>
        public static bool DoesModelExist(uint modelHash) => IsModelInCdimage(modelHash);
        #endregion

        #region GetVehicle from specified player id (if not specified, return the vehicle of the current player)
        /// <summary>
        /// Returns the current or last vehicle of the current player.
        /// </summary>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return Game.PlayerPed.LastVehicle;
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    return Game.PlayerPed.CurrentVehicle;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the current or last vehicle of the selected ped.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(Ped ped, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return ped.LastVehicle;
            }
            else
            {
                if (ped.IsInVehicle())
                {
                    return ped.CurrentVehicle;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the current or last vehicle of the selected player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="lastVehicle"></param>
        /// <returns></returns>
        public static Vehicle GetVehicle(Player player, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return player.Character.LastVehicle;
            }
            else
            {
                if (player.Character.IsInVehicle())
                {
                    return player.Character.CurrentVehicle;
                }
            }
            return null;
        }
        #endregion

        #region GetVehicleModel (uint)(hash) from Entity/Vehicle (int)
        /// <summary>
        /// Get the vehicle model hash (as uint) from the specified (int) entity/vehicle.
        /// </summary>
        /// <param name="vehicle">Entity/vehicle.</param>
        /// <returns>Returns the (uint) model hash from a (vehicle) entity.</returns>
        public static uint GetVehicleModel(int vehicle) => (uint)GetHashKey(GetEntityModel(vehicle).ToString());
        #endregion

        #region Drive Tasks (WIP)
        /// <summary>
        /// Drives to waypoint
        /// </summary>
        public static void DriveToWp(int style = 0)
        {

            ClearPedTasks(Game.PlayerPed.Handle);
            DriveWanderTaskActive = false;
            DriveToWpTaskActive = true;

            Vector3 waypoint = World.WaypointPosition;

            Vehicle veh = GetVehicle();
            uint model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveToCoordLongrange(Game.PlayerPed.Handle, veh.Handle, waypoint.X, waypoint.Y, waypoint.Z, GetVehicleModelMaxSpeed(model), style, 10f);
        }

        /// <summary>
        /// Drives around the area.
        /// </summary>
        public static void DriveWander(int style = 0)
        {
            ClearPedTasks(Game.PlayerPed.Handle);
            DriveWanderTaskActive = true;
            DriveToWpTaskActive = false;

            Vehicle veh = GetVehicle();
            uint model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveWander(Game.PlayerPed.Handle, veh.Handle, GetVehicleModelMaxSpeed(model), style);
        }
        #endregion

        #region Quit session & Quit game
        /// <summary>
        /// Quit the current network session, but leaves you connected to the server so addons/resources are still streamed.
        /// </summary>
        public static void QuitSession() => NetworkSessionEnd(true, true);

        /// <summary>
        /// Quit the game after 5 seconds.
        /// </summary>
        public static async void QuitGame()
        {
            Notify.Info("The game will exit in 5 seconds.");
            Debug.WriteLine("Game will be terminated in 5 seconds, because the player used the Quit Game option in vMenu.");
            await BaseScript.Delay(5000);
            ForceSocialClubUpdate(); // bye bye
        }
        #endregion

        #region Teleport to player (or teleport into the player's vehicle)
        /// <summary>
        /// Teleport to the specified player id. (Optionally teleport into their vehicle).
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inVehicle"></param>
        public static async void TeleportToPlayer(int playerId, bool inVehicle = false)
        {
            // If the player exists.
            if (NetworkIsPlayerActive(playerId))
            {
                int playerPed = GetPlayerPed(playerId);
                if (Game.PlayerPed.Handle == playerPed)
                {
                    Notify.Error("Sorry, you can ~r~~h~not~h~ ~s~teleport to yourself!");
                    return;
                }

                // Get the coords of the other player.
                Vector3 playerPos = GetEntityCoords(playerPed, true);

                // Then await the proper loading/teleporting.
                await TeleportToCoords(playerPos);

                // If the player should be teleported inside the other player's vehcile.
                if (inVehicle)
                {
                    // Is the other player inside a vehicle?
                    if (IsPedInAnyVehicle(playerPed, false))
                    {
                        // Get the vehicle of the specified player.
                        Vehicle vehicle = GetVehicle(new Player(playerId), false);
                        if (vehicle != null)
                        {
                            int totalVehicleSeats = GetVehicleModelNumberOfSeats(GetVehicleModel(vehicle: vehicle.Handle));

                            // Does the vehicle exist? Is it NOT dead/broken? Are there enough vehicle seats empty?
                            if (vehicle.Exists() && !vehicle.IsDead && IsAnyVehicleSeatEmpty(vehicle.Handle))
                            {
                                TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, (int)VehicleSeat.Any);
                                Notify.Success("Teleported into ~g~<C>" + GetPlayerName(playerId) + "</C>'s ~s~vehicle.");
                            }
                            // If there are not enough empty vehicle seats or the vehicle doesn't exist/is dead then notify the user.
                            else
                            {
                                // If there's only one seat on this vehicle, tell them that it's a one-seater.
                                if (totalVehicleSeats == 1)
                                {
                                    Notify.Error("This vehicle only has room for 1 player!");
                                }
                                // Otherwise, tell them there's not enough empty seats remaining.
                                else
                                {
                                    Notify.Error("Not enough empty vehicle seats remaining!");
                                }
                            }
                        }
                    }
                }
                // The player is not being teleported into the vehicle, so the teleporting is successfull.
                // Notify the user.
                else
                {
                    Notify.Success("Teleported to ~y~<C>" + GetPlayerName(playerId) + "</C>~s~.");
                }
            }
            // The specified playerId does not exist, notify the user of the error.
            else
            {
                Notify.Error(CommonErrors.PlayerNotFound, placeholderValue: "So the teleport has been cancelled.");
            }
        }
        #endregion

        #region Teleport To Coords
        /// <summary>
        /// Teleport to the specified <see cref="pos"/>.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="safeModeDisabled"></param>
        /// <returns></returns>
        public static async Task TeleportToCoords(Vector3 pos, bool safeModeDisabled = false)
        {
            if (!safeModeDisabled)
            {
                RequestCollisionAtCoord(pos.X, pos.Y, pos.Z);
                bool inCar = Game.PlayerPed.IsInVehicle() && GetVehicle().Driver == Game.PlayerPed;
                if (inCar)
                {
                    SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z);
                    FreezeEntityPosition(GetVehiclePedIsIn(Game.PlayerPed.Handle, false), true);
                }
                else
                {
                    SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                    FreezeEntityPosition(Game.PlayerPed.Handle, true);
                }

                int timer = GetGameTimer();
                bool failed = true;
                float outputZ = pos.Z;
                await Delay(100);
                var z = 0f;
                while (GetGameTimer() - timer < 5000)
                {
                    await Delay(0);
                    if (GetGroundZFor_3dCoord(pos.X, pos.Y, z, ref outputZ, true))
                    {
                        failed = false;
                        break;
                    }
                    //Debug.WriteLine(z.ToString());
                    z = z + 10f;
                    if (z > 900)
                    {
                        z = 5;
                    }
                }
                if (!failed)
                {
                    if (inCar)
                        SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, pos.X, pos.Y, outputZ);
                    else
                        SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, outputZ, false, false, false, true);
                }
                await Delay(200);
                failed = IsEntityInWater(Game.PlayerPed.Handle) || GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 50f || failed;
                if (failed)
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, 1, false, true);
                    Vector3 safePos = pos;
                    safePos.Z = 810f;
                    var foundSafeSpot = GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, 0, ref safePos, 0, 0, 0);
                    if (foundSafeSpot)
                    {
                        Notify.Alert("No suitable location found near target coordinates. Teleporting to the nearest suitable spawn location as a backup method.");
                        if (inCar)
                            SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, safePos.X, safePos.Y, safePos.Z);
                        else
                            SetEntityCoords(Game.PlayerPed.Handle, safePos.X, safePos.Y, safePos.Z, false, false, false, true);
                    }
                    else
                    {
                        Notify.Alert("Failed to find a suitable location, backup method #1 failed, only backup method #2 remains: Open your parachute!");
                        if (inCar)
                            SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, pos.X, pos.Y, 810f);
                        else
                            SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, 810f, false, false, false, true);
                    }
                }
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle() && GetVehicle().Driver == Game.PlayerPed)
                {
                    SetEntityCoords(GetVehicle().Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                }
                else
                {
                    SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                }
            }
            if (Game.PlayerPed.IsInVehicle())
            {
                FreezeEntityPosition(GetVehiclePedIsIn(Game.PlayerPed.Handle, false), false);
            }
            else
            {
                Game.PlayerPed.IsPositionFrozen = false;
            }
        }

        /// <summary>
        /// Teleports to the player's waypoint. If no waypoint is set, notify the user.
        /// </summary>
        public static async void TeleportToWp()
        {
            if (Game.IsWaypointActive)
            {
                var pos = World.WaypointPosition;
                pos.Z = Game.PlayerPed.Position.Z;
                await TeleportToCoords(pos);
            }
            else
            {
                Notify.Error("You need to set a waypoint first!");
            }
        }
        #endregion

        #region Kick Player
        /// <summary>
        /// Kick a user with the provided kick reason. Or ask the current player to provide a reason.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="askUserForReason"></param>
        /// <param name="providedReason"></param>
        public static async void KickPlayer(Player player, bool askUserForReason, string providedReason = "You have been kicked.")
        {
            if (player != null)
            {
                // Default kick reason.
                string defaultReason = "You have been kicked.";
                // If we need to ask for the user's input and the default reason is the same as the provided reason, get the user input..
                if (askUserForReason && providedReason == defaultReason)
                {
                    string userInput = await GetUserInput(windowTitle: "Enter Kick Message", maxInputLength: 100);
                    // If the input is not invalid, set the kick reason to the user's custom message.
                    if (!string.IsNullOrEmpty(userInput))
                    {
                        defaultReason += $" Reason: {userInput}";
                    }
                    else
                    {
                        Notify.Error("An invalid kick reason was provided. Action cancelled.");
                        return;
                    }
                }
                // Kick the player using the specified reason.
                TriggerServerEvent("vMenu:KickPlayer", player.ServerId, defaultReason);
                Log($"Attempting to kick player {player.Name} (server id: {player.ServerId}, client id: {player.Handle}).");
            }
            else
            {
                Notify.Error("The selected player is somehow invalid, action aborted.");
            }
        }
        #endregion

        #region (Temp) Ban Player
        /// <summary>
        /// Bans the specified player.
        /// </summary>
        /// <param name="player">Player to ban.</param>
        /// <param name="forever">Ban forever or ban temporarily.</param>
        public static async void BanPlayer(Player player, bool forever)
        {
            string banReason = await GetUserInput(windowTitle: "Enter Ban Reason", defaultText: "Banned by staff.", maxInputLength: 200);
            if (!string.IsNullOrEmpty(banReason) && banReason.Length > 1)
            {
                if (forever)
                {
                    TriggerServerEvent("vMenu:PermBanPlayer", player.ServerId, banReason);
                }
                else
                {
                    string banDurationHours = await GetUserInput(windowTitle: "Ban Duration (in hours) - Max: 720 (1 month)", defaultText: "1.5", maxInputLength: 10);
                    if (!string.IsNullOrEmpty(banDurationHours))
                    {
                        if (double.TryParse(banDurationHours, out double banHours))
                        {
                            TriggerServerEvent("vMenu:TempBanPlayer", player.ServerId, banHours, banReason);
                        }
                        else
                        {
                            if (int.TryParse(banDurationHours, out int banHoursInt))
                            {
                                TriggerServerEvent("vMenu:TempBanPlayer", player.ServerId, (double)banHoursInt, banReason);
                            }
                            else
                            {
                                Notify.Error(CommonErrors.InvalidInput);
                                TriggerEvent("chatMessage", $"[vMenu] The input is invalid or you cancelled the action, please try again.");
                            }
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        TriggerEvent("chatMessage", $"[vMenu] The input is invalid or you cancelled the action, please try again.");
                    }

                }
            }
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
                TriggerEvent("chatMessage", $"[vMenu] The input is invalid or you cancelled the action, please try again.");
            }
        }
        #endregion

        #region Kill Player/commit suicide options
        /// <summary>
        /// Kill player
        /// </summary>
        /// <param name="player"></param>
        public static void KillPlayer(Player player) => TriggerServerEvent("vMenu:KillPlayer", player.ServerId);

        /// <summary>
        /// Kill yourself.
        /// </summary>
        public static async void CommitSuicide()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                // Get the suicide animations ready.
                RequestAnimDict("mp_suicide");
                while (!HasAnimDictLoaded("mp_suicide"))
                {
                    await Delay(0);
                }
                // Decide if the death should be using a pill or a gun (randomly).
                uint? weaponHash = null;
                bool takePill = false;


                if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.PistolMk2))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_PISTOL_MK2");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.CombatPistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_COMBATPISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Pistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_PISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon((WeaponHash)(uint)GetHashKey("WEAPON_SNSPISTOL_MK2")))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_SNSPISTOL_MK2");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.SNSPistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_SNSPISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Pistol50))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_PISTOL50");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.HeavyPistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_HEAVYPISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.VintagePistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_VINTAGEPISTOL");
                }
                else
                {
                    takePill = true;
                }


                // If we take the pill, remove any weapons in our hands.
                if (takePill)
                {
                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_unarmed"), true);
                }
                // Otherwise, give the ped a gun.
                else if (weaponHash != null)
                {
                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)weaponHash, true);
                    SetPedDropsWeaponsWhenDead(Game.PlayerPed.Handle, true);
                }
                else
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_pistol_mk2"), 1, false, true);
                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_pistol_mk2"), true);
                    SetPedDropsWeaponsWhenDead(Game.PlayerPed.Handle, true);
                }

                // Play the animation for the pill or pistol suicide type. Pistol oddly enough does not have any sounds. Needs research.
                ClearPedTasks(Game.PlayerPed.Handle);
                TaskPlayAnim(Game.PlayerPed.Handle, "MP_SUICIDE", (takePill ? "pill" : "pistol"), 8f, -8f, -1, 270540800, 0, false, false, false);

                bool shot = false;
                while (true)
                {
                    float time = GetEntityAnimCurrentTime(Game.PlayerPed.Handle, "MP_SUICIDE", (takePill ? "pill" : "pistol"));
                    if (HasAnimEventFired(Game.PlayerPed.Handle, (uint)GetHashKey("Fire")) && !shot) // shoot the gun if the animation event is triggered.
                    {
                        ClearEntityLastDamageEntity(Game.PlayerPed.Handle);
                        SetPedShootsAtCoord(Game.PlayerPed.Handle, 0f, 0f, 0f, false);
                        shot = true;
                    }
                    if (time > (takePill ? 0.536f : 0.365f))
                    {
                        // Kill the player.
                        ClearEntityLastDamageEntity(Game.PlayerPed.Handle);
                        SetEntityHealth(Game.PlayerPed.Handle, 0);
                        break;
                    }
                    await Delay(0);
                }
                RemoveAnimDict("mp_suicide");
            }
            else
            {
                SetEntityHealth(Game.PlayerPed.Handle, 0);
            }


        }
        #endregion

        #region Summon Player
        /// <summary>
        /// Summon player.
        /// </summary>
        /// <param name="player"></param>
        public static void SummonPlayer(Player player) => TriggerServerEvent("vMenu:SummonPlayer", player.ServerId);
        #endregion

        #region Spectate function
        private static int currentlySpectatingPlayer = -1;
        public static async void SpectatePlayer(Player player, bool forceDisable = false)
        {
            if (forceDisable)
            {
                NetworkSetInSpectatorMode(false, 0); // disable spectating.
            }
            else
            {
                if (player.Handle == Game.Player.Handle)
                {
                    if (NetworkIsInSpectatorMode())
                    {
                        DoScreenFadeOut(500);
                        while (IsScreenFadingOut()) await Delay(0);
                        NetworkSetInSpectatorMode(false, 0); // disable spectating.
                        DoScreenFadeIn(500);
                        Notify.Success("Stopped spectating.", false, true);
                        currentlySpectatingPlayer = -1;
                    }
                    else
                    {
                        Notify.Error("You can't spectate yourself.", false, true);
                    }
                }
                else
                {
                    if (NetworkIsInSpectatorMode())
                    {
                        if (currentlySpectatingPlayer != player.Handle)
                        {
                            DoScreenFadeOut(500);
                            while (IsScreenFadingOut()) await Delay(0);
                            NetworkSetInSpectatorMode(false, 0);
                            NetworkSetInSpectatorMode(true, player.Character.Handle);
                            DoScreenFadeIn(500);
                            Notify.Success($"You are now spectating ~g~<C>{GetSafePlayerName(player.Name)}</C>~s~.", false, true);
                            currentlySpectatingPlayer = player.Handle;
                        }
                        else
                        {
                            DoScreenFadeOut(500);
                            while (IsScreenFadingOut()) await Delay(0);
                            NetworkSetInSpectatorMode(false, 0); // disable spectating.
                            DoScreenFadeIn(500);
                            Notify.Success("Stopped spectating.", false, true);
                            currentlySpectatingPlayer = -1;
                        }
                    }
                    else
                    {
                        DoScreenFadeOut(500);
                        while (IsScreenFadingOut()) await Delay(0);
                        NetworkSetInSpectatorMode(false, 0);
                        NetworkSetInSpectatorMode(true, player.Character.Handle);
                        DoScreenFadeIn(500);
                        Notify.Success($"You are now spectating ~g~<C>{GetSafePlayerName(player.Name)}</C>~s~.", false, true);
                        currentlySpectatingPlayer = player.Handle;
                    }
                }
            }
        }
        #endregion

        #region Cycle Through Vehicle Seats
        /// <summary>
        /// Cycle to the next available seat.
        /// </summary>
        public static void CycleThroughSeats()
        {

            // Create a new vehicle.
            Vehicle vehicle = GetVehicle();

            // If there are enough empty seats, continue.
            if (AreAnyVehicleSeatsFree(vehicle.Handle))
            {
                // Get the total seats for this vehicle.
                var maxSeats = GetVehicleModelNumberOfSeats((uint)GetEntityModel(vehicle.Handle));

                // If the player is currently in the "last" seat, start from the driver's position and loop through the seats.
                if (GetPedInVehicleSeat(vehicle.Handle, maxSeats - 2) == Game.PlayerPed.Handle)
                {
                    // Loop through all seats.
                    for (var seat = -1; seat < maxSeats - 2; seat++)
                    {
                        // If the seat is free, get in it and stop the loop.
                        if (vehicle.IsSeatFree((VehicleSeat)seat))
                        {
                            TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
                            break;
                        }
                    }
                }
                // If the player is not in the "last" seat, loop through all the seats starrting from the driver's position.
                else
                {
                    var switchedPlace = false;
                    var passedCurrentSeat = false;
                    // Loop through all the seats.
                    for (var seat = -1; seat < maxSeats - 1; seat++)
                    {
                        // If this seat is the one the player is sitting on, set passedCurrentSeat to true.
                        // This way we won't just keep placing the ped in the 1st available seat, but actually the first "next" available seat.
                        if (!passedCurrentSeat && GetPedInVehicleSeat(vehicle.Handle, seat) == Game.PlayerPed.Handle)
                        {
                            passedCurrentSeat = true;
                        }

                        // Only if the current seat has been passed, check if the seat is empty and if so teleport into it and stop the loop.
                        if (passedCurrentSeat && IsVehicleSeatFree(vehicle.Handle, seat))
                        {
                            switchedPlace = true;
                            TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
                            break;
                        }
                    }
                    // If the player was not switched, then that means there are not enough empty vehicle seats "after" the player, and the player was not sitting in the "last" seat.
                    // To fix this, loop through the entire vehicle again and place them in the first available seat.
                    if (!switchedPlace)
                    {
                        // Loop through all seats, starting at the drivers seat (-1), then moving up.
                        for (var seat = -1; seat < maxSeats - 1; seat++)
                        {
                            // If the seat is free, take it and break the loop.
                            if (IsVehicleSeatFree(vehicle.Handle, seat))
                            {
                                TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                Notify.Alert("There are no more available seats to cycle through.");
            }
        }
        #endregion

        #region Spawn Vehicle
        #region Overload Spawn Vehicle Function
        /// <summary>
        /// Simple custom vehicle spawn function.
        /// </summary>
        /// <param name="vehicleName">Vehicle model name. If "custom" the user will be asked to enter a model name.</param>
        /// <param name="spawnInside">Warp the player inside the vehicle after spawning.</param>
        /// <param name="replacePrevious">Replace the previous vehicle of the player.</param>
        public static async void SpawnVehicle(string vehicleName = "custom", bool spawnInside = false, bool replacePrevious = false)
        {
            if (vehicleName == "custom")
            {
                // Get the result.
                string result = await GetUserInput(windowTitle: "Enter Vehicle Name");
                // If the result was not invalid.
                if (!string.IsNullOrEmpty(result))
                {
                    // Convert it into a model hash.
                    uint model = (uint)GetHashKey(result);
                    SpawnVehicle(vehicleHash: model, spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false, vehicleInfo: new VehicleInfo(),
                        saveName: null);
                }
                // Result was invalid.
                else
                {
                    Notify.Error(CommonErrors.InvalidInput);
                }
            }
            // Spawn the specified vehicle.
            else
            {
                SpawnVehicle(vehicleHash: (uint)GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false,
                    vehicleInfo: new VehicleInfo(), saveName: null);
            }
        }
        #endregion

        #region Main Spawn Vehicle Function
        /// <summary>
        /// Spawns a vehicle.
        /// </summary>
        /// <param name="vehicleHash">Model hash of the vehicle to spawn.</param>
        /// <param name="spawnInside">Teleports the player into the vehicle after spawning.</param>
        /// <param name="replacePrevious">Replaces the previous vehicle of the player with the new one.</param>
        /// <param name="skipLoad">Does not attempt to load the vehicle, but will spawn it right a way.</param>
        /// <param name="vehicleInfo">All information needed for a saved vehicle to re-apply all mods.</param>
        /// <param name="saveName">Used to get/set info about the saved vehicle data.</param>
        public static async void SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious, bool skipLoad, VehicleInfo vehicleInfo, string saveName = null)
        {
            float speed = 0f;
            float rpm = 0f;
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle tmpOldVehicle = GetVehicle();
                speed = GetEntitySpeedVector(tmpOldVehicle.Handle, true).Y; // get forward/backward speed only
                rpm = tmpOldVehicle.CurrentRPM;
                tmpOldVehicle = null;
            }


            var vehClass = GetVehicleClassFromName(vehicleHash);
            int modelClass = GetVehicleClassFromName(vehicleHash);
            if (!VehicleSpawner.allowedCategories[modelClass])
            {
                Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
                return;
            }

            if (!skipLoad)
            {
                bool successFull = await LoadModel(vehicleHash);
                if (!successFull || !IsModelAVehicle(vehicleHash))
                {
                    // Vehicle model is invalid.
                    Notify.Error(CommonErrors.InvalidModel);
                    return;
                }
            }

            Log("Spawning of vehicle is NOT cancelled, if this model is invalid then there's something wrong.");

            // Get the heading & position for where the vehicle should be spawned.
            Vector3 pos = (spawnInside) ? GetEntityCoords(Game.PlayerPed.Handle, true) : GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 8f, 0f);
            float heading = GetEntityHeading(Game.PlayerPed.Handle) + (spawnInside ? 0f : 90f);

            // If the previous vehicle exists...
            if (_previousVehicle != null)
            {
                // And it's actually a vehicle (rather than another random entity type)
                if (_previousVehicle.Exists() && _previousVehicle.PreviouslyOwnedByPlayer &&
                    (_previousVehicle.Occupants.Count() == 0 || _previousVehicle.Driver.Handle == Game.PlayerPed.Handle))
                {
                    // If the previous vehicle should be deleted:
                    if (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious))
                    {
                        // Delete it.
                        _previousVehicle.PreviouslyOwnedByPlayer = false;
                        SetEntityAsMissionEntity(_previousVehicle.Handle, true, true);
                        _previousVehicle.Delete();
                    }
                    // Otherwise
                    else
                    {
                        if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_keep_spawned_vehicles_persistent))
                        {
                            // Set the vehicle to be no longer needed. This will make the game engine decide when it should be removed (when all players get too far away).
                            _previousVehicle.IsPersistent = false;
                            _previousVehicle.PreviouslyOwnedByPlayer = false;
                            _previousVehicle.MarkAsNoLongerNeeded();
                        }
                    }
                    _previousVehicle = null;
                }
            }

            if (Game.PlayerPed.IsInVehicle() && (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious)))
            {
                if (GetVehicle().Driver == Game.PlayerPed)// && IsVehiclePreviouslyOwnedByPlayer(GetVehicle()))
                {
                    var tmpveh = GetVehicle();
                    SetVehicleHasBeenOwnedByPlayer(tmpveh.Handle, false);
                    SetEntityAsMissionEntity(tmpveh.Handle, true, true);

                    if (_previousVehicle != null)
                    {
                        if (_previousVehicle.Handle == tmpveh.Handle)
                        {
                            _previousVehicle = null;
                        }
                    }
                    tmpveh.Delete();
                    Notify.Info("Your old car was removed to prevent your new car from glitching inside it. Next time, get out of your vehicle before spawning a new one if you want to keep your old one.");
                }
            }

            if (_previousVehicle != null)
                _previousVehicle.PreviouslyOwnedByPlayer = false;

            if (Game.PlayerPed.IsInVehicle())
                pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0, 8f, 0.1f);

            // Create the new vehicle and remove the need to hotwire the car.
            Vehicle vehicle = new Vehicle(CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z + 1f, heading, true, false))
            {
                NeedsToBeHotwired = false,
                PreviouslyOwnedByPlayer = true,
                IsPersistent = true
            };

            Log($"New vehicle, hash:{vehicleHash}, handle:{vehicle.Handle}, force-re-save-name:{(saveName ?? "NONE")}, created at x:{pos.X} y:{pos.Y} z:{(pos.Z + 1f)} " +
                $"heading:{heading}");

            // If spawnInside is true
            if (spawnInside)
            {
                // Set the vehicle's engine to be running.
                vehicle.IsEngineRunning = true;

                // Set the ped into the vehicle.
                new Ped(Game.PlayerPed.Handle).SetIntoVehicle(vehicle, VehicleSeat.Driver);

                // If the vehicle is a helicopter and the player is in the air, set the blades to be full speed.
                if (vehicle.ClassType == VehicleClass.Helicopters && GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 10.0f)
                {
                    SetHeliBladesFullSpeed(vehicle.Handle);
                }
                // If it's not a helicopter or the player is not in the air, set the vehicle on the ground properly.
                else
                {
                    vehicle.PlaceOnGround();
                }
            }

            // If mod info about the vehicle was specified, check if it's not null.
            if (saveName != null)
            {
                // Set the modkit so we can modify the car.
                SetVehicleModKit(vehicle.Handle, 0);

                // set the extras
                foreach (var extra in vehicleInfo.extras)
                {
                    if (DoesExtraExist(vehicle.Handle, extra.Key))
                        vehicle.ToggleExtra(extra.Key, extra.Value);
                }

                SetVehicleWheelType(vehicle.Handle, vehicleInfo.wheelType);
                SetVehicleMod(vehicle.Handle, 23, 0, vehicleInfo.customWheels);
                if (vehicle.Model.IsBike)
                {
                    SetVehicleMod(vehicle.Handle, 24, 0, vehicleInfo.customWheels);
                }
                ToggleVehicleMod(vehicle.Handle, 18, vehicleInfo.turbo);
                SetVehicleTyreSmokeColor(vehicle.Handle, vehicleInfo.colors["tyresmokeR"], vehicleInfo.colors["tyresmokeG"], vehicleInfo.colors["tyresmokeB"]);
                ToggleVehicleMod(vehicle.Handle, 20, vehicleInfo.tyreSmoke);
                ToggleVehicleMod(vehicle.Handle, 22, vehicleInfo.xenonHeadlights);
                SetVehicleLivery(vehicle.Handle, vehicleInfo.livery);

                SetVehicleColours(vehicle.Handle, vehicleInfo.colors["primary"], vehicleInfo.colors["secondary"]);
                SetVehicleInteriorColour(vehicle.Handle, vehicleInfo.colors["trim"]);
                SetVehicleDashboardColour(vehicle.Handle, vehicleInfo.colors["dash"]);

                SetVehicleExtraColours(vehicle.Handle, vehicleInfo.colors["pearlescent"], vehicleInfo.colors["wheels"]);

                SetVehicleNumberPlateText(vehicle.Handle, vehicleInfo.plateText);
                SetVehicleNumberPlateTextIndex(vehicle.Handle, vehicleInfo.plateStyle);

                SetVehicleWindowTint(vehicle.Handle, vehicleInfo.windowTint);

                foreach (var mod in vehicleInfo.mods)
                {
                    SetVehicleMod(vehicle.Handle, mod.Key, mod.Value, vehicleInfo.customWheels);
                }
                vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(red: vehicleInfo.colors["neonR"], green: vehicleInfo.colors["neonG"], blue: vehicleInfo.colors["neonB"]);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, vehicleInfo.neonLeft);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, vehicleInfo.neonRight);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, vehicleInfo.neonFront);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, vehicleInfo.neonBack);

                vehicle.CanTiresBurst = !vehicleInfo.bulletProofTires;

                VehicleOptions._SET_VEHICLE_HEADLIGHTS_COLOR(vehicle, vehicleInfo.headlightColor);
            }

            // Set the previous vehicle to the new vehicle.
            _previousVehicle = vehicle;
            //vehicle.Speed = speed; // retarded feature that randomly breaks for no fucking reason
            if (!vehicle.Model.IsTrain) // to be extra fucking safe
            {
                // workaround of retarded feature above:
                SetVehicleForwardSpeed(vehicle.Handle, speed);
            }
            vehicle.CurrentRPM = rpm;

            // Discard the model.
            SetModelAsNoLongerNeeded(vehicleHash);
        }
        #endregion
        #endregion

        #region VehicleInfo struct
        /// <summary>
        /// Contains all information for a saved vehicle.
        /// </summary>
        public struct VehicleInfo
        {
            public Dictionary<string, int> colors;
            public bool customWheels;
            public Dictionary<int, bool> extras;
            public int livery;
            public uint model;
            public Dictionary<int, int> mods;
            public string name;
            public bool neonBack;
            public bool neonFront;
            public bool neonLeft;
            public bool neonRight;
            public string plateText;
            public int plateStyle;
            public bool turbo;
            public bool tyreSmoke;
            public int version;
            public int wheelType;
            public int windowTint;
            public bool xenonHeadlights;
            public bool bulletProofTires;
            public int headlightColor;
        };
        #endregion

        #region Save Vehicle
        /// <summary>
        /// Saves the vehicle the player is currently in to the client's kvp storage.
        /// </summary>
        public static async void SaveVehicle(string updateExistingSavedVehicleName = null)
        {
            // Only continue if the player is in a vehicle.
            if (Game.PlayerPed.IsInVehicle())
            {
                // Get the vehicle.
                Vehicle veh = GetVehicle();
                // Make sure the entity is actually a vehicle and it still exists, and it's not dead.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.IsDriveable)
                {
                    #region new saving method
                    Dictionary<int, int> mods = new Dictionary<int, int>();

                    foreach (var mod in veh.Mods.GetAllMods())
                    {
                        mods.Add((int)mod.ModType, mod.Index);
                    }

                    #region colors
                    var colors = new Dictionary<string, int>();
                    int primaryColor = 0;
                    int secondaryColor = 0;
                    int pearlescentColor = 0;
                    int wheelColor = 0;
                    int dashColor = 0;
                    int trimColor = 0;
                    GetVehicleExtraColours(veh.Handle, ref pearlescentColor, ref wheelColor);
                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref trimColor);
                    colors.Add("primary", primaryColor);
                    colors.Add("secondary", secondaryColor);
                    colors.Add("pearlescent", pearlescentColor);
                    colors.Add("wheels", wheelColor);
                    colors.Add("dash", dashColor);
                    colors.Add("trim", trimColor);
                    int neonR = 255;
                    int neonG = 255;
                    int neonB = 255;
                    if (veh.Mods.HasNeonLights)
                    {
                        GetVehicleNeonLightsColour(veh.Handle, ref neonR, ref neonG, ref neonB);
                    }
                    colors.Add("neonR", neonR);
                    colors.Add("neonG", neonG);
                    colors.Add("neonB", neonB);
                    int tyresmokeR = 0;
                    int tyresmokeG = 0;
                    int tyresmokeB = 0;
                    GetVehicleTyreSmokeColor(veh.Handle, ref tyresmokeR, ref tyresmokeG, ref tyresmokeB);
                    colors.Add("tyresmokeR", tyresmokeR);
                    colors.Add("tyresmokeG", tyresmokeG);
                    colors.Add("tyresmokeB", tyresmokeB);
                    #endregion

                    var extras = new Dictionary<int, bool>();
                    for (int i = 0; i < 20; i++)
                    {
                        if (veh.ExtraExists(i))
                        {
                            extras.Add(i, veh.IsExtraOn(i));
                        }
                    }

                    VehicleInfo vi = new VehicleInfo()
                    {
                        colors = colors,
                        customWheels = GetVehicleModVariation(veh.Handle, 23),
                        extras = extras,
                        livery = GetVehicleLivery(veh.Handle),
                        model = (uint)GetEntityModel(veh.Handle),
                        mods = mods,
                        name = GetLabelText(GetDisplayNameFromVehicleModel((uint)GetEntityModel(veh.Handle))),
                        neonBack = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back),
                        neonFront = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front),
                        neonLeft = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left),
                        neonRight = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right),
                        plateText = veh.Mods.LicensePlate,
                        plateStyle = (int)veh.Mods.LicensePlateStyle,
                        turbo = IsToggleModOn(veh.Handle, 18),
                        tyreSmoke = IsToggleModOn(veh.Handle, 20),
                        version = 1,
                        wheelType = GetVehicleWheelType(veh.Handle),
                        windowTint = (int)veh.Mods.WindowTint,
                        xenonHeadlights = IsToggleModOn(veh.Handle, 22),
                        bulletProofTires = !veh.CanTiresBurst,
                        headlightColor = VehicleOptions._GET_VEHICLE_HEADLIGHTS_COLOR(veh)
                    };

                    #endregion

                    if (updateExistingSavedVehicleName == null)
                    {
                        // Ask the user for a save name (will be displayed to the user and will be used as unique identifier for this vehicle)
                        var saveName = await GetUserInput(windowTitle: "Enter a save name", maxInputLength: 30);
                        // If the name is not invalid.
                        if (!string.IsNullOrEmpty(saveName))
                        {
                            // Save everything from the dictionary into the client's kvp storage.
                            // If the save was successfull:
                            if (StorageManager.SaveVehicleInfo("veh_" + saveName, vi, false))
                            {
                                Notify.Success($"Vehicle {saveName} saved.");
                            }
                            // If the save was not successfull:
                            else
                            {
                                Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: "(" + saveName + ")");
                            }
                        }
                        // The user did not enter a valid name to use as a save name for this vehicle.
                        else
                        {
                            Notify.Error(CommonErrors.InvalidSaveName);
                        }
                    }
                    // We need to update an existing slot.
                    else
                    {
                        StorageManager.SaveVehicleInfo("veh_" + updateExistingSavedVehicleName, vi, true);
                    }

                }
                // The player is not inside a vehicle, or the vehicle is dead/not existing so we won't do anything. Only alert the user.
                else
                {
                    Notify.Error(CommonErrors.NoVehicle, placeholderValue: "to save it");
                }
            }
            // The player is not inside a vehicle.
            else
            {
                Notify.Error(CommonErrors.NoVehicle);
            }

            // update the saved vehicles menu list to reflect the new saved car.
            if (MainMenu.SavedVehiclesMenu != null)
            {
                MainMenu.SavedVehiclesMenu.UpdateMenuAvailableCategories();
            }

        }
        #endregion

        #region Get Saved Vehicles Dictionary
        /// <summary>
        /// Returns a collection of all saved vehicles, with their save name and saved vehicle info struct.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, VehicleInfo> GetSavedVehicles()
        {
            // Create a list to store all saved vehicle names in.
            var savedVehicleNames = new List<string>();
            // Start looking for kvps starting with veh_
            var findHandle = StartFindKvp("veh_");
            // Keep looking...
            while (true)
            {
                // Get the kvp string key.
                var vehString = FindKvp(findHandle);

                // If it exists then the key to the list.
                if (vehString != "" && vehString != null && vehString != "NULL")
                {
                    //Debug.WriteLine(vehString);
                    savedVehicleNames.Add(vehString);
                }
                // Otherwise stop.
                else
                {
                    EndFindKvp(findHandle);
                    break;
                }
            }

            // Create a Dictionary to store all vehicle information in.
            //var vehiclesList = new Dictionary<string, Dictionary<string, string>>();
            var vehiclesList = new Dictionary<string, VehicleInfo>();
            // Loop through all save names (keys) from the list above, convert the string into a dictionary 
            // and add it to the dictionary above, with the vehicle save name as the key.
            foreach (var saveName in savedVehicleNames)
            {
                vehiclesList.Add(saveName, StorageManager.GetSavedVehicleInfo(saveName));
            }
            // Return the vehicle dictionary containing all vehicle save names (keys) linked to the correct vehicle
            // including all vehicle mods/customization parts.
            return vehiclesList;
        }
        #endregion

        #region Load Model
        /// <summary>
        /// Check and load a model.
        /// </summary>
        /// <param name="modelHash"></param>
        /// <returns>True if model is valid & loaded, false if model is invalid.</returns>
        private static async Task<bool> LoadModel(uint modelHash)
        {
            // Check if the model exists in the game.
            if (IsModelInCdimage(modelHash))
            {
                // Load the model.
                RequestModel(modelHash);
                // Wait until it's loaded.
                while (!HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }
                // Model is loaded, return true.
                return true;
            }
            // Model is not valid or is not loaded correctly.
            else
            {
                // Return false.
                return false;
            }
        }
        #endregion

        #region GetUserInput
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            var currentMenu = GetOpenMenu();
            if (currentMenu != null)
            {
                MenuController.CloseAllMenus();
            }
            MainMenu.DisableControls = true;
            MainMenu.DontOpenMenus = true;

            // Create the window title string.
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength.ToString()} Characters)");

            async void ReopenMenuDelayed(Menu menu)
            {
                MainMenu.DontOpenMenus = false;
                await Delay(100);
                if (menu != null)
                {
                    menu.OpenMenu();
                }
                await Delay(50);
                MainMenu.DisableControls = false;
            }

            // Display the input box.
            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await Delay(0);
            // Wait for a result.
            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        ReopenMenuDelayed(currentMenu);
                        return null;
                    case 1: // finished editing
                        ReopenMenuDelayed(currentMenu);
                        return GetOnscreenKeyboardResult();
                    default:
                        await Delay(0);
                        break;
                }
            }
        }
        #endregion

        #region Set License Plate Text
        /// <summary>
        /// Set the license plate text using the player's custom input.
        /// </summary>
        public static async void SetLicensePlateCustomText()
        {
            // Get the input.
            var text = await GetUserInput(windowTitle: "Enter License Plate", maxInputLength: 8);
            // If the input is valid.
            if (!string.IsNullOrEmpty(text))
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // If it exists.
                if (veh != null && veh.Exists())
                {
                    // Set the license plate.
                    SetVehicleNumberPlateText(veh.Handle, text);
                }
                // If it doesn't exist, notify the user.
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                }
            }
            // No valid text was given.
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
            }

        }
        #endregion

        #region ToProperString()
        /// <summary>
        /// Converts a PascalCaseString to a Propper Case String.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Input string converted to a normal sentence.</returns>
        public static string ToProperString(string inputString)
        {
            var outputString = "";
            var prevUpper = true;
            foreach (char c in inputString)
            {
                if (char.IsLetter(c) && c != ' ' && c == char.Parse(c.ToString().ToUpper()))
                {
                    if (prevUpper)
                    {
                        outputString += $"{c.ToString()}";
                    }
                    else
                    {
                        outputString += $" {c.ToString()}";
                    }
                    prevUpper = true;
                }
                else
                {
                    prevUpper = false;
                    outputString += c.ToString();
                }
            }
            while (outputString.IndexOf("  ") != -1)
            {
                outputString = outputString.Replace("  ", " ");
            }
            return outputString;
        }
        #endregion

        #region Permissions
        /// <summary>
        /// Checks if the specified permission is granted for this user.
        /// Also checks parent/inherited/wildcard permissions.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        //public static bool IsAllowed(vMenuShared.PermissionsManager.Permission permission) => vMenuShared.PermissionsManager.IsAllowed(permission);//PermissionsManager.IsAllowed(permission);
        #endregion

        #region Play Scenarios
        /// <summary>
        /// Play the specified scenario name.
        /// If "forcestop" is specified, any currrently playing scenarios will be forcefully stopped.
        /// </summary>
        /// <param name="scenarioName"></param>
        public static void PlayScenario(string scenarioName)
        {
            // If there's currently no scenario playing, or the current scenario is not the same as the new scenario, then..
            if (_currentScenario == "" || _currentScenario != scenarioName)
            {
                // Set the current scenario.
                _currentScenario = scenarioName;
                // Clear all tasks to make sure the player is ready to play the scenario.
                ClearPedTasks(Game.PlayerPed.Handle);

                var canPlay = true;
                // Check if the player CAN play a scenario... 
                if (IsPedRunning(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are running.", true, false);
                    canPlay = false;
                }
                if (IsEntityDead(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are dead.", true, false);
                    canPlay = false;
                }
                if (IsPlayerInCutscene(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are in a cutscene.", true, false);
                    canPlay = false;
                }
                if (IsPedFalling(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are falling.", true, false);
                    canPlay = false;
                }
                if (IsPedRagdoll(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are currently in a ragdoll state.", true, false);
                    canPlay = false;
                }
                if (!IsPedOnFoot(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You must be on foot to start a scenario.", true, false);
                    canPlay = false;
                }
                if (NetworkIsInSpectatorMode())
                {
                    Notify.Alert("You can't start a scenario when you are currently spectating.", true, false);
                    canPlay = false;
                }
                if (GetEntitySpeed(Game.PlayerPed.Handle) > 5.0f)
                {
                    Notify.Alert("You can't start a scenario when you are moving too fast.", true, false);
                    canPlay = false;
                }

                if (canPlay)
                {
                    // If the scenario is a "sit" scenario, then the scenario needs to be played at a specific location.
                    if (PedScenarios.PositionBasedScenarios.Contains(scenarioName))
                    {
                        // Get the offset-position from the player. (0.5m behind the player, and 0.5m below the player seems fine for most scenarios)
                        var pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, -0.5f, -0.5f);
                        var heading = GetEntityHeading(Game.PlayerPed.Handle);
                        // Play the scenario at the specified location.
                        TaskStartScenarioAtPosition(Game.PlayerPed.Handle, scenarioName, pos.X, pos.Y, pos.Z, heading, -1, true, false);
                    }
                    // If it's not a sit scenario (or maybe it is, but using the above native causes other
                    // issues for some sit scenarios so those are not registered as "sit" scenarios), then play it at the current player's position.
                    else
                    {
                        TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenarioName, 0, true);
                    }
                }
            }
            // If the new scenario is the same as the currently playing one, cancel the current scenario.
            else
            {
                _currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedSecondaryTask(Game.PlayerPed.Handle);
            }

            // If the scenario name to play is called "forcestop" then clear the current scenario and force any tasks to be cleared.
            if (scenarioName == "forcestop")
            {
                _currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedTasksImmediately(Game.PlayerPed.Handle);
            }

        }
        #endregion

        #region Data parsing functions
        /// <summary>
        /// Converts a simple json string (only containing (string) key : (string) value).
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> JsonToDictionary(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        #endregion

        #region Weather Sync
        /// <summary>
        /// Update the server with the new weather type, blackout status and dynamic weather changes enabled status.
        /// </summary>
        /// <param name="newWeather">The new weather type.</param>
        /// <param name="blackout">Manual blackout mode enabled/disabled.</param>
        /// <param name="dynamicChanges">Dynamic weather changes enabled/disabled.</param>
        public static void UpdateServerWeather(string newWeather, bool blackout, bool dynamicChanges) => TriggerServerEvent("vMenu:UpdateServerWeather", newWeather, blackout, dynamicChanges);

        /// <summary>
        /// Modify the clouds for everyone. If removeClouds is true, then remove all clouds. If it's false, then randomize the clouds.
        /// </summary>
        /// <param name="removeClouds">Removes the clouds from the sky if true, otherwise randomizes the clouds type for all players.</param>
        public static void ModifyClouds(bool removeClouds) => TriggerServerEvent("vMenu:UpdateServerWeatherCloudsType", removeClouds);
        #endregion

        #region Time Sync
        /// <summary>
        /// Update the server with the new time. If an invalid time is provided, then the time will be set to midnight (00:00);
        /// </summary>
        /// <param name="hours">Hours (0-23)</param>
        /// <param name="minutes">Minutes (0-59)</param>
        /// <param name="freezeTime">Should the time be frozen?</param>
        public static void UpdateServerTime(int hours, int minutes, bool freezeTime)
        {
            var realHours = hours;
            var realMinutes = minutes;
            if (hours > 23 || hours < 0)
            {
                realHours = 0;
            }
            if (minutes > 59 || minutes < 0)
            {
                realMinutes = 0;
            }
            TriggerServerEvent("vMenu:UpdateServerTime", realHours, realMinutes, freezeTime);
        }
        #endregion

        #region StringToStringArray
        /// <summary>
        /// Converts the inputString into a string[] (array).
        /// Each string in the array is up to 99 characters long at max.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string[] StringToArray(string inputString)
        {
            return CitizenFX.Core.UI.Screen.StringToArray(inputString);
        }
        #endregion

        #region Hud Functions
        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
            DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        /// <param name="disableTextOutline">Disables the default text outline.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (IsHudPreferenceSwitchedOn() && Hud.IsVisible && !MainMenu.MiscSettingsMenu.HideHud && !IsPlayerSwitchInProgress() && IsScreenFadedIn() && !IsPauseMenuActive() && !IsFrontendFading() && !IsPauseMenuRestarting() && !IsHudHidden())
            {
                SetTextFont(font);
                SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    SetTextWrap(0f, xPosition);
                }
                SetTextJustification((int)justification);
                if (!disableTextOutline) { SetTextOutline(); }
                BeginTextCommandDisplayText("STRING");
                AddTextComponentSubstringPlayerName(text);
                EndTextCommandDisplayText(xPosition, yPosition);
            }
        }
        #endregion

        #region ped info struct
        public struct PedInfo
        {
            public int version;
            public uint model;
            public bool isMpPed;
            public Dictionary<int, int> props;
            public Dictionary<int, int> propTextures;
            public Dictionary<int, int> drawableVariations;
            public Dictionary<int, int> drawableVariationTextures;
        };
        #endregion

        #region Set Player Skin
        /// <summary>
        /// Sets the player's model to the provided modelName.
        /// </summary>
        /// <param name="modelName">The model name.</param>
        public static async Task SetPlayerSkin(string modelName, PedInfo pedCustomizationOptions, bool keepWeapons = true) => await SetPlayerSkin((uint)GetHashKey(modelName), pedCustomizationOptions, keepWeapons);

        /// <summary>
        /// Sets the player's model to the provided modelHash.
        /// </summary>
        /// <param name="modelHash">The model hash.</param>
        public static async Task SetPlayerSkin(uint modelHash, PedInfo pedCustomizationOptions, bool keepWeapons = true)
        {
            if (IsModelInCdimage(modelHash))
            {
                if (keepWeapons)
                {
                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    Log("saved from SetPlayerSkin()");
                }
                RequestModel(modelHash);
                while (!HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }

                if ((uint)GetEntityModel(Game.PlayerPed.Handle) != modelHash) // only change skins if the player is not yet using the new skin.
                {
                    // check if the ped is in a vehicle.
                    bool wasInVehicle = Game.PlayerPed.IsInVehicle();
                    Vehicle veh = Game.PlayerPed.CurrentVehicle;
                    VehicleSeat seat = Game.PlayerPed.SeatIndex;

                    int lastArmorValue = Game.PlayerPed.Armor;

                    // set the model
                    SetPlayerModel(Game.Player.Handle, modelHash);

                    // warp ped into vehicle if the player was in a vehicle.
                    if (wasInVehicle && veh != null && seat != VehicleSeat.None)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, true);
                        int tmpTimer = GetGameTimer();
                        while (!Game.PlayerPed.IsInVehicle(veh))
                        {
                            // if it takes too long, stop trying to teleport.
                            if (GetGameTimer() - tmpTimer > 1000)
                            {
                                break;
                            }
                            ClearPedTasks(Game.PlayerPed.Handle);
                            await Delay(0);
                            TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, veh.Handle, (int)seat);
                        }
                        FreezeEntityPosition(Game.PlayerPed.Handle, false);
                    }

                    // restore armor.
                    Game.PlayerPed.Armor = lastArmorValue;
                }

                // Reset some stuff.
                SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                ClearAllPedProps(Game.PlayerPed.Handle);
                ClearPedDecorations(Game.PlayerPed.Handle);
                ClearPedFacialDecorations(Game.PlayerPed.Handle);

                if (pedCustomizationOptions.version == 1)
                {
                    var ped = Game.PlayerPed.Handle;
                    for (var drawable = 0; drawable < 21; drawable++)
                    {
                        SetPedComponentVariation(ped, drawable, pedCustomizationOptions.drawableVariations[drawable],
                            pedCustomizationOptions.drawableVariationTextures[drawable], 1);
                    }

                    for (var i = 0; i < 21; i++)
                    {
                        int prop = pedCustomizationOptions.props[i];
                        int propTexture = pedCustomizationOptions.propTextures[i];
                        if (prop == -1 || propTexture == -1)
                        {
                            ClearPedProp(ped, i);
                        }
                        else
                        {
                            SetPedPropIndex(ped, i, prop, propTexture, true);
                        }
                    }
                }
                else if (pedCustomizationOptions.version == -1)
                {
                    // do nothing.
                }
                else
                {
                    // notify user of unsupported version
                    Notify.Error("This is an unsupported saved ped version. Cannot restore appearance. :(");
                }
                if (keepWeapons)
                {
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false);
                }
                if (modelHash == (uint)GetHashKey("mp_f_freemode_01") || modelHash == (uint)GetHashKey("mp_m_freemode_01"))
                {
                    var headBlendData = Game.PlayerPed.GetHeadBlendData();
                    if (pedCustomizationOptions.version == -1)
                    {
                        SetPedHeadBlendData(Game.PlayerPed.Handle, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);
                        while (!HasPedHeadBlendFinished(Game.PlayerPed.Handle))
                        {
                            await Delay(0);
                        }
                    }
                }
                SetModelAsNoLongerNeeded(modelHash);
            }
            else
            {
                Notify.Error(CommonErrors.InvalidModel);
            }
        }

        /// <summary>
        /// Set the player model by asking for user input.
        /// </summary>
        public static async void SpawnPedByName()
        {
            string input = await GetUserInput(windowTitle: "Enter Ped Model Name", maxInputLength: 30);
            if (!string.IsNullOrEmpty(input))
            {
                await SetPlayerSkin((uint)GetHashKey(input), new PedInfo() { version = -1 });
            }
            else
            {
                Notify.Error(CommonErrors.InvalidModel);
            }
        }
        #endregion

        #region Save Ped Model + Customizations
        /// <summary>
        /// Saves the current player ped.
        /// </summary>
        public static async void SavePed(string forceName = null)
        {
            string name = forceName;
            if (string.IsNullOrEmpty(name))
            {
                // Get the save name.
                name = await GetUserInput(windowTitle: "Enter a ped save name", maxInputLength: 30);
            }

            // If the save name is not invalid.
            if (!string.IsNullOrEmpty(name))
            {
                // Create a dictionary to store all data in.
                PedInfo data = new PedInfo();

                // Get the ped.
                int ped = Game.PlayerPed.Handle;

                data.version = 1;
                // Get the ped model hash & add it to the dictionary.
                uint model = (uint)GetEntityModel(ped);
                data.model = model;

                // Loop through all drawable variations.
                var drawables = new Dictionary<int, int>();
                var drawableTextures = new Dictionary<int, int>();
                for (var i = 0; i < 21; i++)
                {
                    int drawable = GetPedDrawableVariation(ped, i);
                    int textureVariation = GetPedTextureVariation(ped, i);
                    drawables.Add(i, drawable);
                    drawableTextures.Add(i, textureVariation);
                }
                data.drawableVariations = drawables;
                data.drawableVariationTextures = drawableTextures;

                var props = new Dictionary<int, int>();
                var propTextures = new Dictionary<int, int>();
                // Loop through all prop variations.
                for (var i = 0; i < 21; i++)
                {
                    int prop = GetPedPropIndex(ped, i);
                    int propTexture = GetPedPropTextureIndex(ped, i);
                    props.Add(i, prop);
                    propTextures.Add(i, propTexture);
                }
                data.props = props;
                data.propTextures = propTextures;

                data.isMpPed = (model == (uint)GetHashKey("mp_f_freemode_01") || model == (uint)GetHashKey("mp_m_freemode_01"));

                // Try to save the data, and save the result in a variable.
                bool saveSuccessful = false;
                if (name == "vMenu_tmp_saved_ped")
                {
                    saveSuccessful = StorageManager.SavePedInfo(name, data, true);
                }
                else
                {
                    saveSuccessful = StorageManager.SavePedInfo("ped_" + name, data, false);
                }


                if (name != "vMenu_tmp_saved_ped") // only send a notification if the save wasn't triggered because the player died.
                {
                    // If the save was successfull.
                    if (saveSuccessful)
                    {
                        Notify.Success("Ped saved.");
                    }
                    // Save was not successfull.
                    else
                    {
                        Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: name);
                    }
                }

            }
            // User cancelled the saving or they did not enter a valid name.
            else
            {
                Notify.Error(CommonErrors.InvalidSaveName);
            }
        }
        #endregion

        #region Load Saved Ped
        /// <summary>
        /// Load the saved ped and spawn it.
        /// </summary>
        /// <param name="savedName">The ped saved name</param>
        public static async void LoadSavedPed(string savedName, bool restoreWeapons)
        {
            if (savedName != "vMenu_tmp_saved_ped")
            {
                PedInfo pi = StorageManager.GetSavedPedInfo("ped_" + savedName);
                Log(JsonConvert.SerializeObject(pi));
                await SetPlayerSkin(pi.model, pi, restoreWeapons);
            }
            else
            {
                PedInfo pi = StorageManager.GetSavedPedInfo(savedName);
                Log(JsonConvert.SerializeObject(pi));
                await SetPlayerSkin(pi.model, pi, restoreWeapons);
                DeleteResourceKvp("vMenu_tmp_saved_ped");
            }

        }

        /// <summary>
        /// Checks if the ped is saved from before the player died.
        /// </summary>
        /// <returns></returns>
        public static bool IsTempPedSaved()
        {
            if (!string.IsNullOrEmpty(GetResourceKvpString("vMenu_tmp_saved_ped")))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region saved ped json string to ped info
        /// <summary>
        /// Load and convert json ped info into PedInfo struct.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public static PedInfo JsonToPedInfo(string json, string saveName)
        {
            return JsonConvert.DeserializeObject<PedInfo>(json);
        }
        #endregion

        #region Save and restore weapon loadouts when changing models
        //private struct WeaponInfo
        //{
        //    public int Ammo;
        //    public uint Hash;
        //    public List<uint> Components;
        //    public int Tint;
        //}

        //private static List<WeaponInfo> weaponsList = new List<WeaponInfo>();

        ///// <summary>
        ///// Saves all current weapons and components.
        ///// </summary>
        //public static async Task SaveWeaponLoadout()
        //{
        //    weaponsList.Clear();
        //    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
        //    {
        //        if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Hash, false))
        //        {
        //            List<uint> components = new List<uint>();
        //            if (vw.Components != null && vw.Components.Count > 0)
        //            {
        //                foreach (var c in vw.Components)
        //                {
        //                    if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, vw.Hash, c.Value))
        //                    {
        //                        components.Add(c.Value);
        //                    }
        //                }
        //            }
        //            weaponsList.Add(new WeaponInfo()
        //            {
        //                Ammo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, vw.Hash),
        //                Components = components,
        //                Hash = vw.Hash,
        //                Tint = GetPedWeaponTintIndex(Game.PlayerPed.Handle, vw.Hash)
        //            });

        //        }
        //    }
        //    await Delay(0);
        //}

        ///// <summary>
        ///// Restores all weapons and components
        ///// </summary>
        //public static async void RestoreWeaponLoadout()
        //{
        //    await Delay(0);
        //    if (weaponsList.Count > 0)
        //    {
        //        foreach (WeaponInfo wi in weaponsList)
        //        {
        //            GiveWeaponToPed(Game.PlayerPed.Handle, wi.Hash, wi.Ammo, false, false);
        //            if (wi.Components.Count > 0)
        //            {
        //                foreach (var wc in wi.Components)
        //                {
        //                    GiveWeaponComponentToPed(Game.PlayerPed.Handle, wi.Hash, wc);
        //                }
        //            }
        //            // sometimes causes problems if this is not manually set.
        //            SetPedAmmo(Game.PlayerPed.Handle, wi.Hash, wi.Ammo);
        //            SetPedWeaponTintIndex(Game.PlayerPed.Handle, wi.Hash, wi.Tint);
        //        }
        //    }
        //}
        #endregion

        #region Get "Header" Menu Item
        /// <summary>
        /// Get a header menu item (text-centered, disabled MenuItem)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static MenuItem GetSpacerMenuItem(string title, string description = null)
        {
            string output = "~h~";
            int length = title.Length;
            int totalSize = 80 - length;

            for (var i = 0; i < totalSize / 2 - (length / 2); i++)
            {
                output += " ";
            }
            output += title;
            MenuItem item = new MenuItem(output, description ?? "")
            {
                Enabled = false
            };
            return item;
        }
        #endregion

        #region Log Function
        /// <summary>
        /// Print data to the console and save it to the CitizenFX.log file. Only when vMenu debugging mode is enabled.
        /// </summary>
        /// <param name="data"></param>
        public static void Log(string data)
        {
            if (MainMenu.DebugMode) Debug.WriteLine(@data);
        }
        #endregion

        #region Get Currently Opened Menu
        /// <summary>
        /// Returns the currently opened menu, if no menu is open, it'll return null.
        /// </summary>
        /// <returns></returns>
        public static Menu GetOpenMenu()
        {
            return MenuController.GetCurrentMenu();
        }
        #endregion

        #region Weapon Options
        /// <summary>
        /// Set the ammo for all weapons in inventory to the custom amount entered by the user.
        /// </summary>
        public static async void SetAllWeaponsAmmo()
        {
            string inputAmmo = await GetUserInput(windowTitle: "Enter Ammo Amount", defaultText: "100");
            if (!string.IsNullOrEmpty(inputAmmo))
            {
                if (int.TryParse(inputAmmo, out int ammo))
                {
                    foreach (ValidWeapon vw in ValidWeapons.WeaponList)
                    {
                        if (HasPedGotWeapon(PlayerPedId(), vw.Hash, false))
                        {
                            SetPedAmmo(Game.PlayerPed.Handle, vw.Hash, ammo);
                        }
                    }
                }
                else
                {
                    Notify.Error("You did not enter a valid number.");
                }
            }
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
            }
        }

        /// <summary>
        /// Spawn a weapon by asking the player for the weapon name.
        /// </summary>
        public static async void SpawnCustomWeapon()
        {
            int ammo = 900;
            string inputName = await GetUserInput(windowTitle: "Enter Weapon Model Name", maxInputLength: 30);
            if (!string.IsNullOrEmpty(inputName))
            {
                if (!ValidWeapons.weaponPermissions.ContainsKey(inputName.ToLower()))
                {
                    if (!IsAllowed(Permission.WPSpawn))
                    {
                        Notify.Error("Sorry, you do not have permission to spawn this weapon.");
                        return;
                    }
                }
                else
                {
                    if (!IsAllowed(ValidWeapons.weaponPermissions[inputName.ToLower()]))
                    {
                        Notify.Error("Sorry, you are not allowed to spawn that weapon by name because it's a restricted weapon.");
                        return;
                    }
                }

                var model = (uint)GetHashKey(inputName.ToUpper());

                if (IsWeaponValid(model))
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, model, ammo, false, true);
                    Notify.Success("Added weapon to inventory.");
                }
                else
                {
                    Notify.Error($"This ({inputName.ToString()}) is not a valid weapon model name, or the model hash ({model.ToString()}) could not be found in the game files.");
                }

            }
            else
            {
                Notify.Error($"This ({inputName.ToString()}) is not a valid weapon model name.");
            }
        }
        #endregion

        #region Weapon Loadouts
        /// <summary>
        /// Gets a saved weapons loadout.
        /// </summary>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public static List<ValidWeapon> GetSavedWeaponLoadout(string saveName)
        {
            if (saveName == "vmenu_temp_weapons_loadout_before_respawn")
            {
                string name = GetResourceKvpString("vmenu_string_default_loadout") ?? saveName;

                string kvp = GetResourceKvpString(name) ?? GetResourceKvpString("vmenu_temp_weapons_loadout_before_respawn");

                // if not allowed to use loadouts, fall back to normal restoring of weapons.
                if (MainMenu.WeaponLoadoutsMenu == null || !MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn || !IsAllowed(Permission.WLEquipOnRespawn))
                {
                    kvp = GetResourceKvpString("vmenu_temp_weapons_loadout_before_respawn");

                    if (!MainMenu.MiscSettingsMenu.RestorePlayerWeapons || !IsAllowed(Permission.MSRestoreWeapons))
                    {
                        // return because normal weapon restoring is not enabled or not allowed.
                        return new List<ValidWeapon>();
                    }
                }

                if (string.IsNullOrEmpty(kvp))
                {
                    return new List<ValidWeapon>();
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<ValidWeapon>>(kvp);
                }
            }
            else
            {
                string kvp = GetResourceKvpString(saveName.StartsWith("vmenu_string_saved_weapon_loadout_") ? saveName : "vmenu_string_saved_weapon_loadout_" + saveName);
                if (string.IsNullOrEmpty(kvp))
                {
                    return new List<ValidWeapon>();
                }
                return JsonConvert.DeserializeObject<List<ValidWeapon>>(kvp);
            }
        }

        /// <summary>
        /// Spawns a saved weapons loadout.
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="appendWeapons"></param>
        public static async Task SpawnWeaponLoadoutAsync(string saveName, bool appendWeapons)
        {
            var loadout = GetSavedWeaponLoadout(saveName);
            Log(JsonConvert.SerializeObject(loadout));
            if (loadout.Count > 0)
            {
                // Remove all current weapons if we're not supposed to append this loadout.
                if (!appendWeapons)
                {
                    Game.PlayerPed.Weapons.RemoveAll();
                }

                // Check if any weapon is not allowed.
                if (loadout.Any((wp) => !IsAllowed(wp.Perm)))
                {
                    Notify.Alert("One or more weapon(s) in this saved loadout are not allowed on this server. Those weapons will not be loaded.");
                }

                foreach (ValidWeapon w in loadout)
                {
                    if (IsAllowed(w.Perm))
                    {
                        // Give the weapon
                        GiveWeaponToPed(Game.PlayerPed.Handle, w.Hash, w.CurrentAmmo > 0 ? w.CurrentAmmo : w.GetMaxAmmo, false, true);

                        // Add components
                        if (w.Components.Count > 0)
                        {
                            foreach (var wc in w.Components)
                            {
                                if (DoesWeaponTakeWeaponComponent(w.Hash, wc.Value))
                                {
                                    GiveWeaponComponentToPed(Game.PlayerPed.Handle, w.Hash, wc.Value);
                                    int timer = GetGameTimer();
                                    while (!HasPedGotWeaponComponent(Game.PlayerPed.Handle, w.Hash, wc.Value))
                                    {
                                        await Delay(0);
                                        if (GetGameTimer() - timer > 1000)
                                        {
                                            // took too long
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // Set tint
                        SetPedWeaponTintIndex(Game.PlayerPed.Handle, w.Hash, w.CurrentTint);

                        if (w.CurrentAmmo > 0)
                        {
                            while (GetAmmoInPedWeapon(Game.PlayerPed.Handle, w.Hash) < 1)
                            {
                                await Delay(0);
                                AddAmmoToPed(Game.PlayerPed.Handle, w.Hash, w.CurrentAmmo);
                                SetAmmoInClip(Game.PlayerPed.Handle, w.Hash, w.AmmoInClip);
                                Log($"waiting for ammo in {w.Name}");
                            }
                        }
                    }
                }

                // Set the current weapon to 'unarmed'.
                SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_unarmed"), true);

                if (saveName != "vmenu_temp_weapons_loadout_before_respawn")
                    Notify.Success("Weapon loadout spawned.");
            }
        }

        /// <summary>
        /// Saves all current weapons the ped has. It does not check if the save already exists!
        /// </summary>
        /// <returns>A bool indicating if the save was successful</returns>
        public static bool SaveWeaponLoadout(string saveName)
        {
            // Stop if the savename is invalid.
            if (string.IsNullOrEmpty(saveName))
            {
                return false;
            }

            List<ValidWeapon> pedWeapons = new List<ValidWeapon>();

            // Loop through all possible weapons.
            foreach (var vw in ValidWeapons.WeaponList)
            {
                // Check if the ped has that specific weapon.
                if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Hash, false))
                {
                    // Create the weapon data with basic info.
                    ValidWeapon weapon = new ValidWeapon()
                    {
                        Hash = vw.Hash,
                        CurrentTint = GetPedWeaponTintIndex(Game.PlayerPed.Handle, vw.Hash),
                        Name = vw.Name,
                        Perm = vw.Perm,
                        SpawnName = vw.SpawnName,
                        Components = new Dictionary<string, uint>()
                    };

                    weapon.CurrentAmmo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, vw.Hash);
                    var ammoInClip = 0;
                    GetAmmoInClip(Game.PlayerPed.Handle, vw.Hash, ref ammoInClip);
                    weapon.AmmoInClip = ammoInClip;


                    // Check for and add components if applicable.
                    foreach (var comp in vw.Components)
                    {
                        if (DoesWeaponTakeWeaponComponent(weapon.Hash, comp.Value))
                        {
                            if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, vw.Hash, comp.Value))
                            {
                                weapon.Components.Add(comp.Key, comp.Value);
                            }
                        }
                    }

                    // Add the weapon info to the list.
                    pedWeapons.Add(weapon);
                }
            }

            // Convert the weapons list to json string.
            string json = JsonConvert.SerializeObject(pedWeapons);

            // Save it.
            SetResourceKvp(saveName, json);

            // If the saved value is the same as the string we just provided, then the save was successful.
            if ((GetResourceKvpString(saveName) ?? "{}") == json)
            {
                Log("weapons save good.");
                return true;
            }

            // Save was unsuccessful.
            return false;
        }
        #endregion

        #region Set Player Walking Style
        /// <summary>
        /// Sets the walking style for this player.
        /// </summary>
        /// <param name="walkingStyle"></param>
        public static async void SetWalkingStyle(string walkingStyle)
        {
            if (IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_f_freemode_01")) || IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01")))
            {
                bool isPedMale = IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01"));
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, 1f);
                ClearPedAlternateWalkAnim(Game.PlayerPed.Handle, 1f);
                string animDict = null;
                if (walkingStyle == "Injured")
                {
                    animDict = isPedMale ? "move_m@injured" : "move_f@injured";
                }
                else if (walkingStyle == "Tough Guy")
                {
                    animDict = isPedMale ? "move_m@tough_guy@" : "move_f@tough_guy@";
                }
                else if (walkingStyle == "Femme")
                {
                    animDict = isPedMale ? "move_m@femme@" : "move_f@femme@";
                }
                else if (walkingStyle == "Gangster")
                {
                    animDict = isPedMale ? "move_m@gangster@a" : "move_f@gangster@ng";
                }
                else if (walkingStyle == "Posh")
                {
                    animDict = isPedMale ? "move_m@posh@" : "move_f@posh@";
                }
                else if (walkingStyle == "Sexy")
                {
                    animDict = isPedMale ? null : "move_f@sexy@a";
                }
                else if (walkingStyle == "Business")
                {
                    animDict = isPedMale ? null : "move_f@business@a";
                }
                else if (walkingStyle == "Drunk")
                {
                    animDict = isPedMale ? "move_m@drunk@a" : "move_f@drunk@a";
                }
                else if (walkingStyle == "Hipster")
                {
                    animDict = isPedMale ? "move_m@hipster@a" : null;
                }
                if (animDict != null)
                {
                    if (!HasAnimDictLoaded(animDict))
                    {
                        RequestAnimDict(animDict);
                        while (!HasAnimDictLoaded(animDict))
                        {
                            await Delay(0);
                        }
                    }
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, animDict, "idle", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, animDict, "walk", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, animDict, "run", 1f, true);
                }
                else if (walkingStyle != "Normal")
                {
                    if (isPedMale)
                    {
                        Notify.Error(CommonErrors.WalkingStyleNotForMale);
                    }
                    else
                    {
                        Notify.Error(CommonErrors.WalkingStyleNotForFemale);
                    }
                }
            }
            else
            {
                Notify.Error("This feature only supports the multiplayer freemode male/female ped models.");
            }
        }
        #endregion

        #region Disable Movement Controls
        /// <summary>
        /// Disables all movement and camera related controls this frame.
        /// </summary>
        /// <param name="disableMovement"></param>
        /// <param name="disableCameraMovement"></param>
        public static void DisableMovementControlsThisFrame(bool disableMovement, bool disableCameraMovement)
        {
            if (disableMovement)
            {
                Game.DisableControlThisFrame(0, Control.MoveDown);
                Game.DisableControlThisFrame(0, Control.MoveDownOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeft);
                Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeftRight);
                Game.DisableControlThisFrame(0, Control.MoveRight);
                Game.DisableControlThisFrame(0, Control.MoveRightOnly);
                Game.DisableControlThisFrame(0, Control.MoveUp);
                Game.DisableControlThisFrame(0, Control.MoveUpDown);
                Game.DisableControlThisFrame(0, Control.MoveUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleFlyMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDown);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDownOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeft);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeftRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRightOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUp);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                Game.DisableControlThisFrame(0, Control.VehicleSubMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.Duck);
                Game.DisableControlThisFrame(0, Control.SelectWeapon);
            }
            if (disableCameraMovement)
            {
                Game.DisableControlThisFrame(0, Control.LookBehind);
                Game.DisableControlThisFrame(0, Control.LookDown);
                Game.DisableControlThisFrame(0, Control.LookDownOnly);
                Game.DisableControlThisFrame(0, Control.LookLeft);
                Game.DisableControlThisFrame(0, Control.LookLeftOnly);
                Game.DisableControlThisFrame(0, Control.LookLeftRight);
                Game.DisableControlThisFrame(0, Control.LookRight);
                Game.DisableControlThisFrame(0, Control.LookRightOnly);
                Game.DisableControlThisFrame(0, Control.LookUp);
                Game.DisableControlThisFrame(0, Control.LookUpDown);
                Game.DisableControlThisFrame(0, Control.LookUpOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookDownOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftRight);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpDown);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook2);
                Game.DisableControlThisFrame(0, Control.VehicleLookBehind);
                Game.DisableControlThisFrame(0, Control.VehicleLookLeft);
                Game.DisableControlThisFrame(0, Control.VehicleLookRight);
                Game.DisableControlThisFrame(0, Control.NextCamera);
                Game.DisableControlThisFrame(0, Control.VehicleFlyAttackCamera);
                Game.DisableControlThisFrame(0, Control.VehicleCinCam);
            }
        }
        #endregion

        #region Set Correct Blip
        /// <summary>
        /// Sets the correct blip sprite for the specific ped and blip.
        /// This is the (old) backup method for setting the sprite if the decorators version doesn't work.
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="blip"></param>
        public static void SetCorrectBlipSprite(int ped, int blip)
        {
            if (IsPedInAnyVehicle(ped, false))
            {
                int vehicle = GetVehiclePedIsIn(ped, false);
                int blipSprite = BlipInfo.GetBlipSpriteForVehicle(vehicle);
                if (GetBlipSprite(blip) != blipSprite)
                {
                    SetBlipSprite(blip, blipSprite);
                }

            }
            else
            {
                SetBlipSprite(blip, 1);
            }
        }
        #endregion

        #region Get safe player name
        /// <summary>
        /// Returns a properly formatted and escaped player name for notifications.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSafePlayerName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            return name.Replace("^", @"\^").Replace("~", @"\~").Replace("<", "«").Replace(">", "»");
        }
        #endregion
    }
}
