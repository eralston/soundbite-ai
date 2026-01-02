/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Recurrence } from '../enums';
import { RecurrencePattern } from './RecurrencePattern.model';

/** 
* Automatically generated model for Masticore.Providers.Calendar.CalendarEntry
*/
export interface CalendarEntry {
  body: string;
  duration: number;
  id: string;
  recurrence: Recurrence;
  recurrencePattern: RecurrencePattern;
  startTime: string;
  subject: string;
}
