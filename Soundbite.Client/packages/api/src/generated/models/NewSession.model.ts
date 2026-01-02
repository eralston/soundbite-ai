/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Recurrence, SessionCommentPolicy, SessionSecurityType, SessionType } from '../enums';
import { NewParticipant } from './NewParticipant.model';
import { NewParticipantGroup } from './NewParticipantGroup.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { Session } from './Session.model';

/** 
* Automatically generated model for Soundbite.Models.NewSession
*/
export interface NewSession extends Session, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  firstPrompt: string;
  groups: NewParticipantGroup[];
  limit: number;
  name: string;
  participants: NewParticipant[];
  publish?: string;
  publishSent?: string;
  recurrence: Recurrence;
  recurrenceData: string;
  reminder?: string;
  reminderCalEventId: string;
  reminderSent?: string;
  route: string;
  sessionCommentPolicy: SessionCommentPolicy;
  sessionSecurity: SessionSecurityType;
  sessionType: SessionType;
  transcribe: boolean;
  updatedUtc: string;
}
