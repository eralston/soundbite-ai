/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Recurrence } from '../enums';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { Series } from './Series.model';
import { SessionPreview } from './SessionPreview.model';

/** 
* Automatically generated model for Soundbite.Models.SeriesPreview
*/
export interface SeriesPreview extends Series, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  name: string;
  recurrence: Recurrence;
  recurrenceData: string;
  route: string;
  template: SessionPreview;
  updatedUtc: string;
}
