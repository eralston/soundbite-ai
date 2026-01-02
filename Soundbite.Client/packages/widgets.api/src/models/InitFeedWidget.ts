export class InitFeedWidget {
  authToken: string;
  orgRoute: string;
  groupRoute: string;
  constructor(authToken: string, orgRoute: string, groupRoute?: string) {
    this.authToken = authToken;
    this.orgRoute = orgRoute;
    this.groupRoute = groupRoute || "";
  }
}
