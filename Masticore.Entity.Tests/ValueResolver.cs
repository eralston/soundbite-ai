using Microsoft.EntityFrameworkCore;
using Moq;
using System;

namespace Masticore.Entity.Tests
{
    public class ValueResolver<TParent, T, TInfrastructure, TDbContext>
        where TParent : TestBuilder<TInfrastructure, TDbContext>
        where T : class
        where TDbContext : DbContext
        where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
    {
        /// <summary>
        /// Sets the resolver to be a <see cref="Mock{T}"/> object by default
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="useMock"></param>
        public ValueResolver(TParent parent, bool useMock = true)
        {
            Parent = parent;
            UseMock = useMock;
            if (UseMock)
            {
                Mock = new Mock<T>();
            }
        }

        /// <summary>
        /// Sets the resolved to call <see cref="ValueFunc"/> when someone asks for an instance
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="valueFunc"></param>
        public ValueResolver(TParent parent, Func<TParent, T> valueFunc)
            : this(parent)
        {
            ValueFunc = valueFunc;
        }

        #region Properties

        private TParent Parent { get; set; }

        private T Instance { get; set; }
        private Func<TParent, T> ValueFunc { get; set; }

        public bool UseMock { get; }

        public Mock<T> Mock { get; set; }

        public T Value
        {
            get
            {
                T result = Instance;
                if (Instance == null)
                {
                    if (ValueFunc != null)
                    {
                        result = ValueFunc(Parent);
                        if (IsSingleton)
                        {
                            Instance = result;
                        }
                    }
                    else if (UseMock)
                    {
                        // If we have nothing, then roll with an empty Mock
                        Instance = Mock.Object;
                        result = Instance;
                    }
                    else
                    {
                        // If we have neither a ValueFunc nor want to make a mock, then this value will always be null!
                        // I hope you're proud of what you've done - expect fireworks unless that's really what you meant
                    }
                }
                else
                {
                    result = Instance;
                    Instance = IsSingleton ? Instance : null;
                }
                return result;
            }
        }

        #endregion

        public TActual Use<TActual>(TActual value, bool isSingleton = true)
            where TActual : T
        {
            Instance = value;
            IsSingleton = isSingleton;
            return value;
        }

        public void Use(Func<TParent, T> valueFunc, bool isSingleton = true)
        {
            Instance = default(T);
            ValueFunc = valueFunc;
            IsSingleton = isSingleton;
        }

        public TReturn As<TReturn>()
            where TReturn : class
        {
            return Value as TReturn;
        }


        public bool IsSingleton { get; set; } = true;

    }
}
