/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ParticipantReactionType, ParticipantRole, ParticipantState } from '../enums';
import { Person } from './Person.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Soundbite.Models.Participant
*/
export interface Participant extends ResourceBase, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  participantRole: ParticipantRole;
  participantState: ParticipantState;
  person: Person;
  reactionType: ParticipantReactionType;
  route: string;
  sessionRoute: string;
  updatedUtc: string;
}
