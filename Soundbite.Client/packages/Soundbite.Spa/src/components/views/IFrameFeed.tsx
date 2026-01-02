import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { useParams } from "react-router-dom";
import { checkDialogHash, FeedWidget } from "@soundbite/widgets-react";

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const IFrameFeed: React.FC = observer(() => {
  // Determine whether automatic play/record has been specified in the hash portion of the URL

  const { orgRoute, groupRoute } = useParams<{
    orgRoute: string;
    groupRoute: string;
  }>();

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  // When the widget first loads
  useEffect(() => {
    checkDialogHash(orgRoute, groupRoute);
    // eslint-disable-next-line
  }, []);

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return <FeedWidget orgRoute={orgRoute} groupRoute={groupRoute} />;
});
