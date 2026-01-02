using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RandomNameGeneratorLibrary;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.MockData
{
    /// <summary>
    /// Azure Functions to loading mock data into the database en masse
    /// </summary>
    public class Functions
    {
        #region Fields

        // Enable or disable these flags to scope the functions that will run

        protected const bool RunCreateUsers = false;
        protected const int NumberOfUsersToCreate = 10000;
        protected const string EmailTld = "mockdata.com";

        protected const string OrgRoute = "SQ4yhWkP";

        protected const bool RunCreatePeople = false;
        protected const int NumberOfPeopleToCreate = 1030;

        protected const bool RunCreateGroups = false;
        protected const int NumberOfGroupsToCreate = 25;
        protected const int NumberOfMembersInGroups = 500;
        protected const int OwnerPersonId = 1;

        protected const bool RunCreateSessions = true;
        protected const bool AreSessionsPending = true;
        protected const bool AreSessionsForGroup = false;
        protected const int NumberOfSessionsToCreate = 128;
        protected const int NumberOfParticipantsPerSession = 128;
        protected const int HostPersonId = 1;

        // The timer only runs once a year. This is just the quickest template to enable RunOnStartup
        private const string EveryFirstOfJanuary = "0 0 0 1 1 *";

        protected ISbInfrastructure Infrastructure { get; }

        protected PersonNameGenerator Names { get; } = new PersonNameGenerator();
        protected Random Rand { get; } = new Random();

        #endregion

        #region Constructor

        public Functions(
            ISbInfrastructure infrastructure
            )
        {
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
        }

        #endregion

        #region Support Methods

        private PersonEntity Random(PersonEntity[] people)
        {
            if (people is null)
            {
                throw new ArgumentNullException(nameof(people));
            }

            int index = Rand.Next(0, people.Length);
            return people[index];
        }

        private PersonEntity[] Random(PersonEntity[] people, int desiredCount)
        {
            if (people is null)
            {
                throw new ArgumentNullException(nameof(people));
            }

            Dictionary<string, PersonEntity> ret = new Dictionary<string, PersonEntity>();
            while (ret.Count < desiredCount)
            {
                PersonEntity person = Random(people);
                ret[person.Route] = person;
            }

            return ret.Values.ToArray();
        }

        private static PersonEntity[] MockPopulation(SbDb db)
        {
            return db.People.Where(p => p.Organization.Route == OrgRoute && p.User.Email.EndsWith(EmailTld)).ToArray();
        }

        #endregion

        #region Create Methods

        private UserEntity CreateUser(SbDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            UserEntity userEntity = db.Users.CreateResource();
            userEntity.GivenName = Names.GenerateRandomFirstName();
            userEntity.FamilyName = Names.GenerateRandomLastName();
            userEntity.UserRole = UserRole.User;
            userEntity.AllowMarketing = true;
            userEntity.AllowNews = true;
            userEntity.AllowEmail = true;
            userEntity.AllowSms = true;
            userEntity.Email = $"{userEntity.GivenName}.{userEntity.FamilyName}.{DateTime.UtcNow.Ticks}@{EmailTld}";
            userEntity.NewUniversalId(); // Can't login because this is random
            userEntity.Accept(); // Does NOT send invite, just marks times
            return userEntity;
        }

        private PersonEntity CreatePerson(SbDb db, OrganizationEntity org)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            UserEntity user = CreateUser(db);
            return CreatePerson(db, org, user);
        }

        private static PersonEntity CreatePerson(SbDb db, OrganizationEntity org, UserEntity user)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            PersonEntity person = db.People.CreateResource();
            person.User = user;
            person.Organization = org;
            person.PersonRole = PersonRole.Person;
            person.Accept(); // Does NOT send invite, just marks times
            return person;
        }

        private static void CreateMember(SbDb db, GroupEntity group, PersonEntity person, MemberRole role = MemberRole.Member)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (group is null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (person is null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            MemberEntity member = db.Members.CreateResource();
            member.Person = person;
            member.Group = group;
            member.MemberRole = role;
        }

        private GroupEntity CreateGroup(SbDb db, OrganizationEntity org)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            GroupEntity group = db.Groups.CreateResource();
            group.Organization = org;
            group.Name = $"The {Names.GenerateRandomLastName()} Group";
            return group;
        }

        private SessionEntity CreateSession(SbDb db, OrganizationEntity org)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            SessionEntity session = db.Sessions.CreateResource();
            session.Name = $"The {Names.GenerateRandomFirstName()} Session";
            session.Organization = org;
            session.SessionType = SessionType.Announcement;
            session.Reminder = Time.UtcNow;
            session.ReminderSent = Time.UtcNow;
            session.Publish = Time.UtcNow;
            if (!AreSessionsPending)
            {
#pragma warning disable CS0162 // Unreachable code detected
                session.PublishSent = Time.UtcNow;
#pragma warning restore CS0162 // Unreachable code detected
            }
            //session.PublishSent = Time.UtcNow;
            return session;
        }

        private ParticipantEntity CreateParticipant(
            SbDb db,
            SessionEntity session,
            PersonEntity person,
            ParticipantRole role = ParticipantRole.Audience)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (person is null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            ParticipantEntity newParticipant = db.Participants.CreateResource();
            newParticipant.Person = person;
            newParticipant.Session = session;
            newParticipant.ParticipantRole = role;
            newParticipant.ParticipantState = ParticipantState.Consumed;
            return newParticipant;
        }

        private static ClipEntity CreateClip(SbDb db, PersonEntity host, PromptEntity prompt)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (prompt is null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            if (host.User == null)
            {
                throw new ArgumentNullException($"{nameof(host)}.{nameof(host.User)} must have a value to relate its user to the clip");
            }

            ClipEntity newClip = db.Clips.CreateResource();
            newClip.CreatedBy = host.User;
            newClip.Prompt = prompt;
            newClip.ClipType = ClipType.Prompt;
            newClip.ClipSource = ClipSource.WidgetRecorderV1;
            newClip.FileType = FileType.Mp3;
            newClip.ClipState = ClipState.Ready;
            newClip.DisplaySeconds = 60;
            newClip.BillingSeconds = 60;
            return newClip;
        }

        private static PromptEntity CreatePrompt(SbDb db, SessionEntity session)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            PromptEntity prompt = db.Prompts.CreateResource();
            prompt.Session = session;
            prompt.Text = "Say Something Worth Hearing";
            return prompt;
        }

        /// <summary>
        /// A session in the format of a one-to-many podcast where <see cref="HostPersonId"/> is the announcer
        /// </summary>
        /// <param name="db"></param>
        /// <param name="org"></param>
        /// <param name="people"></param>
        private void CreateAnnouncements(SbDb db, OrganizationEntity org, PersonEntity[] people)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (people is null)
            {
                throw new ArgumentNullException(nameof(people));
            }

            SessionEntity newSession = CreateSession(db, org);

            PersonEntity host = db.People.Where(p => p.Id == HostPersonId).Include(p => p.User).Single();
            CreateParticipant(db, newSession, host, ParticipantRole.Host);
            PromptEntity prompt = CreatePrompt(db, newSession);

            CreateClip(db, host, prompt);

            PersonEntity[] peopleForThisSession = Random(people, NumberOfParticipantsPerSession);
            foreach (PersonEntity personWhoWantsToParticipate in peopleForThisSession)
            {
                CreateParticipant(db, newSession, personWhoWantsToParticipate);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Creates fresh users who do not have access to any particular org; this would be like people who touch the PLG page, but never actually start a trial
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName(nameof(CreatesUsers))]
        public async Task CreatesUsers([TimerTrigger(EveryFirstOfJanuary, RunOnStartup = RunCreateUsers)] TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation($"{nameof(CreatesUsers)}: {DateTime.Now}");
            SbDb db = await Infrastructure.DbAsync();

            logger.LogInformation($"There are {db.Users.ToArray().Length} users to start, adding {NumberOfUsersToCreate}");

            for (int i = 0; i < NumberOfUsersToCreate; ++i)
            {
                CreateUser(db);
            }

            await db.SaveChangesAsync();

            logger.LogInformation($"Successfully added {NumberOfUsersToCreate} user; There are now {db.Users.ToArray().Length} users");
        }

        /// <summary>
        /// Creates people inside of a target org; these are regular people who do not automatically come with any content on them, but they can receive content
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName(nameof(CreatePeople))]
        public async Task CreatePeople([TimerTrigger(EveryFirstOfJanuary, RunOnStartup = RunCreatePeople)] TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation($"{nameof(CreatePeople)}: {DateTime.Now}");
            SbDb db = await Infrastructure.DbAsync();

            logger.LogInformation($"There are {db.People.ToArray().Length} people to start, adding {NumberOfPeopleToCreate}");

            OrganizationEntity org = await db.OrgWhereRoute(OrgRoute);

            for (int i = 0; i < NumberOfPeopleToCreate; ++i)
            {
                CreatePerson(db, org);
            }

            await db.SaveChangesAsync();

            logger.LogInformation($"Successfully added {NumberOfPeopleToCreate} people; There are now {db.People.ToArray().Length} people");
        }

        [FunctionName(nameof(CreateGroups))]
        public async Task CreateGroups([TimerTrigger(EveryFirstOfJanuary, RunOnStartup = RunCreateGroups)] TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation($"{nameof(CreateGroups)}: {DateTime.Now}");
            SbDb db = await Infrastructure.DbAsync();

            logger.LogInformation($"There are {db.People.ToArray().Length} groups to start, adding {NumberOfGroupsToCreate} with {NumberOfMembersInGroups} people each");

            OrganizationEntity org = await db.OrgWhereRoute(OrgRoute);
            PersonEntity[] people = MockPopulation(db);

            for (int i = 0; i < NumberOfGroupsToCreate; ++i)
            {
                GroupEntity group = CreateGroup(db, org);

                if (OwnerPersonId > 0)
                {
                    CreateMember(db, group, db.People.Find(OwnerPersonId));
                }

                PersonEntity[] peopleForMembers = Random(people, NumberOfMembersInGroups);
                foreach (PersonEntity person in peopleForMembers)
                {
                    CreateMember(db, group, person);
                }
            }

            await db.SaveChangesAsync();

            logger.LogInformation($"Successfully added {NumberOfGroupsToCreate} groups; There are now {db.Groups.ToArray().Length} groups");
        }

        [FunctionName(nameof(CreateSessions))]
        public async Task CreateSessions([TimerTrigger(EveryFirstOfJanuary, RunOnStartup = RunCreateSessions)] TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation($"{nameof(CreateSessions)}: {DateTime.Now}");
            SbDb db = await Infrastructure.DbAsync();

            logger.LogInformation($"There are {db.Sessions.ToArray().Length} sessions to start, adding {NumberOfSessionsToCreate} with {NumberOfParticipantsPerSession} participants each");

            OrganizationEntity org = await db.OrgWhereRoute(OrgRoute);

            PersonEntity[] people = MockPopulation(db);

            for (int i = 0; i < NumberOfSessionsToCreate; ++i)
            {
                CreateAnnouncements(db, org, people);
            }

            await db.SaveChangesAsync();

            logger.LogInformation($"Successfully added {NumberOfSessionsToCreate} sessions; There are now {db.Sessions.ToArray().Length} sessions");
        }

        #endregion
    }
}