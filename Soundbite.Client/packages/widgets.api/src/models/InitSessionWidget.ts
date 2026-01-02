/***************************************************************************************************
 * WARNING - models are duplicated between projects.  Only modify model files that reside in the
 *   Soundbite.npm.react.widgets.app project.  If you see a _SyncModels.ps1 file in the same
 *   directory as the model files you are in the right place.  Use the _SyncModels.ps1 file to copy
 *   files into the Soundbite.npm.widgets.api project
 **************************************************************************************************/

export class InitSessionWidget {
  authToken: string;
  constructor(authToken: string) {
    this.authToken = authToken;
  }
}
