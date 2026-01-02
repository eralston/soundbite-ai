using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Soundbite.Entity.Tests
{
    /// <summary>
    /// Default seed for an SbDb
    /// </summary>
    public class SbDbSeed : IdentityDbSeed
    {
        public const int DefaultDisplaySeconds = 30;
        public const int DefaultBillingSeconds = 60;

        public static string SessionRoute;
        public static string NonMemberSessionRoute;
        public static string SeriesRoute;
        public static string PromptRoute;
        public static string LastClipRoute;




        // Collaboration

        private int ClipId = 1;
        private int PromptId = 1;
        private int SessionId = 1;
        private int ParticipantGroupId = 1;
        private int ParticipantId = 1;
        private int SeriesId = 1;

        public override void Seed(ModelBuilder builder)
        {
            base.Seed(builder);
            SeedWesteros();

            TenantOutside = null; // TODO: make an outside tenant for the GOT seed
        }

        protected void SeedWesteros()
        {
            Tenant = SeedTenant("Westeros");

            // Organization for Starks
            SeedHouseStark(Tenant);
            SeedHouseLannister(Tenant);
        }

        protected void SeedHouseLannister(TenantEntity tenant)
        {
            // Invisible organization for Lannisters
            OrganizationEntity lannisters = SeedOrganization(tenant, "House Lannister", "Hear Me Roar");
            (_, PersonEntity cersei) = SeedUserAndPerson(lannisters, "Cersei", "Lannister", "Cersei@Lannister");
            SeedGroup(lannisters, "Raging Psychopaths", cersei);
            CreateSession(lannisters, "Lannister", 60, SessionType.Meeting, DateTime.UtcNow, DateTime.UtcNow);
            CreateSession(lannisters, "Lannister Deleted", 60, SessionType.Meeting, DateTime.UtcNow, DateTime.UtcNow, true);
        }

        protected void SeedHouseStark(TenantEntity tenant)
        {
            // Identity
            OrganizationEntity starks = SeedOrganization(tenant, "House Stark", "Winter is Coming");
            OrgPrimary = starks;
            PersonOrgAdmin = SeedPerson(starks, UserOrgAdmin, PersonRole.Admin);

            (UserEntity nedUser, PersonEntity ned) = SeedUserAndPerson(starks, "Ned", "Stark", "Ned@Stark");
            (UserEntity catelynUser, PersonEntity catelyn) = SeedUserAndPerson(starks, "Catelyn", "Stark", "Catelyn@Stark");
            (UserEntity sansaUser, PersonEntity sansa) = SeedUserAndPerson(starks, "Sansa", "Stark", "Sansa@Stark");

            UserRegular = sansaUser;
            UserNonOrgAdminGroupOwner = catelynUser;

            (UserOrgMemberNoGroupMembership, PersonNoGroupMembership) = SeedUserAndPerson(starks, "Robb", "Stark", "Robb@Stark");
            (_, PersonEntity arya) = SeedUserAndPerson(starks, "Arya", "Stark", "Arya@Stark");
            (_, PersonEntity bran) = SeedUserAndPerson(starks, "Bran", "Stark", "Bran@Stark");
            (_, PersonEntity jon) = SeedUserAndPerson(starks, "Jon", "Snow", "Jon@Snow");
            (_, PersonEntity rickon) = SeedUserAndPerson(starks, "Rickon", "Stark", "Rickon@Stark.");
            (_, PersonEntity benjen) = SeedUserAndPerson(starks, "Benjen", "Stark", "Benjen@Stark");
            (UserEntity hodorUser, PersonEntity hodor) = SeedUserAndPerson(starks, "Hod", "or", "Hodor@Stark");

            GroupEntity mrAndMrs = SeedGroup(starks, "Mr. & Mrs.", PersonOrgAdmin, ned, catelyn);
            GroupEntity leaders = SeedGroup(starks, "Leaders", PersonOrgAdmin, ned, catelyn, benjen, sansa);
            GroupOrgAdminOwned = leaders;
            GroupEntity undead = SeedGroup(starks, "Undead", catelyn, benjen);
            GroupMemberOwned = undead;
            SeedGroup(starks, "Heirs", PersonOrgAdmin, sansa, PersonNoGroupMembership, arya, bran, jon, rickon);

            // Session w/o Current User
            int limit = 180;
            SessionType type = SessionType.Announcement;
            DateTime? reminder = Time.UtcNow;
            DateTime? deadline = Time.UtcNow;
            CreateNonMemberSession(starks, nedUser, undead, limit, type, reminder, deadline);

            // Collaboration

            CreateFullSessionAsync("Building The Wall Long Ago", starks, PersonOrgAdmin, nedUser, ned, catelynUser, catelyn, sansaUser, sansa, hodorUser, hodor, mrAndMrs, leaders, true);
            CreateFullSessionAsync("Throne Room Meeting", starks, PersonOrgAdmin, nedUser, ned, catelynUser, catelyn, sansaUser, sansa, hodorUser, hodor, mrAndMrs, leaders, false);


            CreateSession(starks, "This session should be deleted", limit, type, reminder, deadline, true);
            CreateSession(starks, "This session should also be deleted", 60, SessionType.Announcement, DateTime.UtcNow, DateTime.UtcNow, true);
        }

        private void CreateNonMemberSession(OrganizationEntity starks, UserEntity nedUser, GroupEntity undead, int limit, SessionType type, DateTime? reminder, DateTime? deadline)
        {
            SessionEntity session = CreateSession(starks, "Walk in the Godswood", limit, type, reminder, deadline);
            NonMemberSessionRoute = session.Route;
            CreateParticipantGroup(undead, session);
            PromptEntity prompt = CreatePrompt(session, "All Men Must Die");
            // Clips
            IEnumerable<(int, ClipType, FileType)> clips = new List<(int, ClipType, FileType)>
            {
                (nedUser.Id, ClipType.Prompt, FileType.Mp3)
            };
            CreateClips(prompt, clips);
        }

        private void CreateFullSessionAsync(string name, OrganizationEntity starks, PersonEntity currentPerson, UserEntity nedUser, PersonEntity ned, UserEntity catelynUser, PersonEntity catelyn, UserEntity sansaUser, PersonEntity sansa, UserEntity hodorUser, PersonEntity hodor, GroupEntity mrAndMrs, GroupEntity leaders, bool isPublished)
        {
            // Full Session
            int limit = 180;
            SessionType type = SessionType.Announcement;
            DateTime? reminder = Time.UtcNow;
            DateTime? deadline = Time.UtcNow;
            SessionEntity session = CreateSession(starks, name, limit, type, reminder, deadline, false, isPublished);
            SessionRoute = session.Route;

            // Participant Group
            CreateParticipantGroup(mrAndMrs, session);
            CreateParticipantGroup(leaders, session, ParticipantRole.Host);

            // Participants
            IEnumerable<(int, ParticipantRole)> participants = new List<(int, ParticipantRole)>
            {
                (currentPerson.Id, ParticipantRole.Host),
                (ned.Id, ParticipantRole.Host),
                (catelyn.Id, ParticipantRole.Host),
                (sansa.Id, ParticipantRole.Participant),
                (hodor.Id, ParticipantRole.Audience),
            };
            CreateParticipants(session, participants);

            // Prompt
            string firstPrompt = "The only time a man can be brave is when he is afraid. Any questions?";
            PromptEntity prompt = CreatePrompt(session, firstPrompt);
            PromptRoute = prompt.Route;

            // Clips
            IEnumerable<(int, ClipType, FileType)> clips = new List<(int, ClipType, FileType)>
            {
                (nedUser.Id, ClipType.Prompt, FileType.Mp3),
                (catelynUser.Id, ClipType.Prompt, FileType.Mp3),
                (sansaUser.Id, ClipType.Contribution, FileType.Mp3),
                (hodorUser.Id, ClipType.Comment, FileType.Mp3),
            };
            CreateClips(prompt, clips);

            // Series
            Recurrence recurrence = Recurrence.Daily;
            string recurrenceData = "TBD";
            session.Name = "Throne Room Series";
            (SeriesEntity series, SessionEntity templateSession) = CreateSeries(starks, session, recurrence, recurrenceData);
            SeriesRoute = series.Route;
            CreateParticipants(templateSession, participants);
            CreateParticipantGroup(mrAndMrs, templateSession);
            CreateParticipantGroup(leaders, templateSession, ParticipantRole.Host);
        }

        private ParticipantGroupEntity CreateParticipantGroup(GroupEntity grp, SessionEntity session, ParticipantRole role = ParticipantRole.Participant)
        {
            ParticipantGroupEntity group = ResourceEntityBase.CreateMock<ParticipantGroupEntity>(ParticipantGroupId++);
            group.Id = ParticipantGroupId++;
            group.ParticipantRole = role;
            group.SessionId = session.Id;
            group.GroupId = grp.Id;
            Builder.Entity<ParticipantGroupEntity>().HasData(group);
            return group;
        }

        // Collaboration

        protected (SeriesEntity, SessionEntity) CreateSeries(OrganizationEntity org, SessionEntity session, Recurrence recurrence, string recurrenceData)
        {
            // Create Template session
            SessionEntity templateSession = ResourceEntityBase.CreateMock<SessionEntity>(SessionId++);
            templateSession.Name = session.Name;
            templateSession.SessionType = session.SessionType;
            templateSession.SessionSecurity = SessionSecurityType.Protected;
            templateSession.Reminder = session.Reminder;
            templateSession.Publish = session.Publish;
            templateSession.OrganizationId = org.Id;
            templateSession.IsTemplate = true;
            Builder.Entity<SessionEntity>().HasData(templateSession);

            // Create series
            SeriesEntity series = ResourceEntityBase.CreateMock<SeriesEntity>(SeriesId++);
            series.Name = session.Name;
            series.Recurrence = recurrence;
            series.RecurrenceData = recurrenceData;
            series.TemplateId = templateSession.Id;
            series.OrganizationId = org.Id;
            Builder.Entity<SeriesEntity>().HasData(series);

            return (series, templateSession);
        }

        protected SessionEntity CreateSession(OrganizationEntity org, string name, int limit, SessionType type, DateTime? reminder, DateTime? deadline, bool deleted = false, bool published = false)
        {
            // Create Session
            SessionEntity session = ResourceEntityBase.CreateMock<SessionEntity>(SessionId++);
            session.Name = name;
            session.Limit = limit;
            session.SessionType = type;
            session.SessionSecurity = SessionSecurityType.Protected;
            session.Reminder = reminder ?? DateTime.UtcNow;
            session.Publish = deadline ?? DateTime.UtcNow;
            if (published)
            {
                session.PublishSent = DateTime.UtcNow;
            }
            if (deleted)
            {
                session.SoftDeleteNow();
            }
            session.OrganizationId = org.Id;
            Builder.Entity<SessionEntity>().HasData(session);
            return session;
        }

        /// <summary>
        /// Creates participants in the given session
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="session"></param>
        /// <param name="participants">(Person.Id, role)</param>
        protected void CreateParticipants(SessionEntity session, IEnumerable<(int, ParticipantRole)> participants)
        {
            // Add Participants
            foreach ((int personId, ParticipantRole role) in participants)
            {
                ParticipantEntity participant = ResourceEntityBase.CreateMock<ParticipantEntity>(ParticipantId++);
                participant.PersonId = personId;
                participant.ParticipantRole = role;
                participant.ParticipantState = ParticipantState.ConsumptionRequested;
                participant.SessionId = session.Id;
                Builder.Entity<ParticipantEntity>().HasData(participant);
            }
        }

        protected PromptEntity CreatePrompt(SessionEntity session, string firstPrompt)
        {
            // Add Prompts
            PromptEntity prompt = ResourceEntityBase.CreateMock<PromptEntity>(PromptId++);
            prompt.Text = firstPrompt;
            prompt.SessionId = session.Id;
            Builder.Entity<PromptEntity>().HasData(prompt);
            return prompt;
        }

        /// <summary>
        /// Creates clips in the given prompt
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="prompt"></param>
        /// <param name="clips"></param>
        protected void CreateClips(PromptEntity prompt, IEnumerable<(int, ClipType, FileType)> clips)
        {
            // Add Clips
            foreach ((int userId, ClipType clipType, FileType fileType) in clips)
            {
                ClipEntity clip = ResourceEntityBase.CreateMock<ClipEntity>(ClipId++);
                clip.CreatedById = userId;
                clip.ClipType = clipType;
                clip.FileType = fileType;
                clip.PromptId = prompt.Id;
                clip.DisplaySeconds = DefaultDisplaySeconds;
                clip.BillingSeconds = DefaultBillingSeconds;
                Builder.Entity<ClipEntity>().HasData(clip);

                LastClipRoute = clip.Route;
            }
        }
    }
}
