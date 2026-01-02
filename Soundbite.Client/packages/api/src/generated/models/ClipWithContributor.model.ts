/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ClipHostingType, ClipType, FileType, MediaProcessingState, TranscriptState } from '../enums';
import { Clip } from './Clip.model';
import { IMediaEffect } from './IMediaEffect.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { User } from './User.model';

/** 
* Automatically generated model for Soundbite.Models.ClipWithContributor
*/
export interface ClipWithContributor extends Clip, Record, Resource {
  clipType: ClipType;
  contributor: User;
  createdUtc: string;
  deletedUtc?: string;
  fileType: FileType;
  hostingData: string;
  hostingType: ClipHostingType;
  mediaEffects?: IMediaEffect[];
  mediaProcessingState: MediaProcessingState;
  route: string;
  seconds: number;
  transcriptState: TranscriptState;
  updatedUtc: string;
  url: string;
}
