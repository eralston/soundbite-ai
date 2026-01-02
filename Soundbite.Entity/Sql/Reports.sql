-- Manual reporting for a particular Soundbite in a single org

-- Step 1: Find the Org and Session you want to analyze

-- Organizations
select Id, Route, Name, Description, SyncType
from Organizations;

-- Sessions with participant count
select s.Id, s.Name as 'Session Name', s.Publish as 'Session Publish', count(*)
from Sessions s inner join Participants p on s.Id = p.SessionId
where s.OrganizationId = 7
group by s.Id, s.Name, s.Publish;

-- Step 2: Extract data relevant to the session

-- Listens
select u.GivenName as 'First Name', u.FamilyName as 'Last Name', u.Email as 'E-Mail', ce.CreatedUtc as 'Listen Datetime (UTC)'
from Sessions s inner join
Prompts p on s.Id = p.SessionId inner join
Clips c on p.Id = c.PromptId inner join
ClipEvents ce on c.Id = ce.TargetId inner join
Users u on ce.CreatedById = u.Id
where s.Id = 114 and ce.ClipEventType = 400
order by ce.CreatedUtc desc;

-- Listens by Day (UTC)
select cast(ce.CreatedUtc as Date) as 'Date (UTC)', count(*) as 'Listen Count'
from Sessions s inner join
Prompts p on s.Id = p.SessionId inner join
Clips c on p.Id = c.PromptId inner join
ClipEvents ce on c.Id = ce.TargetId inner join
Users u on ce.CreatedById = u.Id
where s.Id = 114 and ce.ClipEventType = 400
group by cast(ce.CreatedUtc as Date)
order by cast(ce.CreatedUtc as Date)

-- Events by Month
select format(ce.CreatedUtc,'yyyy-MM') as 'Month', count(*) as 'Events'
from Sessions s inner join
Prompts p on s.Id = p.SessionId inner join
Clips c on p.Id = c.PromptId inner join
ClipEvents ce on c.Id = ce.TargetId inner join
Users u on ce.CreatedById = u.Id
group by  format(ce.CreatedUtc,'yyyy-MM')
order by  format(ce.CreatedUtc,'yyyy-MM')

-- Acknowledgements
select u.GivenName, u.FamilyName, u.email
from Sessions s
inner join Participants p on s.Id = p.SessionId
inner join People pe on pe.Id = p.PersonId
inner join Users u on u.Id = pe.UserId
inner join ClipEvents ce on ce.CreatedById = u.Id
where s.Id = 130 and p.ParticipantState = 5;