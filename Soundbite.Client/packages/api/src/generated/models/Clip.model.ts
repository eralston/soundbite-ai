/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ClipHostingType, ClipType, FileType, MediaProcessingState, TranscriptState } from '../enums';
import { IMediaEffect } from './IMediaEffect.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Soundbite.Models.Clip
*/
export interface Clip extends ResourceBase, Record, Resource {
  clipType: ClipType;
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
}
