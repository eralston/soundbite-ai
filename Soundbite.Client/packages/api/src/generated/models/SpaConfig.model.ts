/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { AdClientConfig } from './AdClientConfig.model';
import { AdPlatformConfig } from './AdPlatformConfig.model';
import { FeatureFlags } from './FeatureFlags.model';

/** 
* Complete configuration object for the front-end
*/
export interface SpaConfig {
  /***
   * The Active Directory client configuration, with key fields like URLs, keys, and scopes
   */
  adClientConfig: AdClientConfig;

  /***
   * Gets or sets the platform configuration that the SPA needs to know about
   */
  adPlatformConfig: AdPlatformConfig;

  /***
   * Features flags for the front-end of the application
   */
  featureFlags: FeatureFlags;

  /***
   * The credentials for connecting to the client-side telemetry tooling; in most cases this should be Azure App Insights
   */
  telemetryKey: string;
}
