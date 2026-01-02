import * as React from "react";
import { observer } from "mobx-react-lite";
import {
  DisplayMode,
  Environment,
  EnvironmentType,
} from "@microsoft/sp-core-library";
import { BaseClientSideWebPart } from "@microsoft/sp-webpart-base";

import { SessionPreview, SessionSecurityType } from "@soundbite/api";
import {
  DialogManagerUi,
  FeedWidget,
  Loader,
  SbBootstrapStyles,
  SbDashboardStyles,
  SbStyles,
  WidgetStore,
} from "@soundbite/widgets-react";

import { ISoundbiteFeedProps } from "../interfaces/ISoundbiteFeedProps";
import { WebPartStore } from "../../../services/WebPartStore";
import { WebPartServiceState } from "../../../enums/WebPartServiceState";

/***************************************************************************************************
 *  Component
 **************************************************************************************************/

export const SoundbiteFeed: React.FC<ISoundbiteFeedProps> = observer(
  (props: ISoundbiteFeedProps) => {
    //////////[ Define ]//////////////////////////////////////////////////////////////////////////////

    const [isLoading, setIsLoading] = React.useState<boolean>(true);
    const [errorMsg, setErrorMsg] = React.useState<string>("");
    const [sessions, setSessions] = React.useState<SessionPreview[]>([]);

    const hideTitle = props.hideTitle === true ? true : false;
    const title = props.title;

    //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

    const initialize = async () => {
      if (await isPageInEditMode(props.part)) {
        setErrorMsg(
          "Soundbite Feed Webpart will display after page editing is complete..."
        );
      } else {
        await WebPartStore.initialize(props.context);

        // Setup error message (if applicable)
        switch (WebPartStore.state) {
          case WebPartServiceState.none:
          case WebPartServiceState.init:
            setErrorMsg(
              "Sorry! Something went wrong initializing the webpart."
            );
            return;
          case WebPartServiceState.badConfig:
            setErrorMsg("Please update the web part configuration");
            return;
          case WebPartServiceState.tokenFailed:
            setErrorMsg("Sorry! SharePoint is not issuing an access token.");
            return;
          case WebPartServiceState.apiFailed:
            setErrorMsg("Sorry! Your feed is not available.");
            return;
        }

        // Put this component into loading mode
        setIsLoading(true);
        setSessions([]);

        // Are we org only or org+group?
        const orgRoute = WidgetStore.organizations.currentOrg.details.route;
        if (props.groupRoute) {
          try {
            //TODO: Figure out how to suppport protected/public feeds
            const groupFeed = await WidgetStore.sessions.readGroupFeedAsync(
              orgRoute,
              props.groupRoute,
              SessionSecurityType.Protected
            );
            setSessions(groupFeed);
            setIsLoading(false);
          } catch {
            setIsLoading(false);
            setErrorMsg("Failed to get groups");
          }
        } else {
          //TODO: Figure out how to suppport protected/public feeds
          const feed = await WidgetStore.sessions.readOrgFeedAsync(
            orgRoute,
            SessionSecurityType.Protected
          );
          setSessions(feed);
          setIsLoading(false);
        }
      }
    };

    React.useEffect(() => {
      initialize();
    }, [WidgetStore.organizations.currentOrg, props.groupRoute]);

    //////////[ Methods ]///////////////////////////////////////////////////////////////////////////

    /**
     * Determines whether the page is in edit mode.  This is a Promise because on a classic
     * SharePoint page the ribbon must loaded before edit mode can be determined.
     */
    function isPageInEditMode<T>(webPart: BaseClientSideWebPart<T>): boolean {
      if (Environment.type == EnvironmentType.ClassicSharePoint) {
        // Approach for checking in classic mode did not work.  It also did not seem to break the
        // page when the web part rendered in edit mode, so it is probably not worth the effort
        // to really deal with it since most customers will be using modern pages.
        return false;
      } else if (Environment.type == EnvironmentType.SharePoint) {
        // Webpart is on a Modern SharePoint page so just check the display mode.
        return webPart.displayMode == DisplayMode.Edit;
      }
    }

    function renderError(): JSX.Element {
      return (
        <React.Fragment>
          <div>{errorMsg}</div>
        </React.Fragment>
      );
    }

    function renderLoading() {
      return <Loader isLoadedWhen={!isLoading && errorMsg === ""}></Loader>;
    }

    function renderFeed() {
      return (
        <React.Fragment>
          <SbBootstrapStyles />
          <SbDashboardStyles />
          <SbStyles />
          <FeedWidget
            currentUser={WidgetStore.organizations.currentOrg?.details.me}
            noFeedMessage={"There are no Soundbite Sessions in your feed."}
            orgRoute={WidgetStore.organizations.currentOrg?.details.route}
            sessions={sessions}
            showActions={true}
            showCreateButton={false}
            showTitle={!hideTitle}
            title={title}
          />
          <DialogManagerUi />
        </React.Fragment>
      );
    }

    //////////[ Render UI ]///////////////////////////////////////////////////////////////////////////

    if (errorMsg) {
      return renderError();
    } else if (isLoading) {
      return renderLoading();
    } else {
      return renderFeed();
    }
  }
);
