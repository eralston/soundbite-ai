/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Recurrence } from '../enums';
import { CalendarEntry } from './CalendarEntry.model';
import { RecurrencePattern } from './RecurrencePattern.model';

/** 
* Automatically generated model for Masticore.Providers.Calendar.SessionCalendarEntry
*/
export interface SessionCalendarEntry extends CalendarEntry {
  body: string;
  duration: number;
  id: string;
  orgRoute: string;
  recurrence: Recurrence;
  recurrencePattern: RecurrencePattern;
  seriesRoute: string;
  sessionRoute: string;
  startTime: string;
  subject: string;
}
