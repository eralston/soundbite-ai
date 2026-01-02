	-- Delete org people
delete from People 
where Id in (select p.Id
from 
	People p inner join
	Organizations o on p.OrganizationId = o.Id inner join
	Users u on p.UserId = u.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs');

-- Delete Org
delete from Organizations where route = 'QbCYpyCMsEPlAwQ7R2uKs'

-- Delete Clips in Org
delete from Clips 
where Id in (select c.Id
from 
	Clips c inner join
	Prompts p on c.PromptId = p.Id inner join
	Sessions s on p.SessionId = s.Id inner join
	Organizations o on s.OrganizationId = o.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs')

-- Delete Prompts in Org
delete from Prompts
	where Id in (select p.Id
from 
	Prompts p inner join
	Sessions s on p.SessionId = s.Id inner join
	Organizations o on s.OrganizationId = o.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs')

-- Delete Sessions in Org
delete from Sessions
	where Id in (select s.Id
from 
	Sessions s inner join
	Organizations o on s.OrganizationId = o.Id
Where
	o.Route = 'QbCYpyCMsEPlAwQ7R2uKs')

-- Delete users with e-mail domain
delete from Users where Id in
(select u.Id
from Users u
Where email like '%@my1ud5u6yspggfq4vl4te.onmicrosoft.com')