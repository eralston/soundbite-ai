
-- Users in Org
select o.Name, p.Id, u.Email
from 
	People p inner join
	Organizations o on p.OrganizationId = o.Id inner join
	Users u on p.UserId = u.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs';

	-- Clips in Org
select o.Name, s.Name, c.Id
from 
	Clips c inner join
	Prompts p on c.PromptId = p.Id inner join
	Sessions s on p.SessionId = s.Id inner join
	Organizations o on s.OrganizationId = o.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs';

	-- Prompts in org
select o.Name, s.Name, p.Id
from 
	Prompts p inner join
	Sessions s on p.SessionId = s.Id inner join
	Organizations o on s.OrganizationId = o.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs';

select *
from Participants p inner join
	Sessions s on p.SessionId = s.id inner join
	Organizations o on s.OrganizationId = o.Id
where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs';

	-- Sessions in org
select o.Name, s.Name, s.Id
from 
	Sessions s inner join
	Organizations o on s.OrganizationId = o.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs';

	-- Users in e-mail domain
	select u.Id
from Users u
Where email like '%@my1ud5u6yspggfq4vl4te.onmicrosoft.com'

-- Member records for org
select 
	g.Name 'Group Name',
	CASE m.MemberRole 
		WHEN 0 THEN 'Unknown'
		when 50 THEN 'Member'
		when 100 THEN 'Owner'
		ELSE 'UNRECOGNIZED' 
	END 'Role',
	u.GivenName 'Given Name',
	u.FamilyName 'Family Name',
	u.email 'E-Mail',
	o.Name 'Org Name'
from 
	members m inner join
	groups g on m.GroupId = g.id inner join
	people p on m.PersonId = p.Id inner join
	users u on p.UserId = u.id inner join
	organizations o on p.OrganizationId = o.Id
where 
	o.route = 'SQ4yhWkP'
order by g.Name, u.GivenName