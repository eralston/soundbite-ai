/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { ClientOpWrapper, IndexPageRequest, IndexPageResponse, NewSession, Participant, ParticipantStateUpdate, ReactionSummary, Session, SessionDetails, SessionPreview, SessionSummary } from '../models/index';
import { ParticipantReactionType, SessionSummaryType } from '../enums/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.SessionsController
 **/
class SessionsServiceClass {

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async createAsync_Obsolete(orgRoute: string, newSession: NewSession, options?: IHttpRequestOptions
): Promise<SessionDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions`;
    const response = await HttpService.post<ClientOpWrapper<SessionDetails>>(url, newSession, options);
    await SoundbiteApiConfig.clientOpsHandler(response?.clientOps);
    return response.result;
  }

  /** 
  * Automatically generated API call
  */
  async createAsync(orgRoute: string, newSession: NewSession, options?: IHttpRequestOptions
): Promise<SessionDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/New`;
    const response = await HttpService.post<SessionDetails>(url, newSession, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readAllAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<Session[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions`;
    const response = await HttpService.get<Session[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readPendingAsync(orgRoute: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<SessionPreview>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/Pending?&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<SessionPreview>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readPastAsync(orgRoute: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<SessionPreview>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/Past?&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<SessionPreview>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readFeedAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<SessionPreview[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Feed`;
    const response = await HttpService.get<SessionPreview[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readPublicFeedAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<SessionPreview[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Public/Feed`;
    const response = await HttpService.get<SessionPreview[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readAllGroupSessionsAsync(orgRoute: string, groupRoute: string, options?: IHttpRequestOptions
): Promise<Session[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Sessions`;
    const response = await HttpService.get<Session[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readGroupPendingAsync(orgRoute: string, groupRoute: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<SessionPreview>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Sessions/Pending?&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<SessionPreview>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readGroupPastAsync(orgRoute: string, groupRoute: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<SessionPreview>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Sessions/Past?&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<SessionPreview>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readGroupFeedAsync(orgRoute: string, groupRoute: string, options?: IHttpRequestOptions
): Promise<SessionPreview[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Feed`;
    const response = await HttpService.get<SessionPreview[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readGroupPublicFeedAsync(orgRoute: string, groupRoute: string, options?: IHttpRequestOptions
): Promise<SessionPreview[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Public/Groups/${encodeURIComponent(groupRoute)}/Feed`;
    const response = await HttpService.get<SessionPreview[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readRecentlyPublishedAsync(orgRoute: string, skip: number = 0, take: number = 10, options?: IHttpRequestOptions
): Promise<SessionPreview[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/Published?skip=${encodeURIComponent(skip)}&take=${encodeURIComponent(take)}`;
    const response = await HttpService.get<SessionPreview[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAsync(orgRoute: string, sessionRoute: string, options?: IHttpRequestOptions
): Promise<SessionDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}`;
    const response = await HttpService.get<SessionDetails>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readReactionsAsync(orgRoute: string, sessionRoute: string, options?: IHttpRequestOptions
): Promise<ReactionSummary[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/reactions`;
    const response = await HttpService.get<ReactionSummary[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async summaryAsync(orgRoute: string, sessionRoute: string, summaryType: SessionSummaryType = SessionSummaryType.Paragraph, options?: IHttpRequestOptions
): Promise<SessionSummary> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/summary?summaryType=${encodeURIComponent(summaryType)}`;
    const response = await HttpService.get<SessionSummary>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readPublicAsync(orgRoute: string, sessionRoute: string, options?: IHttpRequestOptions
): Promise<SessionDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Public/Sessions/${encodeURIComponent(sessionRoute)}`;
    const response = await HttpService.get<SessionDetails>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateAsync(orgRoute: string, sessionRoute: string, session: NewSession, options?: IHttpRequestOptions
): Promise<SessionDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}`;
    const response = await HttpService.put<SessionDetails>(url, session, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateStateAsync(orgRoute: string, sessionRoute: string, partUpdate: ParticipantStateUpdate, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/state`;
    await HttpService.put(url, partUpdate, options);
  }

  /** 
  * Automatically generated API call
  */
  async updateReactionAsync(orgRoute: string, sessionRoute: string, reactionType: ParticipantReactionType = ParticipantReactionType.None, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/reaction`;
    await HttpService.put(url, reactionType, options);
  }

  /** 
  * Automatically generated API call
  */
  async acknowledgePublicSession(orgRoute: string, sessionRoute: string, userRoute?: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/public/Sessions/${encodeURIComponent(sessionRoute)}/acknowledge?userRoute=${encodeURIComponent(userRoute ?? "")}`;
    await HttpService.put(url, null, options);
  }

  /** 
  * Automatically generated API call
  */
  async deleteAsync(orgRoute: string, sessionRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}`;
    await HttpService.delete(url, options);
  }

  /** 
  * Automatically generated API call
  */
  async setReminderCalendarEntryId(orgRoute: string, sessionRoute: string, reminderCalendarEventId: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/SetReminderCalendarEntryId`;
    await HttpService.post(url, reminderCalendarEventId, options);
  }

  /** 
  * Automatically generated API call
  */
  async readParticipants(orgRoute: string, sessionRoute: string, includeRoles?: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<Participant>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/Participants/Page?includeRoles=${encodeURIComponent(includeRoles ?? "")}&&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<Participant>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readPariticpantsInGroup(orgRoute: string, sessionRoute: string, groupRoute: string, includeRoles?: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<Participant>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/Participants/Groups/${encodeURIComponent(groupRoute)}/Page?includeRoles=${encodeURIComponent(includeRoles ?? "")}&&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<Participant>>(url, options);
    return response;
  }
}

export const SessionsService = new SessionsServiceClass();
