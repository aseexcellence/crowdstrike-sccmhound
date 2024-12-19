using SCCMHound.src.models;
using SharpHoundCommonLib.OutputTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SCCMHound.src
{
    class LocalAdminsResolver
    {
        public LocalAdminsResolver(List<ComputerExt> computers, List<Group> groups, List<User> users, List<LocalAdmin> localAdmins)
        {
            Dictionary<string, ComputerExt> computerLookupByResourceName = HelperUtilities.createLookupTableComputers(computers);
            Dictionary<string, User> userLookupByUniqueUserName = HelperUtilities.createLookupTableUsers(users);
            Dictionary<string, Group> groupLookupByGroupName = HelperUtilities.createLookupTableGroups(groups);
            // group resolver

            Dictionary<string, ComputerLocalAdmins> computerLocalAdminsLookupByComputer = new Dictionary<string, ComputerLocalAdmins>();

            var computerLocalAdminsList = new List<ComputerLocalAdmins>();

            foreach (LocalAdmin localAdmin in localAdmins)
            {
                if (computerLookupByResourceName.ContainsKey(localAdmin.deviceName))
                {
                    ComputerExt computer = computerLookupByResourceName[localAdmin.deviceName];
                    ComputerLocalAdmins computerLocalAdmins;

                    if (!computerLocalAdminsLookupByComputer.ContainsKey(computer.Properties["name"].ToString()))
                    {

                        computerLocalAdmins = new ComputerLocalAdmins(computer);
                        computerLocalAdminsLookupByComputer[computer.Properties["name"].ToString()] = computerLocalAdmins;
                        computerLocalAdminsList.Add(computerLocalAdmins);


                    }
                    // retrieve object from dictionary
                    else
                    {
                        computerLocalAdmins = computerLocalAdminsLookupByComputer[computer.Properties["name"].ToString()];
                    }


                    if (localAdmin.type.Equals("User"))
                    {
                        if (userLookupByUniqueUserName.ContainsKey(localAdmin.name.ToLower()))
                        {
                            User recordUser = userLookupByUniqueUserName[localAdmin.name.ToLower()];
                            computerLocalAdmins.AddAdminUser(recordUser);
                        }
                    }
                    else if (localAdmin.type.Equals("Group"))
                    {
                        string localAdminName = $"{localAdmin.name.Split('\\')[1].ToUpper()}@{computer.Properties["domain"]}".ToLower();
                        if (groupLookupByGroupName.ContainsKey(localAdminName))
                        {
                            Group recordGroup = groupLookupByGroupName[localAdminName];
                            computerLocalAdmins.AddAdminGroup(recordGroup);
                        }
                    }
                }
            }

            foreach (ComputerLocalAdmins computerLocalAdmins in computerLocalAdminsList)
            {
                ComputerExt computer = computerLocalAdmins.computer;
                string groupSID = $"{computer.ObjectIdentifier}-500";
                Group groupObj = new Group
                {
                    ObjectIdentifier = groupSID
                };


                groupObj.Properties.Add("name", $"ADMINISTRATORS@{computer.Properties["name"]}");
                //groups.Add(groupObj); commented out as we don't want to add new BH objects for local groups


                computerLocalAdmins.PopulateComputerExtWithLocalAdminData(groupSID);
            }
        }
    }
}
