using Masticore.Jobs;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Masticore.Queue
{
    /// <summary>
    /// Base class for <see cref="IJob"/> classes that assumes the message is passed as JSON and the type is just the concrete class name
    /// </summary>
    public abstract class JobBase : IJob
    {
        /// <summary>
        /// Default constructor; called when JSON deserializing
        /// </summary>
        /// <remarks>Child classes can always implement further constructors, but they must maintain a default constructor</remarks>
        public JobBase() { }

        /// <summary>
        /// Gets the unique type name for this variety of job based on the concrete class type
        /// </summary>
        /// <remarks>It should be safe to use the type name since the concrete class should be processed off the same queue</remarks>
        public virtual string Type => TypeNameUtils.TypeNameForObject(this);

        /// <inheritdoc/>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual string Message => this.ToLowerCamelJson();

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public abstract Task Process();
    }
}
