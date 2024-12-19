using ProtoBuf.Meta;
using SCCMHound.src.models;
using SharpHoundCommonLib.OutputTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Group = SharpHoundCommonLib.OutputTypes.Group;

namespace SCCMHound.src
{
    public class HelperUtilities
    {
        public static string getDomainSidFromUserSid(string sid)
        {
            Match result = Regex.Match(sid, @"^(.*)(?=-)");
            return result.Value;
        }

        public static Dictionary<string, User> createLookupTableUsers(List<User> collection)
        {
            Dictionary<string, User> lookupTable = new Dictionary<string, User>();
            foreach (User element in collection)
            {
                try
                {
                    lookupTable.Add(element.Properties["sccmUniqueUserName"].ToString().ToLower(), element);
                }
                // There may be duplicates with the same name in the SCCM dataset. TODO: workout which one has the most active/accurate data and only store that
                catch (ArgumentException ex)
                {
                    Debug.Print(ex.ToString());
                }
                catch (KeyNotFoundException ex) // handles users which dont have a sccmUniqueUserName which is used for session correlation
                {
                    Debug.Print(ex.ToString());
                }
            }

            return lookupTable;
        }

        public static Dictionary<string, Group> createLookupTableGroups(List<SharpHoundCommonLib.OutputTypes.Group> collection)
        {
            Dictionary<string, Group> lookupTable = new Dictionary<string, Group>();
            foreach (Group element in collection)
            {
                try
                {
                    lookupTable.Add(element.Properties["name"].ToString().ToLower(), element);
                }
                // There may be duplicates with the same name in the SCCM dataset. TODO: workout which one has the most active/accurate data and only store that
                catch (ArgumentException ex)
                {
                    Debug.Print(ex.ToString());
                }
                catch (KeyNotFoundException ex) // handles users which dont have a sccmUniqueUserName which is used for session correlation
                {
                    Debug.Print(ex.ToString());
                }
            }

            return lookupTable;
        }



        public static Dictionary<string, ComputerExt> createLookupTableComputers(List<ComputerExt> collection)
        {
            Dictionary<string, ComputerExt> lookupTable = new Dictionary<string, ComputerExt>();
            foreach (ComputerExt element in collection)
            {
                try
                {
                    lookupTable.Add(element.Properties["sccmName"].ToString(), element);
                }

                // There may be duplicates with the same name in the SCCM dataset. TODO: workout which one has the most active/accurate data and only store that
                catch (ArgumentException ex)
                {
                    Debug.Print(ex.ToString());
                }

            }

            return lookupTable;
        }

        // Revisit
        public static void ConfigureRttModel(RuntimeTypeModel rttModel, Type type)
        {
            if (rttModel.CanSerialize(type)) return;

            var metaType = rttModel.Add(type, false);
            int counter = 1;

            foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
            {
                
                var dataField = metaType.Add(counter++, property.Name);
                
                var childType = property.PropertyType;

                if (childType.IsArray)
                {
                    childType = childType.GetElementType();
                }

                if (childType.IsGenericType && childType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    childType = childType.GetGenericArguments()[0];
                }

                if (!childType.IsPrimitive && childType != typeof(string) && !rttModel.CanSerialize(childType))
                {
                    ConfigureRttModel(rttModel, childType); // recursive call to serialize child type
                }

            }
        }
    }
}
