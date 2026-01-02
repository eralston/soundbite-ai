///** @jsx jsx */
//import { jsx, css, SerializedStyles } from "@emotion/react";
//import React, { CSSProperties, useEffect, useRef, useState } from "react";

//import { observer } from "mobx-react-lite";
//import { FormGroup, InputGroup, InputGroupAddon } from "reactstrap";
//import { library } from "@fortawesome/fontawesome-svg-core";
//import {
//  faArrowUp,
//  faComment,
//  faPlus,
//  faTrash,
//} from "@fortawesome/free-solid-svg-icons";
//import { SbButton, SbButtonSize, SbButtonType } from "../SbButton";
//import { ShowWhen } from "./ShowWhen";
//import { Loader } from "./Loader";
//import {
//  IndexPageResponse,
//  PageUtils,
//  SessionComment,
//  SessionCommentsService,
//  Utils,
//} from "@soundbite/api";
//import { ErrorDlg } from "../ErrorDlg";
//import { Avatar } from "./Avatar";

///***************************************************************************************************
// *  Constants / Global Variables
// **************************************************************************************************/

//library.add(faPlus, faTrash, faComment);

//const getStyles = () => {
//  const ret = css`
//    .sb-clear-comment-btn {
//      border: 0 !important;
//    }
//    .sb-addon-btn {
//      transform: translateY(0px) !important;
//      box-shadow: none !important;

//      &:active {
//        outline: 1px;
//      }

//      svg {
//        height: 1.25rem;
//        padding: 3px 3px 0 3px;
//      }
//    }

//    .sb-comments-table .table th,
//    .sb-comments-table .table td {
//      border: 0;
//    }

//    .sb-infinite-table {
//      min-height: 150px;
//      height: auto;
//      max-height: 300px;
//    }
//  `;
//  return ret;
//};

//let styles: SerializedStyles | undefined = undefined;

///***************************************************************************************************
// *  Component Properties Interface
// **************************************************************************************************/
//interface IProps {
//  orgRoute?: string;
//  sessionRoute?: string;
//  className?: string;
//}

///** React component for showing a session's transcript */
//export const InfiniteSessionComments: React.FC<IProps> = observer((props: IProps) => {
//  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

//  const [isSaving, setIsSaving] = useState<boolean>(false);
//  const [isLoading, setIsLoading] = useState<boolean>(true);
//  const [newComment, setNewComment] = useState<string>("");

//  // Infinite comments table

//  const comments = useRef<Map<number, SessionComment | null>>(
//    new Map<number, SessionComment | null>()
//  );
//  const maxTake = useRef<number>(128);
//  const maxTotalItems = useRef<number>(2147483647);
//  const [count, setCount] = useState<number | undefined>(undefined);
//  const hasComments = count != null && count > 0;

//  //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

//  const setComments = (page: IndexPageResponse<SessionComment>) => {
//    PageUtils.addPageToMap(page, comments.current);
//  };

//  async function load() {
//    console.log("Loading org and session", props.orgRoute, props.sessionRoute);

//    if (props.orgRoute == null || props.sessionRoute == null) {
//      return;
//    }

//    // initial load
//    setIsLoading(true);

//    // Loading logic

//    const firstCommentsPage =
//      await SessionCommentsService.readAllSessionComments(
//        props.orgRoute,
//        props.sessionRoute,
//        { includesCounts: true }
//      );
//    console.log(
//      "Received first comments page:",
//      JSON.stringify(firstCommentsPage, null, 2)
//    );
//    setComments(firstCommentsPage);
//    setCount(firstCommentsPage.totalCount);
//    maxTake.current = firstCommentsPage.maxTake;

//    setIsLoading(false);
//  }

//  //const scrollData: InfiniteScrollDataHelper = new InfiniteScrollDataHelper(
//  //  async (startIndex: number, stopIndex: number): Promise<void> => {
//  //    return new Promise<void>(async (resolve, reject) => {
//  //      try {
//  //        if (props.orgRoute == null || props.sessionRoute == null) {
//  //          resolve();
//  //          return;
//  //        }

//  //        // Indicate
//  //        for (let index = startIndex; index <= stopIndex; index++) {
//  //          comments.current.set(index, null);
//  //        }
//  //        const page = await SessionCommentsService.readAllSessionComments(
//  //          props.orgRoute,
//  //          props.sessionRoute,
//  //          {
//  //            skip: startIndex - 1,
//  //            take: maxTake.current,
//  //          }
//  //        );

//  //        PageUtils.addPageToMap(page, comments.current);

//  //        resolve();
//  //      } catch (err) {
//  //        ErrorDlg.show(
//  //          err,
//  //          "Error Loading Session Comments",
//  //          `Error querying for session comments ${startIndex} to ${stopIndex}:`
//  //        );
//  //        reject();
//  //      }
//  //    });
//  //  }
//  //);

//  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

//  useEffect(() => {
//    const firstLoad = () => {
//      load();
//    };
//    firstLoad();
//  }, []);

//  function onAddComment() {
//    alert("Add comment logic");
//  }

//  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

//  function CommentRow({
//    context,
//    index,
//    comment,
//    style,
//  }: {
//    context: InfiniteListContext;
//    index: number;
//    style: CSSProperties;
//    comment?: SessionComment;
//  }) {
//    if (comment == null) {
//      return <tr data-row-index={index}>Error Loading Row {index}</tr>;
//    }

//    const person = comment?.person;
//    return (
//      <InfiniteListItem context={context} index={index} style={style}>
//        <th scope="row">
//          <Avatar user={person.user} showName={false} sizingClassName="" />
//        </th>
//        <td className="">
//          <span className="font-weight-bold pr-1">
//            {Utils.userDisplay(person.user)}
//          </span>
//          {comment.content}
//        </td>
//      </InfiniteListItem>
//    );
//  }

//  function LoadingRow({
//    context,
//    index,
//    style,
//  }: {
//    context: InfiniteListContext;
//    index: number;
//    style: CSSProperties;
//  }) {
//    return (
//      <InfiniteListItem context={context} index={index} style={style}>
//        <th scope="row" className="text-muted">
//          Loading {index != null ? `${index}...` : "..."}
//        </th>
//        <td className="d-none d-lg-table-cell"></td>
//        <td></td>
//        <td></td>
//      </InfiniteListItem>
//    );
//  }

//  function LastRow({
//    context,
//    index,
//    style,
//  }: {
//    context: InfiniteListContext;
//    index: number;
//    style: CSSProperties;
//  }) {
//    return (
//      <InfiniteListItem context={context} index={index} style={style}>
//        <th scope="row" className="text-muted">
//          Cannot load more than {maxTotalItems.current} at this time
//        </th>
//        <td className="d-none d-lg-table-cell"></td>
//        <td></td>
//        <td></td>
//      </InfiniteListItem>
//    );
//  }

//  function tableRowGetter(
//    context: InfiniteListContext
//  ): InfiniteListItemGetter {
//    return ({ index, style }: { index: number; style: CSSProperties }) => {
//      const comment = comments.current.get(index);
//      if (index >= maxTotalItems.current) {
//        return <LastRow context={context} index={index} style={style} />;
//      } else if (comment == null) {
//        return <LoadingRow context={context} index={index} style={style} />;
//      } else {
//        return (
//          <CommentRow
//            context={context}
//            index={index}
//            style={style}
//            comment={comment}
//          />
//        );
//      }
//    };
//  }

//  const Comments: React.FC = () => {
//    // Every row is loaded except for our loading indicator row.
//    const isItemLoaded = (index: number) => {
//      const person = comments.current.get(index);
//      return person != null;
//    };

//    let itemCount = count ?? 0;
//    if (itemCount > maxTotalItems.current) {
//      itemCount = maxTotalItems.current + 1;
//    }

//    return <ShowWhen is={count != null}>Comments loaded here</ShowWhen>;
//  };

//  // Setting up render state
//  if (styles == null) {
//    styles = getStyles();
//  }

//  const commentClass = "rounded border-1";
//  const combinedClass =
//    props.className != null
//      ? `${props.className} ${commentClass}`
//      : commentClass;

//  return (
//    <div className={combinedClass} css={styles}>
//      <Loader isLoadedWhen={!isLoading}>
//        <div className="sb-comment-list pt-2 pb-2">
//          <ShowWhen is={!hasComments}>
//            <span className="text-muted">Be The First To Comment</span>
//          </ShowWhen>
//          <ShowWhen is={hasComments}>
//            <Comments />
//          </ShowWhen>
//        </div>
//        <div className="sb-comment-create">
//          <FormGroup>
//            <InputGroup aria-disabled={isSaving}>
//              <input
//                type="text"
//                className="form-control"
//                placeholder="Add Comment..."
//                aria-label="Title"
//                aria-describedby="basic-addon1"
//                onChange={(e) => setNewComment(e.target.value)}
//                disabled={isSaving}
//                title="Add Comment"
//                value={newComment}
//              />
//              <InputGroupAddon
//                className={"sb-addon-btn sb-add-comment-btn-addon"}
//                addonType="append"
//              >
//                <SbButton
//                  type={SbButtonType.Primary}
//                  size={SbButtonSize.Small}
//                  btnClassName="sb-addon-btn sb-add-comment-btn"
//                  title="Add Comment"
//                  onClick={onAddComment}
//                  icon={faArrowUp}
//                />
//              </InputGroupAddon>
//            </InputGroup>
//          </FormGroup>
//        </div>
//      </Loader>
//    </div>
//  );
//});
