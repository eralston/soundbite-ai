
-- Organizations
select Id, Route, Name, Description, SyncType
from Organizations;

-- People
select o.Name as 'Organization Name', u.Email, u.UserRole as 'User Role',  p.PersonRole as 'Person Role', u.UniversalId, u.DeletedUtc as 'User Deleted', p.DeletedUtc as 'Person Deleted'
from Users u
	inner join People p on u.Id = p.UserId
	inner join Organizations o on p.OrganizationId = o.Id
order by 1,2;

-- Removed People
select o.Name as 'Organization Name', u.Email, u.UserRole as 'User Role',  p.PersonRole as 'Person Role', u.UniversalId
from Users u
	inner join People p on u.Id = p.UserId
	inner join Organizations o on p.OrganizationId = o.Id
where p.DeletedUtc is not null
order by 1,2;

-- Groups and members
select o.Name, g.Name as 'Group Name', U.Email, m.MemberRole, p.PersonRole, u.UserRole
from 
	Organizations o
	inner join Groups as g on g.OrganizationId = o.Id 
	inner join Members as m on g.Id = m.GroupId
	inner join People as p on m.PersonId = p.Id
	inner join Users as u on p.UserId = u.Id
where g.DeletedUtc is null
order by 1, 2;

-- Total Counts

select count(*) 'Organizations'
from Organizations

select count(*) 'Users'
from Users

select count(*) 'People'
from People