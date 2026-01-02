# Soundbite.Client Project
---
This is a lerna monorepo with the Soundbite SDK libraries, but supporting layers of libraries that build up the capability to make React-powered single page apps (SPA), MS Teams apps, and more.

## Getting Started
---
1. Clone the repo
2. Setup .nprmrc and authenticate: https://learn.microsoft.com/en-us/azure/devops/artifacts/npm/npmrc?view=azure-devops&tabs=windows%2Cclassic
2. Run `npm install`
3. Run `npm run bootstrap`
4. Run `npm run build`

You may need to setup a local npmrc file

## Fixing Failed Authentication
---
The custom npm feed can lost its authentication over time. If you try to run npm i, lerna run bootstrap, or lerna add and they get an authentication error that looks anything like this:

```
info cli using local version of lerna
lerna notice cli v5.6.2
lerna info versioning independent
lerna ERR! HttpErrorAuthUnknown: Unable to authenticate, need: Basic realm="https://pkgsprodcus1.pkgs.visualstudio.com/"
lerna ERR!     at C:\Users\erikr\source\repos\Soundbite\Soundbite.Client\node_modules\npm-registry-fetch\lib\check-response.js:80:17
lerna ERR!     at processTicksAndRejections (internal/process/task_queues.js:95:5)
lerna ERR!     at async RegistryFetcher.packument (C:\Users\erikr\source\repos\Soundbite\Soundbite.Client\node_modules\pacote\lib\registry.js:92:19)
lerna ERR! lerna Unable to authenticate, need: Basic realm="https://pkgsprodcus1.pkgs.visualstudio.com/"
```

Then you can likely reauthenticate with the following command:

```
vsts-npm-auth -config .npmrc -F
```

If your custom .npmrc credential with ADO has expired, then you need to go refresh that by following the instructions in ADO's feed connection: https://dev.azure.com/soundbiteai/Soundbite/_artifacts/feed/Soundbite/connect/npm