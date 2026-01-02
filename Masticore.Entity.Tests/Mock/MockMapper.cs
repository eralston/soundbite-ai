using AutoMapper;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Masticore.Entity.Tests
{
    public class MockMapper
    {
        private static IMapper _instance;

        public static IMapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = BuildInstance(true);
                    }
                    catch
                    {
                        _instance = BuildInstance(false);
                    }
                }
                return _instance;

            }
        }

        //NOTE: something very strange started occuring with the mock mapper after checking in
        // the change that added EnvSettings to the SbDb. Tons of masticore tests were failing
        // because of a reflection issue on the getter for EnvSettings.  No idea what the underlying
        // cause was, but the workaround here is to try to load it like normal and it fails then
        // go back and load it without the Soundbite.Entity assembly reference.

        private static IMapper BuildInstance(bool allowEntityAssembly)
        {
            List<Assembly> assembliesToScan = new List<Assembly>() { typeof(MappingProfile).Assembly };
            if (allowEntityAssembly)
            {
                Type entityProfile = Type.GetType("Soundbite.Entity.MappingProfile, Soundbite.Entity");
                if (entityProfile != null)
                {
                    assembliesToScan.Add(entityProfile.Assembly);
                }
            }

            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(assembliesToScan);
            });
            return config.CreateMapper();
        }

    }
}