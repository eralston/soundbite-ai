/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ClipWithContributor } from './ClipWithContributor.model';
import { Prompt } from './Prompt.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';

/** 
* Automatically generated model for Soundbite.Models.PromptWithClips
*/
export interface PromptWithClips extends Prompt, Record, Resource {
  clips: ClipWithContributor[];
  createdUtc: string;
  deletedUtc?: string;
  route: string;
  text: string;
  updatedUtc: string;
}
