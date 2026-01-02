/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ClipHostingType, ClipState, TranscriptState } from '../enums';

/** 
* Automatically generated model for Soundbite.Models.ClipDetails
*/
export interface ClipDetails {
  clipRoute: string;
  downloadUrl?: string;
  hostingData: string;
  hostingType: ClipHostingType;
  orgRoute: string;
  promptRoute: string;
  sessionRoute: string;
  state: ClipState;
  transcriptState: TranscriptState;
  uploadUrl?: string;
}
