using Masticore;

namespace Soundbite
{
    /***********************************************************************************************
     * WARNING: Organization settings are sent client-side.  Any secure data must be marked with
     *  JsonIgnore attribute to avoid being sent out and compromising security.
     **********************************************************************************************/

    /// <summary>
    /// Defines session-type specific transcription settings.
    /// </summary>
    [CodeGenModel]
    public class SessionTypeTranscriptionSettings
    {
        public SessionType SessionType { get; set; }
        public bool Enabled { get; set; }
        public bool OnByDefault { get; set; }
    }
}