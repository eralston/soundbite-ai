-- Sessions
select Id, s.Name as 'Session Name', s.Publish as 'Session Publish', s.SeriesId as 'Series ID', s.DeletedUtc as 'Deleted'
from Sessions s;

-- Sessions joined with Participants
select s.Name as 'Session Name', s.Publish as 'Session Publish', s.SeriesId as 'Series ID', u.Email as 'User Email'
from Sessions s
	inner join Participants p on s.Id = p.SessionId
	inner join People pe on p.PersonId = pe.Id
	inner join Users u on pe.UserId = u.Id;

-- Recent Sessions with Participant Count for Organization by Route

select s.route, s.name, count(*) 'Participant Count'
from Sessions s
	inner join Participants p on s.Id = p.SessionId
	inner join People pe on p.PersonId = pe.Id
	inner join Users u on pe.UserId = u.Id
	inner join [dbo].[Organizations] o on s.organizationid = o.id
where o.route = 'nypbtgcmfw26osieshuuf'
group by s.id, s.route, s.name
order by s.id desc

-- Sessions joined with Clips
select 
    s.Id 'Session ID',
	s.Name 'Session Name', 
	s.Route 'Session Route', 
	s.DeletedUtc 'Session Deleted', 
	p.Text 'Prompt Text', 
	p.Route 'Prompt Route', 
	s.Publish 'Publish', 
	s.PublishSent 'Publish Sent', 
	c.Id 'Clip ID',
	c.Route 'Clip Route',
	c.ClipType 'Clip Type',
	c.MediaProcessingState 'Clip Media State'

from Sessions s
	inner join Prompts p on s.Id = p.SessionId
	inner join Clips c on p.Id = c.PromptId

where s.PublishSent is null
	and s.DeletedUtc is null;

-- Participants in a session
select s.Name, p.ParticipantState, p.ParticipantRole, p.Route as 'Participant Route', u.GivenName, u.FamilyName, u.email, u.route as 'User Route'
from Sessions s
inner join Participants p on s.Id = p.SessionId
inner join People pe on pe.Id = p.PersonId
inner join Users u on u.Id = pe.UserId
where s.route = '9pBwMLQl'; -- Session route

update Participants set ParticipantState = 5 where route = '9r6xcFVy'

-- Clip Duration
select 
	s.Name,  
	c.CreatedUtc,
	c.DisplaySeconds/60 as "Display Minutes",
	c.DisplaySeconds % 60 as "Display Seconds",
	c.BillingSeconds/60 as "Billing Minutes", 
	c.BillingSeconds % 60 as "Billing Seconds"
from 
	Clips c inner join
	Prompts p on c.PromptId = p.Id inner join
	Sessions s on p.SessionId = s.Id
where c.BillingSeconds > 0

-- Total Counts

select count(*) 'Sessions'
from Sessions

select count(*) 'Prompts'
from Prompts

select count(*) 'Clips'
from Clips

-- Check Event Time Disparity
select c.Id, c.Route, c.DisplaySeconds, ce.Id, ce.ClipEventType, ce.EventType, ce.Duration
from 
Clips c inner join
ClipEvents ce on c.Id = ce.TargetId
where DisplaySeconds != Duration

-- Reconcile ClipEvents to match DisplaySeconds
-- This was written when all event types had duration = DisplaySeconds
MERGE INTO ClipEvents
USING Clips
ON ClipEvents.TargetId = Clips.Id
WHEN MATCHED THEN 
    UPDATE SET ClipEvents.Duration = Clips.DisplaySeconds;

-- Notification History
SELECT 
	sn.createdutc 'Sent', 
	CASE sn.status
		WHEN 0 THEN 'Unknown'
		when 10 THEN 'Success'
		when 20 THEN 'Failure'
		ELSE 'Other' 
	END,
	CASE sn.notificationtype 
		WHEN 0 THEN 'Unknown'
		when 10 THEN 'Reminder'
		when 20 THEN 'Audience Publish'
		when 30 THEN 'Host Publish' 
		ELSE 'Other' 
	END 'Type', 
	CASE sn.channel
		WHEN 0 THEN 'Unknown'
		WHEN 10 THEN 'E-Mail'
		WHEN 20 THEN 'SMS'
		WHEN 30 THEN 'Teams' 
	END 'Channel', 
	sn.details 'Notification Details',
	u.givenname 'First Name',
	u.familyname 'Last Name',
	u.email 'Email'
FROM 
	[dbo].[SessionNotifications] sn inner join
	[dbo].[Sessions] s on sn.sessionid = s.id inner join
	[dbo].[Users] u on sn.userid = u.id
WHERE
	s.route = 'oievq6j7gt0wxx9o2h7yn'