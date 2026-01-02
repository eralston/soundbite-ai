/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ActivityHighlight } from './ActivityHighlight.model';
import { ActivityRange } from './ActivityRange.model';
import { Participant } from './Participant.model';
import { ParticipantGroup } from './ParticipantGroup.model';
import { SessionContentNotificiation } from './SessionContentNotificiation.model';
import { SessionContentPersonEvent } from './SessionContentPersonEvent.model';
import { SessionContentReport } from './SessionContentReport.model';

/** 
* Automatically generated model for Soundbite.Models.SessionContentDetailsReport
*/
export interface SessionContentDetailsReport extends SessionContentReport {
  acknowledgeCount: ActivityHighlight;
  acknowledgers: SessionContentPersonEvent[];
  audience: Participant[];
  audienceGroups: ParticipantGroup[];
  audienceSize: ActivityHighlight;
  consumeCount: ActivityHighlight;
  consumeCountOverTime: ActivityRange;
  consumerCount: ActivityHighlight;
  endUtc: string;
  highlights: ActivityHighlight[];
  name: string;
  notifications: SessionContentNotificiation[];
  orgRoute: string;
  plays: SessionContentPersonEvent[];
  sessionRoute: string;
  startUtc: string;
}
