/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { useEffect, useRef, useState } from "react";

import { FormGroup, InputGroup, InputGroupAddon } from "reactstrap";
import { library } from "@fortawesome/fontawesome-svg-core";
import {
  faArrowUp,
  faComment,
  faPlus,
  faTrash,
} from "@fortawesome/free-solid-svg-icons";
import { SbButton, SbButtonSize, SbButtonType } from "../SbButton";
import { Loader } from "./Loader";
import { SessionCommentsService } from "@soundbite/api";

import { ErrorDlg } from "../ErrorDlg";
import { ShowWhen } from "./ShowWhen";

/***************************************************************************************************
 *  Constants / Global Variables
 **************************************************************************************************/

library.add(faPlus, faTrash, faComment);

const getStyles = () => {
  const ret = css``;
  return ret;
};

let styles: SerializedStyles | undefined = undefined;

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  onCommit: () => Promise<void>;
  maxCharacters?: number;
}

/**
 * Control for taking in text and submitting it
 * @param props
 * @returns
 */
export const SessionCommentCreate: React.FC<IProps> = (props: IProps) => {
  const maxCharacters = props.maxCharacters ?? 150;
  const [newComment, setNewComment] = useState<string>("");
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const inputRef = useRef<HTMLInputElement>(null);

  /**
   * Callback for async adding a comment and resetting the submit box
   * @returns
   */
  async function onCommit() {
    try {
      const content = newComment.trim();

      // Can't submit something too long
      if (content.length > maxCharacters || content.length === 0) {
        return;
      }

      if (props.orgRoute == null || props.sessionRoute == null) {
        return;
      }

      setIsSaving(true);

      await SessionCommentsService.createSessionComment(
        props.orgRoute,
        props.sessionRoute,
        { content }
      );

      setNewComment("");
      setIsSaving(false);

      await props.onCommit();

      // After we're done processing, set focus to the input
      // This focus interaction is the reason this control and its parent must use
      // isDisplayBased={true} in its attributes; perhaps figure out a better way
      if (inputRef.current) {
        inputRef.current.focus();
      }
    } catch (err) {
      ErrorDlg.show(err, "Error Creating Comment");
    }
  }

  const onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      onCommit();
    }
  };

  /** Re-render when comments change */
  useEffect(() => {}, [newComment]);

  // Setting up render state
  if (styles == null) {
    styles = getStyles();
  }

  const isWarning = newComment.length > maxCharacters - 50;
  const isBeyondLimit = newComment.length > maxCharacters;
  let warningClass = "";
  if (isWarning) {
    warningClass = "text-warning";
  }
  if (isBeyondLimit) {
    warningClass = "text-danger";
  }

  return (
    <div className="sb-comment-create mt-2" css={styles}>
      <Loader isLoadedWhen={!isSaving} isDisplayBased={true}>
        <FormGroup>
          <InputGroup aria-disabled={isSaving}>
            <input
              type="text"
              className="form-control"
              placeholder="Add Comment..."
              aria-label="Title"
              aria-describedby="basic-addon1"
              onChange={(e) => setNewComment(e.target.value)}
              disabled={isSaving}
              title="Add Comment"
              value={newComment}
              onKeyDown={onKeyDown}
              maxLength={maxCharacters}
              ref={inputRef}
            />
            <InputGroupAddon
              className={"sb-addon-btn sb-add-comment-btn-addon"}
              addonType="append"
            >
              <SbButton
                type={SbButtonType.Primary}
                size={SbButtonSize.Small}
                btnClassName="sb-addon-btn sb-add-comment-btn"
                title="Add Comment"
                onClick={onCommit}
                icon={faArrowUp}
                disabled={isBeyondLimit}
              />
            </InputGroupAddon>
          </InputGroup>
          <ShowWhen is={isWarning}>
            <div className={`${warningClass} text-sm`}>
              Max {newComment.length}/{maxCharacters} characters allowed
            </div>
          </ShowWhen>
        </FormGroup>
      </Loader>
    </div>
  );
};
