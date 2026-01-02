/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ActivityHighlight } from './ActivityHighlight.model';
import { ReportBase } from './ReportBase.model';

/** 
* Automatically generated model for Soundbite.Services.ActivityReport
*/
export interface ActivityReport extends ReportBase {
  consumerCount: number;
  endUtc: string;
  highlights: ActivityHighlight[];
  name: string;
  orgRoute: string;
  producerCount: number;
  startUtc: string;
  unitsConsumed: number;
  unitsProduced: number;
}
