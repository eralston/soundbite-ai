import React from "react";
import { observer } from "mobx-react-lite";
import QueryString from "query-string";
import Public from "./Public";
import { Card, CardHeader } from "reactstrap";
import { useParams } from "react-router";
import { PublicPlayerWidget, WidgetStore } from "@soundbite/widgets-react";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const PublicSession: React.FC<IProps> = observer((props: IProps) => {
  const { orgRoute, sessionRoute } = useParams<{
    orgRoute: string;
    sessionRoute: string;
  }>();

  const parsedQueryString = QueryString.parse(window.location.hash);
  const userRoute: string | (string | null)[] | null = parsedQueryString["ur"];
  const userRouteStr: string = Array.isArray(userRoute)
    ? userRoute.length > 0
      ? (userRoute[0] as string)
      : ""
    : (userRoute as string);

  // NOTE: this needs to get set here because the MediaPlayerContext needs a way to get the
  //  current org when displaying a public video.  If it makes sense, we can move this setter back
  //  to the ViewRouter.tsx file but there was only one public org component and this is it.
  WidgetStore.organizations.currentOrgRoute = orgRoute;

  return (
    <Public>
      <Card>
        <CardHeader className="rounded">
          <div className="mt-2 mb-3">
            <PublicPlayerWidget
              orgRoute={orgRoute}
              userRoute={userRouteStr}
              sessionRoute={sessionRoute}
            />
          </div>
        </CardHeader>
      </Card>
    </Public>
  );
});
