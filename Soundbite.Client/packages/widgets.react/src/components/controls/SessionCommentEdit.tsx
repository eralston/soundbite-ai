import React, { useEffect, useState } from "react";

import { FormGroup, InputGroup, InputGroupAddon } from "reactstrap";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowUp, faCircleXmark } from "@fortawesome/free-solid-svg-icons";
import { SbButton, SbButtonSize, SbButtonType } from "../SbButton";
import { Loader } from "./Loader";
import { SessionComment, SessionCommentsService, Utils } from "@soundbite/api";

import { ErrorDlg } from "../ErrorDlg";
import { FaPrepend } from "./FaPrepend";
import { ShowWhen } from "./ShowWhen";

/***************************************************************************************************
 *  Constants / Global Variables
 **************************************************************************************************/

library.add(faArrowUp, faCircleXmark);

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  comment?: SessionComment;
  onCommit: () => Promise<void>;
  onCancel: () => void;
  maxCharacters?: number;
}

/**
 * Control for taking in text and submitting it
 * @param props
 * @returns
 */
export const SessionCommentEdit: React.FC<IProps> = (props: IProps) => {
  const maxCharacters = props.maxCharacters ?? 150;
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [editedContent, setEditedContent] = useState<string | undefined>(
    props.comment?.content
  );
  const content = editedContent?.trim() ?? "";
  const contentLength = content?.trim().length ?? 0;

  useEffect(() => {
    setEditedContent(props.comment?.content);
  }, [props.comment]);

  /**
   * Callback for async adding a comment and resetting the submit box
   * @returns
   */
  async function onConfirmEdit() {
    try {
      // Can't submit if props had junk in them
      if (
        props.orgRoute == null ||
        props.sessionRoute == null ||
        props.comment == null
      ) {
        return;
      }

      // new content needs to fit parameters
      if (content.length > maxCharacters || content.length === 0) {
        return;
      }

      // Do the update lifecycle
      setIsSaving(true);

      await SessionCommentsService.updateSessionComment(
        props.orgRoute,
        props.sessionRoute,
        props.comment.route,
        { content }
      );

      await props.onCommit();

      setIsSaving(false);
    } catch (err) {
      ErrorDlg.show(err, "Error Updating Comment");
    }
  }

  const onKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      onConfirmEdit();
    }
  };

  function onCancel() {
    props.onCancel();
  }

  const isWarning = contentLength > maxCharacters - 50;
  const isBeyondLimit = contentLength > maxCharacters;
  let warningClass = "";
  if (isWarning) {
    warningClass = "text-warning";
  }
  if (isBeyondLimit) {
    warningClass = "text-danger";
  }

  return (
    <div className="sb-comment-edit mt-2">
      <Loader isLoadedWhen={!isSaving}>
        <FormGroup className="pr-2 mb-0">
          <InputGroup aria-disabled={isSaving}>
            <FaPrepend icon="pen" />
            <input
              type="text"
              className="form-control"
              placeholder="Updated Content..."
              aria-label="Updated Content"
              aria-describedby="basic-addon1"
              value={editedContent}
              onChange={(e) => setEditedContent(e.target.value)}
              disabled={isSaving}
              title="Updated Content"
              onKeyDown={onKeyDown}
            />
            <InputGroupAddon className={"sb-addon-btn"} addonType="append">
              <SbButton
                type={SbButtonType.Primary}
                size={SbButtonSize.Small}
                btnClassName="sb-addon-btn"
                title="Confirm Edit"
                onClick={onConfirmEdit}
                icon={faArrowUp}
              />
            </InputGroupAddon>
            <InputGroupAddon className={"sb-addon-btn"} addonType="append">
              <SbButton
                type={SbButtonType.Secondary}
                size={SbButtonSize.Small}
                btnClassName="sb-addon-btn"
                title="Cancel Edit"
                onClick={onCancel}
                icon={faCircleXmark}
              />
            </InputGroupAddon>
          </InputGroup>
        </FormGroup>
        <div className="text-sm text-muted">
          {Utils.formatRelativeDate(props.comment?.createdUtc)}
        </div>
        <ShowWhen is={isWarning}>
          <div className={`${warningClass} text-sm`}>
            Max {content.length}/{maxCharacters} characters allowed
          </div>
        </ShowWhen>
      </Loader>
    </div>
  );
};
