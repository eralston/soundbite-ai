-- Finding an organization to which we will add metadata
SELECT
    *
FROM organizations;

-- Hone in on a single org and its config
SELECT
    NAME,
    SyncConfigJson
FROM organizations
WHERE id = 1;

-- All Users
SELECT
    *
FROM users;

-- Users for an Org
SELECT
    o.NAME,
    u.givenname,
    u.familyname,
    u.email,
    u.phone
FROM organizations o
INNER JOIN people p
    ON o.id = p.organizationid
INNER JOIN users u
    ON p.userid = u.id
WHERE o.id = 1;