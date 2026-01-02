using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Soundbite.Entity;
using Soundbite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Creates a fully loader Session boject based on the INewSession definition
    /// NOTE: This does NOT save it into the database or fire the lifecycle events, it just makes the entities
    /// </summary>
    public class SessionEntityBuilder
    {
        private SbDb Db { get; }
        private UserEntity CurrentUser { get; }
        private OrgSettings? Settings { get; }

        /// <summary>
        /// Cache of Person records for this builder
        /// </summary>
        private readonly Dictionary<string, PersonEntity> People = new Dictionary<string, PersonEntity>();

        /// <summary>
        /// Cache of group records for this builder
        /// </summary>
        private readonly Dictionary<string, GroupEntity> Groups = new Dictionary<string, GroupEntity>();

        public SessionEntityBuilder(SbDb db, UserEntity currentUser = null, OrgSettings orgSettings = null)
        {
            Db = db;
            CurrentUser = currentUser;
            Settings = orgSettings;
        }

        /// <summary>
        /// At least one host must have this role when trying to assign to the individual
        /// </summary>
        protected PersonRole RequiredOrgCreateRole => Settings?.Permissions?.MinRoleToCreateSession ?? PersonRole.Person;

        protected PersonRole RequiredOrgCreatePublicRole => Settings?.Permissions?.MinRoleToCreatePublic ?? PersonRole.Person;

        protected MemberRole RequiredGroupCreateRole => Settings?.Permissions?.MinRoleToCreateTeamSession ?? MemberRole.Member;

        protected MemberRole RequireGroupCreatePublicRole => Settings?.Permissions?.MinRoleToCreateTeamPublic ?? MemberRole.Member;


        protected SessionEntity CreateSession(OrganizationEntity org, Session newSession)
        {
            SessionEntity session = Db.Sessions.CreateResource(CurrentUser);
            session.Name = newSession.Name;
            session.Reminder = newSession.Reminder;
            session.Publish = newSession.Publish;
            session.Limit = newSession.Limit;
            session.SessionType = newSession.SessionType;
            session.SessionSecurity = newSession.SessionSecurity;
            session.SessionCommentPolicy = newSession.SessionCommentPolicy;
            session.Transcribe = newSession.Transcribe;
            // Relationships
            session.Organization = org;
            session.Participants = new List<ParticipantEntity>();
            session.Groups = new List<ParticipantGroupEntity>();
            session.Prompts = new List<PromptEntity>();
            return session;
        }

        protected PromptEntity CreatePrompt(SessionEntity session, string text)
        {
            PromptEntity prompt = Db.Prompts.CreateResource(CurrentUser);
            prompt.Text = text;
            prompt.Session = session;
            prompt.Clips = new List<ClipEntity>();
            session.Prompts.Add(prompt);
            return prompt;
        }

        protected ParticipantEntity CreateParticipant(SessionEntity session, PersonEntity person, ParticipantRole role, bool isDirectParticipant)
        {
            // Ignore if a repear assignment
            ParticipantEntity participant = session.Participants.Where(p => p.ParticipantRole == role && p.PersonId == person.Id).FirstOrDefault();
            if (participant != null)
            {
                return participant;
            }

            participant = Db.Participants.CreateResource(CurrentUser);
            participant.Person = person;
            participant.Session = session;
            participant.ParticipantRole = role;
            participant.ParticipantState = ParticipantState.Pending;
            participant.IsDirectParticipant = isDirectParticipant;
            session.Participants.Add(participant);
            return participant;
        }

        public ParticipantGroupEntity CreateParticipantGroup(SessionEntity session, ParticipantRole role, GroupEntity existingGroup)
        {
            // Ignore if a repeat assignment
            ParticipantGroupEntity participantGroup = session.Groups.Where(g => g.ParticipantRole == role && g.GroupId == existingGroup.Id).FirstOrDefault();
            if (participantGroup != null)
            {
                return participantGroup;
            }

            participantGroup = Db.ParticipantGroups.CreateResource(CurrentUser);
            participantGroup.Session = session;
            participantGroup.Group = existingGroup;
            participantGroup.ParticipantRole = role;
            session.Groups.Add(participantGroup);
            return participantGroup;
        }

        private async Task<ParticipantEntity> ParticipantAsync(OrganizationEntity org, SessionEntity session, string personRoute, ParticipantRole role)
        {
            if (!People.ContainsKey(personRoute))
            {
                PersonEntity dbPerson = await Db.PersonWhereRoute(org.Route, personRoute, false, true);
                dbPerson.AssertFound($"Could not find person ${personRoute} in org ${org.Route}");
                People.Add(dbPerson.Route, dbPerson);
            }

            PersonEntity person = People[personRoute];
            ParticipantEntity participant = CreateParticipant(session, person, role, true);
            return participant;
        }

        private async Task ParticipantGroupAsync(OrganizationEntity org, SessionEntity session, string grpRoute, ParticipantRole role)
        {
            if (!Groups.ContainsKey(grpRoute))
            {
                GroupEntity group = await Db.GroupWhereRoute(org.Route, grpRoute);
                string assertMsg = $"Could not find group ${grpRoute} in org ${org.Route}";
                group.AssertFound(assertMsg);

                // If we have a current user, then we must only allow them to make sessions for groups where they have access
                if (CurrentUser != null)
                {
                    UserEntity memberOrAdminForGrp = await Db.UserWhereUid(org.Route, grpRoute, CurrentUser.UniversalId, MemberRole.Member);
                    memberOrAdminForGrp.AssertFound(assertMsg);
                }

                Groups.Add(grpRoute, group);
            }

            GroupEntity existingGroup = Groups[grpRoute];
            CreateParticipantGroup(session, role, existingGroup);
        }

        private async Task AssertRequiredOrgRole(string orgRoute, SessionSecurityType securityType)
        {
            // Must be able to create at the org level
            await AssertCurrentUserInRole(orgRoute, RequiredOrgCreateRole);

            // If it's public, then they must be able to create public at the org level
            if (securityType == SessionSecurityType.Public)
            {
                await AssertCurrentUserInRole(orgRoute, RequiredOrgCreatePublicRole);
            }
        }

        private async Task AssertCurrentUserInRole(string orgRoute, PersonRole requiredRole)
        {
            if (CurrentUser == null)
            {
                return;
            }

            UserEntity user = await Db.UserWhereUid(orgRoute, CurrentUser.UniversalId, requiredRole);
            user.AssertFound();
        }

        private async Task AssertRequiredGroupRole(string orgRoute, string groupRoute, SessionSecurityType securityType)
        {
            if (CurrentUser == null)
            {
                return;
            }

            // User must have fulfill minimum role requirements for the groups
            await AssertCurrentUserInRole(orgRoute, groupRoute, RequiredGroupCreateRole);

            if (securityType == SessionSecurityType.Public)
            {
                await AssertCurrentUserInRole(orgRoute, groupRoute, RequireGroupCreatePublicRole);
            }
        }

        private async Task AssertCurrentUserInRole(string orgRoute, string groupRoute, MemberRole requiredRole)
        {
            if (CurrentUser == null)
            {
                return;
            }

            UserEntity user = await Db.UserWhereUid(orgRoute, groupRoute, CurrentUser.UniversalId, requiredRole);
            user.AssertFound();
        }

        protected async Task<bool> CreateParticipants(OrganizationEntity org, SessionEntity session, NewSession newSession)
        {
            bool hasHost = false;

            if (newSession.Participants == null || newSession.Participants.Count() == 0)
            {
                return hasHost;
            }

            await AssertRequiredOrgRole(org.Route, newSession.SessionSecurity);

            foreach (NewParticipant newParticipant in newSession.Participants)
            {
                ParticipantEntity participant = await ParticipantAsync(org, session, newParticipant.PersonRoute, newParticipant.ParticipantRole);
                if (participant.ParticipantRole >= ParticipantRole.Host)
                {
                    hasHost = true;
                }
            }

            return hasHost;
        }

        protected async Task<bool> CreateParticipants(OrganizationEntity org, SessionEntity session, SessionEntity existingSession)
        {
            bool hasHost = false;

            if (existingSession.Participants == null || existingSession.Participants.Count == 0)
            {
                return hasHost;
            }

            await AssertRequiredOrgRole(org.Route, existingSession.SessionSecurity);

            IEnumerable<ParticipantEntity> parts = existingSession.Participants.Where(p => p.DeletedUtc == null && p.Person.DeletedUtc == null && p.Person.User.DeletedUtc == null);
            foreach (ParticipantEntity part in parts)
            {
                ParticipantEntity participant = await ParticipantAsync(org, session, part.Person.Route, part.ParticipantRole);
                if (participant.ParticipantRole >= ParticipantRole.Host)
                {
                    hasHost = true;
                }
            }

            return hasHost;
        }

        protected async Task<bool> CreateGroups(OrganizationEntity org, SessionEntity session, SessionEntity existingSession)
        {
            bool hasHost = false;

            if (existingSession.Groups == null || existingSession.Groups.Count == 0)
            {
                return hasHost;
            }

            foreach (ParticipantGroupEntity grp in existingSession.Groups.Where(g => g.DeletedUtc == null && g.Group.DeletedUtc == null))
            {
                await AssertRequiredGroupRole(org.Route, grp.Group.Route, existingSession.SessionSecurity);

                await ParticipantGroupAsync(org, session, grp.Group.Route, grp.ParticipantRole);
                if (grp.ParticipantRole >= ParticipantRole.Host)
                {
                    hasHost = true;
                }
            }

            return hasHost;
        }

        protected async Task<bool> CreateGroups(OrganizationEntity org, SessionEntity session, NewSession newSession)
        {
            bool hasHost = false;

            if (newSession.Groups == null)
            {
                return hasHost;
            }

            foreach (NewParticipantGroup newGrp in newSession.Groups)
            {
                await AssertRequiredGroupRole(org.Route, newGrp.GroupRoute, session.SessionSecurity);

                await ParticipantGroupAsync(org, session, newGrp.GroupRoute, newGrp.ParticipantRole);
                if (newGrp.ParticipantRole >= ParticipantRole.Host)
                {
                    hasHost = true;
                }
            }

            return hasHost;
        }

        /// <summary>
        /// Creates a new Session instance, loaded with prompt, participant, and participant group records per the given INewSession
        /// WARNING: This will throw an exception if the INewSession lacks a host participant or group
        /// </summary>
        /// <param name="org"></param>
        /// <param name="newSession"></param>
        /// <returns></returns>
        public async Task<SessionEntity> BuildAsync(OrganizationEntity org, NewSession newSession)
        {
            SessionEntity session = CreateSession(org, newSession);

            CreatePrompt(session, newSession.FirstPrompt);

            bool hasGroupHost = await CreateGroups(org, session, newSession);
            bool hasParticipantHost = await CreateParticipants(org, session, newSession);

            // Validate
            if (!hasGroupHost && !hasParticipantHost)
            {
                throw new ArgumentException("Must have at least one host participant or group");
            }

            return session;
        }

        public async Task<SessionEntity> BuildAsync(IMapper mapper, OrganizationEntity org, SessionEntity existingSessionEntity)
        {
            // Create the session
            Session existingSession = mapper.MapSafe<Session>(existingSessionEntity);
            SessionEntity session = CreateSession(org, existingSession);

            // Clone prompts
            existingSessionEntity.Prompts.Where(p => p.DeletedUtc == null).ForEach(p => CreatePrompt(session, p.Text));

            bool hasGroupHost = await CreateGroups(org, session, existingSessionEntity);
            bool hasParticipantHost = await CreateParticipants(org, session, existingSessionEntity);

            // Validate
            if (!hasGroupHost && !hasParticipantHost)
            {
                throw new ArgumentException("Must have at least one host participant or group");
            }

            return session;
        }
    }
}
