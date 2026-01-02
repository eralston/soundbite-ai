/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { NotificationChannel, NotificationStatus, SessionNotificationType } from '../enums';
import { SessionContentPersonEvent } from './SessionContentPersonEvent.model';
import { UserFields } from './UserFields.model';

/** 
* Automatically generated model for Soundbite.Models.SessionContentNotificiation
*/
export interface SessionContentNotificiation extends SessionContentPersonEvent, UserFields {
  channel: NotificationChannel;
  dateTimeUtc: string;
  details?: string;
  email: string;
  familyName: string;
  givenName: string;
  notificationType: SessionNotificationType;
  phone: string;
  status: NotificationStatus;
  title: string;
  universalId: string;
  userRoute: string;
}
