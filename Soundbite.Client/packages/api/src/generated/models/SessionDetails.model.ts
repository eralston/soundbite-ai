/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { SessionCommentPolicy, SessionSecurityType, SessionType } from '../enums';
import { Participant } from './Participant.model';
import { ParticipantGroup } from './ParticipantGroup.model';
import { PromptWithClips } from './PromptWithClips.model';
import { ReactionSummary } from './ReactionSummary.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { SeriesPreview } from './SeriesPreview.model';
import { Session } from './Session.model';

/** 
* Automatically generated model for Soundbite.Models.SessionDetails
*/
export interface SessionDetails extends Session, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  groups: ParticipantGroup[];
  limit: number;
  myParticipation: Participant[];
  name: string;
  participants: Participant[];
  prompts: PromptWithClips[];
  publish?: string;
  publishSent?: string;
  reactions: ReactionSummary[];
  reminder?: string;
  reminderCalEventId: string;
  reminderSent?: string;
  route: string;
  series: SeriesPreview;
  sessionCommentPolicy: SessionCommentPolicy;
  sessionSecurity: SessionSecurityType;
  sessionType: SessionType;
  transcribe: boolean;
  updatedUtc: string;
}
