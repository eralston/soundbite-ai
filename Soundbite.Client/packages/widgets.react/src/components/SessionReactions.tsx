/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faFaceFlushed,
  faFaceLaugh,
  faFrown,
  faHeart,
  faThumbsUp,
} from "@fortawesome/free-solid-svg-icons";

import {
  ParticipantReactionType,
  ParticipantRole,
  ReactionSummary,
  SessionDetails,
  SessionsService,
} from "@soundbite/api";
import { IconProp } from "@fortawesome/fontawesome-svg-core";
import { ShowWhen } from "./controls";
import { Button } from "reactstrap";

/***************************************************************************************************
 *  Constants / Global Variables
 **************************************************************************************************/

const getStyles = () => {
  const ret = css`
    .btn-inner--text {
      font-size: 1rem;
      margin-left: 0.25rem;
      font-weight: 300;
    }

    svg {
      margin-top: 1px;
      width: 1.25rem;
      height: 1.25rem;
    }
  `;
  return ret;
};

let styles: SerializedStyles | undefined = undefined;

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  session?: SessionDetails;
  readOnly?: boolean;
}

/**
 * Provides a standard way of creating buttons in Soundbite Studio.
 */
export const SessionReactions: React.FC<IProps> = (props: IProps) => {
  //////////[ Methods ]///////////////////////////////////////////////////////////////////

  const [reactions, setReactions] = useState<ReactionSummary[] | undefined>(
    props.session?.reactions
  );
  const [myReaction, setMyReaction] = useState<
    ParticipantReactionType | undefined
  >(props.session?.myParticipation[0]?.reactionType);

  const sessionRoute = props.sessionRoute ?? props.session?.route;
  const isReadOnly = props.readOnly ?? false;

  const applyReactions = (reactions: ReactionSummary[]) => {
    setReactions(reactions);
    setMyReaction(undefined);
  };

  // Load the session, but only at the scope of this control in case we need changes or they didn't load it before showing this
  const load = async (forceLoad: boolean = false) => {
    // If we have not context, then we can't load
    if (props.orgRoute == null) {
      return;
    }

    if (sessionRoute == null) {
      return;
    }

    // We only load if we haven't already OR they're making us do it
    const needToLoad =
      reactions == null && props.orgRoute != null && props.sessionRoute != null;
    if (forceLoad || needToLoad) {
      const newReactions = await SessionsService.readReactionsAsync(
        props.orgRoute,
        sessionRoute
      );
      applyReactions(newReactions);
    }
  };

  useEffect(() => {
    const doLoad = () => {
      load();
    };
    doLoad();
  }, []);

  const removeReaction = (reactionType: ParticipantReactionType) => {
    if (props.session == null) {
      return;
    }

    setMyReaction(undefined);

    const oldReaction = props.session.reactions.find(
      (r) => r.reactionType === reactionType
    );
    if (oldReaction == null) {
      return;
    } else {
      if (oldReaction.count <= 0) {
        return;
      }
      oldReaction.count -= 1;
    }
  };

  const addReaction = (reactionType: ParticipantReactionType) => {
    if (props.session == null) {
      return;
    }

    setMyReaction(reactionType);
    const oldReaction = props.session.reactions.find(
      (r) => r.reactionType === reactionType
    );
    if (oldReaction == null) {
      props.session.reactions.push({ reactionType, count: 1 });
    } else {
      oldReaction.count += 1;
    }
  };

  const calcNewReaction = (
    reactionType: ParticipantReactionType
  ): ParticipantReactionType => {
    if (props.session == null) {
      return reactionType;
    }

    // Having no reactions also mean they don't verifiably have participant records on this session, though they might have a host role
    const oldReaction = props.session.myParticipation.find(
      (p) =>
        p.participantRole === ParticipantRole.Audience ||
        p.participantRole === ParticipantRole.Participant
    )?.reactionType;

    if (oldReaction == null) {
      // It's a new reaction with no old one
      addReaction(reactionType);
      return reactionType;
    } else if (oldReaction === reactionType) {
      // It's the same reaction, so remove entirely
      removeReaction(oldReaction);
      return ParticipantReactionType.None;
    } else {
      // It's a novel reaction, so change it
      removeReaction(oldReaction);
      addReaction(reactionType);
      return reactionType;
    }
  };

  const onReaction = async (reactionType: ParticipantReactionType) => {
    if (props.orgRoute == null || sessionRoute == null) {
      return;
    }

    reactionType = calcNewReaction(reactionType);

    await SessionsService.updateReactionAsync(
      props.orgRoute,
      sessionRoute,
      reactionType
    );
    await load(true);
  };

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  const iconForReactionType = (
    reactionType: ParticipantReactionType
  ): IconProp | undefined => {
    switch (reactionType) {
      case ParticipantReactionType.None:
        return undefined;
      case ParticipantReactionType.Like:
        return faThumbsUp;
      case ParticipantReactionType.Love:
        return faHeart;
      case ParticipantReactionType.Laugh:
        return faFaceLaugh;
      case ParticipantReactionType.Wow:
        return faFaceFlushed;
      case ParticipantReactionType.Sad:
        return faFrown;
      default:
        return undefined;
    }
  };

  const reactionBtn = (reactionType: ParticipantReactionType) => {
    const iconName = iconForReactionType(reactionType);
    if (iconName == null) {
      return <React.Fragment></React.Fragment>;
    }

    const number =
      reactions?.find((r) => r.reactionType === reactionType)?.count ?? 0;
    let classes = "";
    if (isReadOnly) {
      classes += "dark";
    } else {
      classes += "secondary";
    }
    let handler = isReadOnly
      ? undefined
      : () => {
          onReaction(reactionType);
        };

    const isMyReaction = myReaction === reactionType;

    return (
      <Button
        outline={true}
        size={"sm"}
        color={classes}
        className={classes}
        onClick={handler}
        disabled={isReadOnly}
        active={isMyReaction}
      >
        <span className="btn-inner--icon">
          <FontAwesomeIcon icon={iconName}></FontAwesomeIcon>
        </span>
        <ShowWhen is={isReadOnly || number > 0}>
          <span className="btn-inner--text">{number}</span>
        </ShowWhen>
      </Button>
    );
  };

  if (styles == null) {
    styles = getStyles();
  }

  return (
    <div className="sb-reaction-summary mt-2 mb-2" css={styles}>
      {reactionBtn(ParticipantReactionType.Like)}
      {reactionBtn(ParticipantReactionType.Love)}
      {reactionBtn(ParticipantReactionType.Laugh)}
      {reactionBtn(ParticipantReactionType.Wow)}
      {reactionBtn(ParticipantReactionType.Sad)}
    </div>
  );
};
