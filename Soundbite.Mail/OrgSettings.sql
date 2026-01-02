-- SQL Scripts for applying variation OrgSettings JSON to Organizations

-- Clear settings
UPDATE organizations
SET    ConfigJson = null
WHERE  id = 1;


-- Enable ALL notifications
-- This is the default behavior, so you should never reall have to do this
UPDATE organizations
SET    ConfigJson = '{   
            "notifications": {
                "isWelcomeEnabled":true,
                "isPersonInviteEnabled":true,
                "isMemberInvitedEnabled":true,
                "isSessionReminderEnabled":true,
                "isSessionPublishEnabled":true,
                "isSessionHostPublishEnabled":true
            }
        }'
WHERE  id = 1;

-- Disable Everything, except host notifications with handy links in them
UPDATE organizations
SET    ConfigJson = '{   
            "notifications": {
                "isWelcomeEnabled":false,
                "isPersonInviteEnabled":false,
                "isMemberInvitedEnabled":false,
                "isSessionReminderEnabled":false,
                "isSessionPublishEnabled":false,
                "isSessionHostPublishEnabled":true
            }
        }'
WHERE  id = 1;

-- Disable ALL notifications
UPDATE organizations
SET    ConfigJson = '{   
            "notifications": {
                "isWelcomeEnabled":false,
                "isPersonInviteEnabled":false,
                "isMemberInvitedEnabled":false,
                "isSessionReminderEnabled":false,
                "isSessionPublishEnabled":false,
                "isSessionHostPublishEnabled":false
            }
        }'
WHERE  id = 1;

-- Corporate Comms Mode: Only Admins Can Do Most Things; Almost No Notifications
-- Disable ALL notifications
UPDATE organizations
SET    ConfigJson = '{   
            "notifications": {
                "isWelcomeEnabled":false,
                "isPersonInviteEnabled":false,
                "isMemberInvitedEnabled":false,
                "isSessionReminderEnabled":false,
                "isSessionPublishEnabled":false,
                "isSessionHostPublishEnabled":true
            },
            "permissions": {
                "minRoleToCreateTeam": 100,
                "minRoleToCreateSession": 100,
                "minRoleToCreateTeamSession": 100,
                "minRoleForInvite": 100,
                "minRoleForOrgInvite": 100,
                "minRoleForTeamInvite": 100,
                "minRoleForAudience": 100,
            }
        }'
WHERE  id = 1;
