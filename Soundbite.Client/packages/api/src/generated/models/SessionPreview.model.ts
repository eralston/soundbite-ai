/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { SessionCommentPolicy, SessionSecurityType, SessionType } from '../enums';
import { Participant } from './Participant.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { Session } from './Session.model';

/** 
* Automatically generated model for Soundbite.Models.SessionPreview
*/
export interface SessionPreview extends Session, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  hostParticipants: Participant[];
  limit: number;
  myParticipants: Participant[];
  name: string;
  publish?: string;
  publishSent?: string;
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
