/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ClipHostingType, ClipType, FileType, ParticipantRole } from '../enums';
import { IMediaEffect } from './IMediaEffect.model';

/** 
* Automatically generated model for Soundbite.Models.NewClip
*/
export interface NewClip {
  clipType: ClipType;
  fileType: FileType;
  hostingData?: string;
  hostingType: ClipHostingType;
  mediaEffects?: IMediaEffect[];
  metaData?: string;
  participantRole: ParticipantRole;
  seconds?: number;
  stream?: File;
}
