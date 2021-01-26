/*
_________________________________________________________________________
|                                                                       |
|██████╗░░█████╗░████████╗  ██████╗░░█████╗░████████╗██╗░░░██╗░█████╗░  |
|██╔══██╗██╔══██╗╚══██╔══╝  ██╔══██╗██╔══██╗╚══██╔══╝╚██╗░██╔╝██╔══██╗  |
|██████╦╝██║░░██║░░░██║░░░  ██████╦╝██║░░██║░░░██║░░░░╚████╔╝░███████║  |
|██╔══██╗██║░░██║░░░██║░░░  ██╔══██╗██║░░██║░░░██║░░░░░╚██╔╝░░██╔══██║  |
|██████╦╝╚█████╔╝░░░██║░░░  ██████╦╝╚█████╔╝░░░██║░░░░░░██║░░░██║░░██║  |
|╚═════╝░░╚════╝░░░░╚═╝░░░  ╚═════╝░░╚════╝░░░░╚═╝░░░░░░╚═╝░░░╚═╝░░╚═╝  |
|______________________________________________________________________ |
|Author: Denis Voitenko.                                                |
|GitHub: https://github.com/DenchickPenchick                            |
|DEV: https://dev.to/denchickpenchick                                   |
|_____________________________Project__________________________________ |
|GitHub: https://github.com/DenchickPenchick/BotBotya                   |
|______________________________________________________________________ |
|© Copyright 2021 Denis Voitenko                                        |
|© Copyright 2021 All rights reserved                                   |
|License: http://opensource.org/licenses/MIT                            |
_________________________________________________________________________
*/

using Discord;
using Discord.WebSocket;
using DiscordBot.Serializable;
using System;
using System.Collections.Generic;

namespace DiscordBot.Providers
{
    public class EconomicProvider
    {
        private SocketGuild Guild { get; set; } 
        public SerializableEconomicGuild EconomicGuild { get => FilesProvider.GetEconomicGuild(Guild); }
        public enum Result { NoRole, RoleAlreadyAdded, Error, Succesfull, NoBalance }

        public EconomicProvider(SocketGuild guild)
        {
            Guild = guild;
        }

        public void SetBalance(IUser user, int count)
        {
            SerializableEconomicGuildUser economicGuildUser = GetEconomicGuildUser(user.Id);           
            var economGuild = EconomicGuild;
            if (economGuild != null && economicGuildUser != null)
            {
                int indexOf = economGuild.SerializableEconomicUsers.IndexOf(economicGuildUser);

                if (economicGuildUser == null)
                    economGuild.SerializableEconomicUsers.Add(new SerializableEconomicGuildUser
                    {
                        Id = user.Id
                    });

                economicGuildUser.Balance = count;
                economGuild.SerializableEconomicUsers[indexOf] = economicGuildUser;

                FilesProvider.RefreshEconomicGuild(economGuild);
            }            
        }

        public void ClearBalance(IUser user)
        {
            SetBalance(user, 0);
        }

        public void MaxBalance(IUser user)
        {
            SetBalance(user, int.MaxValue);
        }

        public void AddBalance(IUser user, int count)
        {
            var economicGuildUser = GetEconomicGuildUser(user.Id);
            if (economicGuildUser != null)
            {
                int newBalance = economicGuildUser.Balance + count;

                SetBalance(user, newBalance);
            }            
        }

        public void MinusBalance(IUser user, int count)
        {
            var economicGuildUser = GetEconomicGuildUser(user.Id);
            if (economicGuildUser != null)
            {
                int newBalance = economicGuildUser.Balance - count;

                SetBalance(user, newBalance);
            }            
        }

        public Result BuyRole(IRole role, IGuildUser user)
        {
            var economGuild = EconomicGuild;

            if (economGuild != null)
            {
                var rolesCosts = economGuild.RolesAndCostList;
                ulong roleId = 0;
                int cost = 0;

                foreach (var roleCost in rolesCosts)
                {
                    if (roleCost.Item1 == role.Id)
                    {
                        roleId = roleCost.Item1;
                        cost = roleCost.Item2;
                    }
                }

                if (roleId > 0)
                    try
                    {
                        var stEconomUser = FilesProvider.GetEconomicGuildUser(user);
                        var economUser = FilesProvider.GetEconomicGuildUser(user);

                        if (economUser.Balance - cost > 0)
                        {
                            user.AddRoleAsync(role).GetAwaiter();
                            economUser.Balance -= cost;
                            economGuild.SerializableEconomicUsers[economGuild.SerializableEconomicUsers.IndexOf(stEconomUser)] = economUser;
                            FilesProvider.RefreshEconomicGuild(economGuild);
                            return Result.Succesfull;
                        }
                        else
                            return Result.NoBalance;
                    }
                    catch (Exception)
                    {
                        return Result.Error;
                    }
                else
                    return Result.NoRole;
            }
            return Result.Succesfull;
        }

        public Result AddRole(IRole role, int cost)
        {
            var economGuild = EconomicGuild;
            if (economGuild != null)
            {
                List<ulong> rolesId = new List<ulong>();

                foreach (var roleCost in EconomicGuild.RolesAndCostList)
                    rolesId.Add(roleCost.Item1);

                if (!rolesId.Contains(role.Id))
                    economGuild.RolesAndCostList.Add((role.Id, cost));
                else
                {
                    int indexOf = rolesId.IndexOf(role.Id);
                    var roleCost = EconomicGuild.RolesAndCostList[indexOf];
                    roleCost.Item2 = cost;
                }

                FilesProvider.RefreshEconomicGuild(economGuild);

                return Result.Succesfull;
            }
            return Result.Succesfull;
        }

        public Result DeleteRole(ulong id)
        {
            var economGuild = EconomicGuild;
            List<ulong> rolesId = new List<ulong>();

            foreach (var roleCost in EconomicGuild.RolesAndCostList)
                rolesId.Add(roleCost.Item1);

            if (rolesId.Contains(id))
            {
                int indexOf = rolesId.IndexOf(id);
                economGuild.RolesAndCostList.RemoveAt(indexOf);
                return Result.Succesfull;
            }
            else
                return Result.NoRole;
        }        

        public SerializableEconomicGuildUser GetEconomicGuildUser(ulong id)
        {
            foreach (var economUser in EconomicGuild.SerializableEconomicUsers)
                if (economUser.Id == id)
                    return economUser;
            return null;
        }
    }
}
