using System;
using System.Collections.Generic;
using System.Linq;

namespace Soundbite.Services
{
    #region Fluent UI Query

    public class ParticipantQueryCriteriaBuilder
    {
        private readonly ParticipantQueryCriteria _target;
        public ParticipantQueryCriteriaBuilder(ParticipantQueryCriteria target)
        {
            _target = target;
        }
        public ParticipantQueryCriteriaBuilder UserId(int userId)
        {
            _target.UserId = userId;
            return this;
        }

        public ParticipantQueryCriteriaBuilder UserRoute(string userRoute)
        {
            _target.UserRoute = userRoute;
            return this;
        }

        public ParticipantQueryCriteriaBuilder UserUid(string userUniversalId)
        {
            _target.UserUniversalId = userUniversalId;
            return this;
        }

        public ParticipantQueryCriteriaBuilder Roles(params ParticipantRole[] role)
        {
            role.ToList().ForEach(r => _target.Roles.Add(r));
            return this;
        }

        public ParticipantQueryCriteriaBuilder States(params ParticipantState[] states)
        {
            states.ToList().ForEach(s => _target.States.Add(s));
            return this;
        }
    }

    #endregion

    public class ParticipantQuery
    {
        public static ParticipantQuery Single(Action<ParticipantQueryCriteriaBuilder> config)
        {
            ParticipantQuery result = MultipleOr(config);
            return result;
        }

        public static ParticipantQuery MultipleAnd(params Action<ParticipantQueryCriteriaBuilder>[] config)
        {
            ParticipantQuery result = MultipleOr(config);
            result.IsOrQuery = false;
            return result;
        }

        public static ParticipantQuery MultipleOr(params Action<ParticipantQueryCriteriaBuilder>[] config)
        {
            ParticipantQuery result = new ParticipantQuery() { IsOrQuery = true };
            config?.Where(i => i != null).ToList().ForEach(i =>
                {
                    ParticipantQueryCriteria criteria = new ParticipantQueryCriteria();
                    result.Criteria.Add(criteria);
                    ParticipantQueryCriteriaBuilder builder = new ParticipantQueryCriteriaBuilder(criteria);
                    i(builder);
                });
            return result;
        }

        public bool IsOrQuery { get; set; } = true;

        public IList<ParticipantQueryCriteria> Criteria { get; set; } = new List<ParticipantQueryCriteria>();
    }

    public class ParticipantQueryCriteria
    {
        public int? UserId { get; set; }
        public string UserRoute { get; set; }
        public string UserUniversalId { get; set; }
        public IList<ParticipantRole> Roles { get; } = new List<ParticipantRole>();
        public IList<ParticipantState> States { get; } = new List<ParticipantState>();
    }
}
