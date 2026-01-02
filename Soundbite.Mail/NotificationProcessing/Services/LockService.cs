using System;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Serivces used for managing a syncronization lock.
    /// </summary>
    public class LockService
    {
        #region Fields

        private object _lockObj;
        private readonly Func<Task<object>> _lockFunc;

        #endregion

        #region Properties

        public object LockObj
        {
            get
            {
                if (_lockObj == null)
                {
                    if (_lockFunc != null)
                    {
                        Task<object> task = _lockFunc();
                        _lockObj = task.Result;
                    }
                    else
                    {
                        _lockObj = new object();
                    }
                }
                return _lockObj;
            }
            set => _lockObj = value;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="LockService"/> instance.
        /// </summary>
        /// <param name="lockObj">Object used to manage lock syncronization.</param>
        public LockService(object lockObj)
        {
            _lockObj = lockObj;
        }

        /// <summary>
        /// Instantiates a new <see cref="LockService"/> instance.
        /// </summary>
        /// <param name="lockFunc">Function used to acquire object used to manage lock syncronization.</param>
        public LockService(Func<Task<object>> lockFunc)
        {
            _lockFunc = lockFunc;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs an action with the database locked.
        /// </summary>
        /// <param name="lockedAction">Action to perform when a lock has been established.</param>
        public void Lock(Action lockedAction)
        {
            lock (LockObj)
            {
                if (lockedAction != null)
                {
                    lockedAction();
                }
            }
        }

        /// <summary>
        /// Performs a function with the database locked.
        /// </summary>
        /// <typeparam name="TResult">.NET Type of the result of the <paramref name="lockedFunc"/>.</typeparam>
        /// <param name="lockedFunc">Function to execute when a lock has been established.</param>
        /// <returns>the result of the function specified in <paramref name="lockedFunc"/>.</returns>
        public TResult Lock<TResult>(Func<TResult> lockedFunc)
        {
            TResult result;
            lock (LockObj)
            {
                result = lockedFunc != null ? lockedFunc() : default;
            }
            return result;
        }

        /// <summary>
        /// Performs an async action with the database locked.
        /// </summary>
        /// <param name="lockedAction">Action to perform when a lock has been established.</param>
        public void LockAsync(Func<Task> lockedAction)
        {
            lock (LockObj)
            {
                if (lockedAction != null)
                {
                    // Run the action and wait for completion to ensure
                    // lock is maintained for duration of processing.
                    Task task = lockedAction();
                    task.Wait();
                }
            }
        }

        /// <summary>
        /// Performs a function with the database locked.
        /// </summary>
        /// <typeparam name="TResult">.NET Type of the result of the <paramref name="lockedFunc"/>.</typeparam>
        /// <param name="lockedFunc">Function to execute when a lock has been established.</param>
        /// <returns>the result of the function specified in <paramref name="lockedFunc"/>.</returns>
        public TResult LockAsync<TResult>(Func<Task<TResult>> lockedFunc)
        {
            TResult result;
            lock (LockObj)
            {
                if (lockedFunc != null)
                {
                    // Run the function and wait for completion to ensure
                    // lock is maintained for duration of processing.
                    result = lockedFunc().Result;
                }
                else
                {
                    result = default;
                }
            }
            return result;
        }

        #endregion
    }
}