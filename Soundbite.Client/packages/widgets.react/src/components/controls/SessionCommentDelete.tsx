import React, { useEffect, useState } from "react";

import { Col, FormGroup, InputGroup, InputGroupAddon, Row } from "reactstrap";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faComment, faPlus, faTrash } from "@fortawesome/free-solid-svg-icons";
import { Loader } from "./Loader";
import { SessionComment, SessionCommentsService, Utils } from "@soundbite/api";

import { ErrorDlg } from "../ErrorDlg";

/***************************************************************************************************
 *  Constants / Global Variables
 **************************************************************************************************/

library.add(faPlus, faTrash, faComment);

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  comment?: SessionComment;
  onCommit: () => Promise<void>;
  onCancel: () => void;
}

/**
 * Control for taking in text and submitting it
 * @param props
 * @returns
 */
export const SessionCommentDelete: React.FC<IProps> = (props: IProps) => {
  const [isSaving, setIsSaving] = useState<boolean>(false);

  /**
   * Callback for async adding a comment and resetting the submit box
   * @returns
   */
  async function onCommitDelete() {
    try {
      // Can't submit if props had junk in them
      if (
        props.orgRoute == null ||
        props.sessionRoute == null ||
        props.comment == null
      ) {
        return;
      }

      // Do the update lifecycle
      setIsSaving(true);

      await SessionCommentsService.deleteSessionComment(
        props.orgRoute,
        props.sessionRoute,
        props.comment.route
      );

      await props.onCommit();

      setIsSaving(false);
    } catch (err) {
      ErrorDlg.show(err, "Error Deleting Comment");
    }
  }

  function onCancel() {
    props.onCancel();
  }

  /** Re-render when comments change */
  useEffect(() => {}, [props.comment]);

  return (
    <div className="sb-comment-edit mt-2">
      <Loader isLoadedWhen={!isSaving}>
        <div>{props.comment?.content}</div>
        <Row className="sb-comment-subheader text-muted text-sm">
          <Col>{Utils.formatRelativeDate(props.comment?.createdUtc)}</Col>
          <Col className="text-right">
            <button
              onClick={onCommitDelete}
              className="sb-link-btn text-danger"
              title="Confirm Delete"
            >
              Delete
            </button>
            <button
              onClick={onCancel}
              className="sb-link-btn text-muted"
              title="Cancel Delete"
            >
              Cancel
            </button>
          </Col>
        </Row>
      </Loader>
    </div>
  );
};
