/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ActivityHighlight } from './ActivityHighlight.model';
import { ActivityRange } from './ActivityRange.model';
import { ContentReport } from './ContentReport.model';
import { SessionPreview } from './SessionPreview.model';

/** 
* Automatically generated model for Soundbite.Models.OrgContentReport
*/
export interface OrgContentReport extends ContentReport {
  acknowledgeCount: ActivityHighlight;
  consumeCount: ActivityHighlight;
  consumeCountOverTime: ActivityRange;
  endUtc: string;
  highlights: ActivityHighlight[];
  name: string;
  orgRoute: string;
  recentSessions: SessionPreview[];
  seriesCount: ActivityHighlight;
  sessionsCount: ActivityHighlight;
  sessionsCountOverTime: ActivityRange;
  startUtc: string;
}
