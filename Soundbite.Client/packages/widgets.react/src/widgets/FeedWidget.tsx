/** @jsx jsx */
import { jsx } from "@emotion/react";
import React from "react";
import { observer } from "mobx-react-lite";

import { Person, SessionPreview, SessionSecurityType } from "@soundbite/api";

import { IconProp } from "@fortawesome/fontawesome-svg-core";
import { TableFeedWidget } from "./TableFeedWidget";
import { CardFeedWidget } from "./CardFeedWidget";

export enum FeedMode {
  Table,
  Card,
}

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  currentUser?: Person;
  groupRoute?: string; // Route of the group to which sessions are limited if sessions are loaded from the current organization feed.
  icon?: IconProp;
  isReadOnly?: boolean;
  noFeedMessage?: string; // Message displayed when no feed is available
  orgRoute?: string; // Optional organization route
  securityType?: SessionSecurityType;
  sessions?: SessionPreview[]; // Sessions to display in the feed.  When not provided sessions are acquired from the current organization feed.
  showCreateButton?: boolean; // Flag indicating whether the create button should be displayed
  showNoFeedImage?: boolean; // Flag indicating whether the empty card art image should be displayed when there is no feed data
  showTitle?: boolean;
  title?: string;
  mode?: FeedMode;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const FeedWidget: React.FC<IProps> = observer((props: IProps) => {
  if (props.mode === FeedMode.Card) {
    return <CardFeedWidget {...props} />;
  }

  return <TableFeedWidget {...props} />;
});
