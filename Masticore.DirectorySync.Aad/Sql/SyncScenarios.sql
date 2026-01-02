-- No settings
UPDATE organizations
SET    SyncType = null,
       SyncConfigJson = null
WHERE  id = 1;

-- All Users; All Groups
-- Adding DirectorySyncConfig JSON to an Organization
-- Sync all users and groups
-- This can also be used to reset by clearing the DeltaLink values
UPDATE organizations
SET    SyncType = 'AAD',
       SyncConfigJson = '{   
            "tenantId": "[AMS TENANT ID]",   
            "importAllGroups": true,   
            "importAllUsers": true,   
        }'
WHERE  id = 1;

-- Whitelist Groups; Group Members Only
-- Sync Customer Success and Engineering and their members in the Soundbite tenant
-- This should NOT make records for non-members (EG, test accounts should NOT appear if they were not already there).
-- NOTE: Group-based sync does NOT rely upon the delta mechanism, so this should NEVER be overwritten by the system
UPDATE organizations
SET    SyncType = 'AAD',
       SyncConfigJson = '{   
           "tenantId": "[AMS TENANT ID]",    
           "importAllGroups": false,   
           "importAllUsers": false,   
           "groups": [        
            { 
                "groupId" : "e2d83e41-a6fb-4ede-a086-627d65fa2b9c", 
                "memberSyncType" : "SyncAllMembers" 
            },        
            { 
                "groupId" : "221a90a6-373b-4536-ae08-bb37635cea78", 
                "memberSyncType" : "SyncAllMembers" 
            }   
           ]
       }'
WHERE  id = 1; -- Target org ID in Soundbite database

-- No Groups; Whitelist Users
-- Erik and Test User
UPDATE organizations
SET    SyncType = 'AAD',
       SyncConfigJson = '{   
           "tenantId": "[AMS TENANT ID]",    
           "importAllGroups": false,   
           "importAllUsers": false,   
           "users": [
				{ "Id":"b2be56fb-0b86-44c6-b0ab-8a00e90c61be"},
				{ "Id":"b7013678-031a-43af-b392-d063359caa3d"}
		   ]
       }'
WHERE  id = 1; -- Target org ID in Soundbite database

-- Get Orgs to Sync
SELECT [name],
       [route],
       [id],
       [SyncConfigJson],
       [SyncType]
FROM   [organizations]
WHERE  [SyncConfigJson] IS NOT NULL
       AND [SyncType] IS NOT NULL;