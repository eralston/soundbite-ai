using Masticore;

namespace Soundbite
{
    /***********************************************************************************************
     * WARNING: Organization settings are sent client-side.  Any secure data must be marked with
     *  JsonIgnore attribute to avoid being sent out and compromising security.
     **********************************************************************************************/

    /// <summary>
    /// These are flags that organize what the users can and can't do within an org
    /// </summary>
    /// <remarks>
    /// This is for usability. As of Nov 2021, this is for hiding things in the UI w/o security in the API is NOT yet RBAC in the back-end
    /// </remarks>
    public class OrgPermissions
    {
        /// <summary>
        /// Indicates if the user has access to create a "Public" Soundbite in the UI.
        /// </summary>
        public PersonRole MinRoleToCreatePublic { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_CREATEPUBLIC", PersonRole.Admin);

        /// <summary>
        /// Indicates if the user has access to the "Create Team" functionality in the UI
        /// </summary>
        public PersonRole MinRoleToCreateTeam { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_CREATETEAM", PersonRole.Person);

        /// <summary>
        /// Indicates if the user can access the "Create Session" functionality
        /// </summary>
        /// <remarks>
        /// The user should still be able to record a session if prompted
        /// </remarks>
        public PersonRole MinRoleToCreateSession { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_CREATESESSION", PersonRole.Person);

        /// <summary>
        /// Indicates if the user can access "Create Session" in the context of a team; Org admins should also always be able to do this
        /// </summary>
        public MemberRole MinRoleToCreateTeamSession { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_CREATETEAMSESSION", MemberRole.Member);

        /// <summary>
        /// The user can invite people to the org
        /// </summary>
        public PersonRole MinRoleForOrgInvite { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_INVITEPERSON", PersonRole.Person);

        /// <summary>
        /// The user can invite people to a team; Org admins should also always be able to do this
        /// </summary>
        public MemberRole MinRoleForTeamInvite { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_INVITEMEMBER", MemberRole.Member);

        /// <summary>
        /// What level of users can make public Soundbites at the team level
        /// </summary>
        public MemberRole MinRoleToCreateTeamPublic { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_CREATETEAMPUBLIC", MemberRole.Owner);

        /// <summary>
        /// The user is allowed to the audience (listeners only) for a given Soundbite; Org admins should also always be able to do this
        /// </summary>
        public PersonRole MinRoleForAudience { get; set; } = MasticoreExtensions.GetEnvEnum("SB_PERMS_SHOWAUDIENCE", PersonRole.Person);
    }
}
