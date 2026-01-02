import OrgStore from "./OrgStore";

/**
 * Acts as a container to manage the various stores used in the application
 **/
class AdminStore {
  orgStore: OrgStore = new OrgStore();
}

// Export an app store instance representing the application state
var adminStore = new AdminStore();

export default adminStore;
