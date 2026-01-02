/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { CSSProperties, useEffect, useRef, useState } from "react";

import { observer } from "mobx-react-lite";
import {
  Alert,
  Col,
  FormGroup,
  InputGroup,
  InputGroupAddon,
  Row,
} from "reactstrap";
import { library } from "@fortawesome/fontawesome-svg-core";
import {
  faArrowUp,
  faCircleXmark,
  faComment,
  faPlus,
  faTrash,
} from "@fortawesome/free-solid-svg-icons";
import { SbButton, SbButtonSize, SbButtonType } from "../SbButton";
import { ShowWhen } from "./ShowWhen";
import { Loader } from "./Loader";
import {
  IndexPageResponse,
  PageUtils,
  Person,
  SessionComment,
  SessionCommentsService,
  Utils,
} from "@soundbite/api";

import { ErrorDlg } from "../ErrorDlg";
import { Avatar } from "./Avatar";
import { GlobalTheme } from "../../styles";
import { SessionCommentCreate } from "./SessionCommentCreate";
import { WidgetStore } from "../../store";
import { SessionCommentEdit } from "./SessionCommentEdit";
import { SessionCommentDelete } from "./SessionCommentDelete";

/***************************************************************************************************
 *  Constants / Global Variables
 **************************************************************************************************/

library.add(faPlus, faTrash, faComment);

const getStyles = () => {
  const ret = css`
    .sb-comment-list {
      max-height: 300px;
      overflow-y: auto;
      width: 100%;

      table {
      width: 100%;
        
      tr {
        width: 100%;

        form-group {
          margin-bottom: 0;
        }
      }

      .sb-row-delete {
        border-left: 2px solid ${GlobalTheme.current.colors.bootstrap.danger};
      }
    }

    .sb-clear-comment-btn {
      border: 0 !important;
    }
    .sb-addon-btn {
      transform: translateY(0px) !important;
      box-shadow: none !important;

      &:active {
        outline: 1px;
      }

      svg {
        height: 1.25rem;
        padding: 3px 3px 0 3px;
      }
    }

    .sb-comments-table .table th,
    .sb-comments-table .table td {
      border: 0;
    }

    .sb-infinite-table {
      min-height: 150px;
      height: auto;
      max-height: 300px;
    }

    .alert .text-muted {
      color: ${GlobalTheme.current.colors.neutrals.min} !important;
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
  className?: string;
}

export enum CommentMode {
  Create = "create",
  Edit = "edit",
  Delete = "delete",
}

/** React component for showing a session's transcript */
export const SessionComments: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  // State
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [comments, setComments] = useState<SessionComment[]>([]);
  const [targetComment, setTargetComment] = useState<
    SessionComment | undefined
  >();
  const [mode, setMode] = useState<CommentMode>(CommentMode.Create);
  const [count, setCount] = useState<number | undefined>(undefined);

  // Refs
  const scrollableList = useRef<HTMLDivElement>(null);
  const actionElem = useRef<HTMLDivElement>(null);

  // Calculated
  const hasComments = count != null && count > 0;
  const currentPersonRoute =
    WidgetStore.organizations.currentOrg?.details.me?.route;

  //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

  const restoreScroll = () => {
    if (scrollableList.current) {
      const top = scrollableList.current.scrollTop;
      setTimeout(() => {
        if (scrollableList.current) {
          scrollableList.current.scrollTop = top;
        }
      });
    }
  };

  const ensureActionsInView = () => {
    if (actionElem.current) {
      actionElem.current.scrollIntoView({ behavior: "smooth" });
    }
  };

  const setCreateMode = () => {
    setTargetComment(undefined);
    setMode(CommentMode.Create);
  };

  const setEditMode = (comment: SessionComment) => {
    setMode(CommentMode.Edit);
    setTargetComment(comment);
    ensureActionsInView();
    restoreScroll();
  };

  const setDeleteMode = (comment: SessionComment) => {
    setMode(CommentMode.Delete);
    setTargetComment(comment);
    ensureActionsInView();
    restoreScroll();
  };

  const loadComments = (page: IndexPageResponse<SessionComment>) => {
    setComments(page.result);
    scrollToBottom();
  };

  const load = async () => {
    if (props.orgRoute == null || props.sessionRoute == null) {
      return;
    }

    try {
      setIsLoading(true);

      const firstCommentsPage =
        await SessionCommentsService.readAllSessionComments(
          props.orgRoute,
          props.sessionRoute,
          { includesCounts: true }
        );
      loadComments(firstCommentsPage);
      setCount(firstCommentsPage.totalCount);
    } catch (err) {
      ErrorDlg.show(err, "Error Loading Session Comments");
    }
    setIsLoading(false);
  };

  function scrollToBottom() {
    setTimeout(() => {
      if (scrollableList.current) {
        const scrollHeight = scrollableList.current.scrollHeight;
        scrollableList.current.scrollTop = scrollHeight;
      }
    });
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  useEffect(() => {
    const firstLoad = () => {
      load();
    };
    firstLoad();
  }, []);

  useEffect(() => {}, [targetComment]);

  // Whenever comments is set, scroll to the bottom of the list
  useEffect(() => {
    scrollToBottom();
  }, [comments]);

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  function onEdit(comment: SessionComment) {
    setEditMode(comment);
  }

  function onDelete(comment: SessionComment) {
    setDeleteMode(comment);
  }

  async function onCommit(): Promise<void> {
    await load();
    setCreateMode();
  }

  function onCancel(): void {
    setCreateMode();
  }

  function commentRowContent(
    comment: SessionComment,
    person: Person,
    isActionable: boolean = true
  ) {
    return (
      <React.Fragment>
        <span className="font-weight-bold pr-1">
          {Utils.userDisplay(person.user)}
        </span>
        {comment.content}
        <div
          className="sb-comment-subheader text-muted text-sm"
          title={Utils.formatDateTime(comment.createdUtc)}
        >
          {Utils.formatRelativeDate(comment.createdUtc)}
          <ShowWhen
            is={currentPersonRoute === comment.person.route && isActionable}
          >
            <button
              onClick={() => onEdit(comment)}
              className="sb-link-btn text-muted"
              title="Edit Comment"
            >
              Edit
            </button>
            <button
              onClick={() => onDelete(comment)}
              className="sb-link-btn text-muted"
              title="Delete Comment"
            >
              Delete
            </button>
          </ShowWhen>
        </div>
      </React.Fragment>
    );
  }

  function wrappedCommentRowContent(comment: SessionComment, person: Person) {
    const isCurrent = comment.route === targetComment?.route;
    const isEdit = mode === CommentMode.Edit;
    const isDelete = mode === CommentMode.Delete;

    if (isCurrent && (isEdit || isDelete)) {
      return (
        <Alert color={isEdit ? "warning" : "danger"}>
          {commentRowContent(comment, person, false)}
        </Alert>
      );
    } else {
      return commentRowContent(comment, person);
    }
  }

  function commentsRow(comment: SessionComment, index: number) {
    const person = comment?.person;
    return (
      <tr key={comment.route}>
        <th scope="row">
          <Avatar user={person.user} showName={false} sizingClassName="" />
        </th>
        <td className="">{wrappedCommentRowContent(comment, person)}</td>
      </tr>
    );
  }
  const Comments: React.FC = () => {
    return (
      <table>
        <tbody>
          {comments.map((comment, index) => {
            return commentsRow(comment, index);
          })}
        </tbody>
      </table>
    );
  };

  const List: React.FC = () => {
    return (
      <div className="sb-comment-list pb-2" ref={scrollableList}>
        <ShowWhen is={!hasComments}>
          <span className="text-muted">Be The First To Comment</span>
        </ShowWhen>
        <ShowWhen is={hasComments}>
          <Comments />
        </ShowWhen>
      </div>
    );
  };

  // Setting up render state
  if (styles == null) {
    styles = getStyles();
  }

  const commentClass = "rounded border-1";
  const combinedClass =
    props.className != null
      ? `${props.className} ${commentClass}`
      : commentClass;

  return (
    <div className={combinedClass} css={styles}>
      <h4>Comments</h4>
      <Loader isLoadedWhen={!isLoading} isDisplayBased={true}>
        <List />
        <div ref={actionElem}>
          <ShowWhen is={mode === CommentMode.Create}>
            <SessionCommentCreate
              orgRoute={props.orgRoute}
              sessionRoute={props.sessionRoute}
              onCommit={onCommit}
              maxCharacters={150}
            />
          </ShowWhen>
          <ShowWhen is={mode === CommentMode.Edit}>
            <SessionCommentEdit
              orgRoute={props.orgRoute}
              sessionRoute={props.sessionRoute}
              comment={targetComment}
              onCommit={onCommit}
              onCancel={onCancel}
              maxCharacters={150}
            />
          </ShowWhen>
          <ShowWhen is={mode === CommentMode.Delete}>
            <SessionCommentDelete
              orgRoute={props.orgRoute}
              sessionRoute={props.sessionRoute}
              comment={targetComment}
              onCommit={onCommit}
              onCancel={onCancel}
            />
          </ShowWhen>
        </div>
      </Loader>
    </div>
  );
});
