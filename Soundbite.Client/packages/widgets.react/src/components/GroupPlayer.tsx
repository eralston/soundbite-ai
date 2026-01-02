import React, { useState, useEffect } from "react";
import { Card, CardHeader, CardBody, Row, Col } from "reactstrap";

import {
  SessionDetails,
  ClipWithContributor,
  SessionType,
} from "@soundbite/api";

import { GroupPlayerRow } from "./GroupPlayerRow";
import { PlayButton } from "./controls";
import { IMediaPlayerContext } from "./video/context/IMediaPlayerContext";
import { Scrubber2 } from "./SoundScrubber2";
import { MediaPlayerContextFactory } from "./video/context/MediaPlayerContextFactory";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  currentUserRoute?: string;
  session?: SessionDetails;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/

/**
 * Displays a series of sessions allowing users to play through the list of content.
 */
export const GroupPlayer: React.FC<IProps> = (props: IProps) => {
  //////////[ State ]///////////////////////////////////////////////////////////////////////////////

  const [contexts, setContexts] = useState<IMediaPlayerContext[]>([]);
  const [currentContextIndex, setCurrentContextIndex] = useState<number>(0);

  //////////[ Variables ]///////////////////////////////////////////////////////////////////////////

  let clips: ClipWithContributor[] = trimClipsBySessionType();

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  useEffect(() => {
    const contexts: IMediaPlayerContext[] = new Array<IMediaPlayerContext>(
      length
    );
    // Initialize the contexts and load media URLs
    for (let i = 0; i < contexts.length - 1; i++) {
      // There should be a 1:1 ratio of contexts to clips
      const currentClip = clips[i];
      contexts[i] = MediaPlayerContextFactory.getContext(currentClip);
      contexts[i].onPlaybackComplete = () => {
        onRowEnded();
      };
      if (currentClip) {
        contexts[i].load(currentClip);
      }
    }
    setContexts(contexts);
  }, [props.session, props.session?.prompts[0].clips]);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible trimming down all cips in a session to only applicable clips
   */
  function trimClipsBySessionType(): ClipWithContributor[] {
    let result: ClipWithContributor[] = props.session?.prompts[0].clips || [];

    // For meetings trim down the clips to those other than the current user
    if (props.session?.sessionType === SessionType.Meeting) {
      result = result.filter(
        (clip) => clip.contributor.route !== props.currentUserRoute
      );
    }

    return result;
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  const onPlayToggle = () => {
    const context = contexts[currentContextIndex];
    if (context.isPlaying) {
      context.pause();
    } else {
      context.play();
    }
  };

  const onRowEnded = () => {
    // Determine if current context is NOT the last context.
    if (currentContextIndex < contexts.length - 2) {
      contexts[currentContextIndex + 1].play();
      setCurrentContextIndex(currentContextIndex + 1);
    }
  };

  //////////[ Build UI ]////////////////////////////////////////////////////////////////////////////

  return (
    <Card className="sb-teamplayer">
      <CardHeader>
        <Row>
          <Col xs="6">
            <h5 className="h3 mb-0">Participants</h5>
          </Col>
          <Col>
            <PlayButton
              onToggle={onPlayToggle}
              context={contexts[currentContextIndex]}
            />
          </Col>
        </Row>
      </CardHeader>
      <CardBody>
        <ul className="list-group list-group-flush list my--3">
          {clips.map((clip: ClipWithContributor, key) => {
            return (
              <GroupPlayerRow key={key} clip={clip}>
                <Scrubber2 context={contexts[key]} />
              </GroupPlayerRow>
            );
          })}
        </ul>
      </CardBody>
    </Card>
  );
};
