using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Soundbite.CmdLine
{
    internal class ActionService
    {
        #region Properties

        public IList<ActionInfo> AllActions { get; } = new List<ActionInfo>();

        #endregion

        /// <summary>
        /// Scans all of the assemblies in the current application domain looking for actions. Any
        /// actions that are found along the way are added to the <see cref="AllActions"/> list.
        /// </summary>
        public void ScanAssembliesForActions()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Utils.IgnoreError(() =>
                {
                    foreach (Type t in assembly.GetTypes())
                    {
                        CmdLineActionInfoAttribute attrib = t.GetCustomAttribute<CmdLineActionInfoAttribute>();
                        if (attrib != null)
                        {
                            AllActions.Add(new ActionInfo()
                            {
                                Name = attrib.Name,
                                Description = attrib.Description,
                                TypeRef = t
                            });
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Retrieves an action by name.
        /// </summary>
        /// <param name="actionName">Name of the action to retrieve.</param>
        /// <returns>a reference to the action if found, otherwise <c>null</c>.</returns>
        public ICmdLineAction GetActionByName(string actionName)
        {
            ActionInfo actionInfo = AllActions.Where(i =>
                 string.Equals(i.Name, actionName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            ICmdLineAction result = actionInfo != null
                ? (ICmdLineAction)Activator.CreateInstance(actionInfo.TypeRef)
                : null;

            return result;
        }

        /// <summary>
        /// Responsible for converting a string value into a typed value.
        /// </summary>
        /// <param name="input">String-based value of the parameter.</param>
        public void SetValue(Parameter param, string input)
        {
            switch (param.ValueType.Name)
            {
                case "String":
                    (param as Parameter<string>).Value = input;
                    break;
                case "Version":
                    (param as Parameter<Version>).Value = new Version(input);
                    break;
            }
        }

    }
}
