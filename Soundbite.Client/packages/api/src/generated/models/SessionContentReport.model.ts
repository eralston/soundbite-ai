/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ActivityHighlight } from './ActivityHighlight.model';
import { ActivityRange } from './ActivityRange.model';
import { ContentReport } from './ContentReport.model';

/** 
* Automatically generated model for Soundbite.Models.SessionContentReport
*/
export interface SessionContentReport extends ContentReport {
  acknowledgeCount: ActivityHighlight;
  audienceSize: ActivityHighlight;
  consumeCount: ActivityHighlight;
  consumeCountOverTime: ActivityRange;
  consumerCount: ActivityHighlight;
  endUtc: string;
  highlights: ActivityHighlight[];
  name: string;
  orgRoute: string;
  sessionRoute: string;
  startUtc: string;
}
