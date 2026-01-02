import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { Nav, NavItem, NavLink, FormGroup, Row, Col } from "reactstrap";
import classnames from "classnames";

import {
  SoundbiteApiConfig,
  User,
  UserNotifications,
  UsersService,
  Utils,
} from "@soundbite/api";

import { DialogHelper } from "./DialogHelper";
import { DialogStyleType } from "./DialogStyleType";
import { DialogAction, DialogButtonStyleType } from ".";
import { ShowWhen } from "../components/controls/ShowWhen";
import { ErrorDlg } from "../components/ErrorDlg";
import { Loader } from "../components/controls/Loader";
import { WidgetStore } from "../store/WidgetStore";

interface PermissionRowProps {
  title: string;
  defaultValue: boolean;
  onChange: (newRole: boolean) => void;
}

/** A row that provides a view into a selected MemberRole */
const PermissionRow: React.FC<PermissionRowProps> = observer(
  (props: PermissionRowProps) => {
    const createTab = (
      title: string,
      value: boolean,
      onClick: () => void,
      currentMode: boolean
    ) => {
      const className = classnames(
        { active: value === currentMode },
        "sb-tab-btn"
      );

      return (
        <NavItem>
          <NavLink
            role="tab"
            aria-selected={value === currentMode}
            className={className}
            onClick={onClick}
          >
            {title}
          </NavLink>
        </NavItem>
      );
    };

    const Tabs: React.FC = () => {
      return (
        <div className="d-flex flex-md-row flex-row mt-2">
          <Nav className="sb-2-tab flex-row" role="tablist">
            {createTab(
              "Allow",
              true,
              () => {
                props.onChange(true);
              },
              props.defaultValue
            )}
            {createTab(
              "Block",
              false,
              () => {
                props.onChange(false);
              },
              props.defaultValue
            )}
          </Nav>
        </div>
      );
    };

    return (
      <tr>
        <td>{props.title}</td>
        <td className="d-flex justify-content-end pr-4">
          <Tabs />
        </td>
      </tr>
    );
  }
);

interface UserProfileProps {
  user?: User;
  showSettings?: boolean;
  onClose: () => void;
}

/** Configuration card for org permissions */
export const ProfileDialog: React.FC<UserProfileProps> = observer(
  (props: UserProfileProps) => {
    let [isActionRunning, setIsActionRunning] = React.useState(false);

    const [user, setUser] = useState<User | undefined>(props.user);

    const [isChanged, setIsChanged] = useState(false);
    const [isSaving, setIsSaving] = useState(false);

    useEffect(() => {
      const load = async () => {
        setIsChanged(false);
        if (user != null) {
          return;
        }
        const myUser = await UsersService.readMeAsync();
        setUser(myUser);
      };
      load();
    }, []);

    const onSave = async () => {
      // Must have settings; otherwise, just ignore
      if (user == null) {
        return;
      }

      setIsSaving(true);
      try {
        await UsersService.updateNotificationAsync(user);
        setIsChanged(false);
      } catch (err) {
        ErrorDlg.show(err, "Error Saving User Settings");
      }
      setIsSaving(false);
    };

    const onChangeEmail = (allow: boolean) => {
      if (user == null) {
        return;
      }
      setUser({ ...user, allowEmail: allow });
      setIsChanged(true);
    };

    const onChangeSms = (allow: boolean) => {
      if (user == null) {
        return;
      }
      setUser({ ...user, allowSms: allow });
      setIsChanged(true);
    };

    const getBody = () => {
      const mailtoLink = `mailto:${user?.email}`;
      const telLinks = `tel:${user?.phone}`;

      let avatarlUrl = SoundbiteApiConfig.imgAvatarUrl;
      if (user?.imageSrc) {
        avatarlUrl = user.imageSrc;
      }

      return (
        <div>
          <Loader isLoadedWhen={!!user} style={{ minHeight: "12rem" }}>
            <FormGroup className="rounded border p-2">
              <Row>
                <Col xs="12" sm="9">
                  <div>{Utils.userDisplay(user)}</div>
                  <ShowWhen is={!!user?.title}>
                    <div>
                      <small className="text-muted">{user?.title}</small>
                    </div>
                  </ShowWhen>
                  <ShowWhen is={!!user?.phone}>
                    <div>
                      <a href={telLinks}>{user?.phone}</a>
                    </div>
                  </ShowWhen>
                  <div>
                    <a href={mailtoLink}>{user?.email}</a>
                  </div>
                </Col>
                <Col xs="12" sm="3">
                  <div>
                    <span className="avatar rounded-circle">
                      <img
                        src={avatarlUrl}
                        className="rounded-circle"
                        alt="Avatar"
                      />
                    </span>
                  </div>
                </Col>
              </Row>
            </FormGroup>
            <ShowWhen is={!!props.showSettings}>
              <table className="table align-items-center sb-max-width-table sb-dual-column-table mt-4">
                <thead className="thead-light">
                  <tr>
                    <th
                      scope="col"
                      className="sort"
                      data-sort="name"
                      colSpan={2}
                    >
                      Notification Channels
                    </th>
                  </tr>
                </thead>
                <PermissionRow
                  title="E-Mail"
                  defaultValue={user?.allowEmail ?? true}
                  onChange={onChangeEmail}
                />
                <PermissionRow
                  title="SMS"
                  defaultValue={user?.allowSms ?? true}
                  onChange={onChangeSms}
                />
              </table>
            </ShowWhen>
          </Loader>
        </div>
      );
    };

    function getActions(): DialogAction[] {
      const ret: DialogAction[] = [
        {
          name: "Close",
          isClose: true,
          style: DialogButtonStyleType.Secondary,
        },
      ];

      if (props.showSettings) {
        ret.push({
          name: "Save",
          isClose: true,
          style: DialogButtonStyleType.Primary,
          onSelected: onSave,
          isEnabled: isChanged && !isSaving,
        });
      }

      return ret;
    }

    function getSubTitle(): string {
      return "";
    }

    const title =
      user != null ? `${Utils.userDisplay(user, true)}'s Profile` : "Profile";

    return DialogHelper.RenderDialogWithSubTitle(
      DialogStyleType.Normal,
      title,
      getSubTitle(),
      getActions(),
      getBody(),
      isActionRunning,
      setIsActionRunning,
      props.onClose
    );
  }
);
