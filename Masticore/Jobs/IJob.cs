using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Masticore.Jobs
{
    /// <summary>
    /// A command object for async processing of work over a queue; must be serializable to JSON
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Gets or sets the unique idenfitier for this job
        /// </summary>
        /// <remarks>This will be null before the job is pending</remarks>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// Gets the unique type name for this variety of job
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        string Type { get; }

        /// <summary>
        /// Gets the stringified data representing this object
        /// </summary>
        /// <remarks>THis is a bit of an implementation detail to the queue and worker, so one day perhaps refune this away from the interface</remarks>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Message { get; }

        /// <summary>
        /// Async do the work for this job
        /// </summary>
        /// <returns></returns>
        Task Process();
    }
}
