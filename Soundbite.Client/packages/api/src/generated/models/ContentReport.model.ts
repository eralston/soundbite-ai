/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ActivityHighlight } from './ActivityHighlight.model';
import { ActivityRange } from './ActivityRange.model';
import { ReportBase } from './ReportBase.model';

/** 
* Automatically generated model for Soundbite.Services.ContentReport
*/
export interface ContentReport extends ReportBase {
  acknowledgeCount: ActivityHighlight;
  consumeCount: ActivityHighlight;
  consumeCountOverTime: ActivityRange;
  endUtc: string;
  highlights: ActivityHighlight[];
  name: string;
  orgRoute: string;
  startUtc: string;
}
