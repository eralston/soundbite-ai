import React from "react";
import { observer } from "mobx-react-lite";
import { NavLink as RouterNavLink } from "react-router-dom";

import { Invite, SoundbiteApiConfig, UserRole, Utils } from "@soundbite/api";
import {
  ShowWhen,
  Loader,
  WidgetStore,
  DialogManager,
} from "@soundbite/widgets-react";

import InviteBtn from "../controls/InviteBtn";
import SpaStore from "../../store/Spa.Store";

export default observer(() => {
  const openProfileDialog = async () => {
    DialogManager.ShowProfileDialog();
  };

  const logout = async () => {
    await SpaStore.logout();
  };

  const onInviteAsync = async (emails: string[]): Promise<void> => {
    const invites = new Array<Invite>();
    for (const eml of emails) {
      invites.push({ token: eml });
    }
    await WidgetStore.users.inviteUsersAsync(invites);
  };

  let avatarlUrl = SoundbiteApiConfig.imgAvatarUrl;

  if (WidgetStore.users.currentUser?.imageSrc) {
    avatarlUrl = WidgetStore.users.currentUser.imageSrc;
  }

  return (
    <li className="nav-item dropdown">
      <Loader
        isLoadedWhen={!!WidgetStore.users.currentUser}
        className="text-secondary sb-avatar-loader"
      >
        <span
          className="nav-link pr-0"
          role="button"
          data-toggle="dropdown"
          aria-haspopup="true"
          aria-expanded="false"
          title="User Menu"
        >
          <div className="media align-items-center">
            <div className="media-body mr-2 d-none d-md-block">
              <span className="mb-0 text-sm font-weight-bold">
                {Utils.userDisplay(WidgetStore.users.currentUser)}
              </span>
            </div>
            <span className="avatar avatar-sm rounded-circle">
              <img alt="Avatar" src={avatarlUrl} />
            </span>
          </div>
        </span>
        <div className="dropdown-menu  dropdown-menu-right">
          <span
            className="dropdown-item"
            onClick={openProfileDialog}
            title="My Profile"
          >
            <i className="fas fa-user"></i>
            <span>My Profile</span>
          </span>
          <ShowWhen
            is={
              WidgetStore.organizations.myOrgs &&
              (WidgetStore.organizations.myOrgs?.length ?? 0) > 1
            }
          >
            <RouterNavLink
              activeClassName="none"
              className="dropdown-item"
              to={`/`}
              title="Home"
            >
              <i className="fas fa-home"></i>
              <span>Home</span>
            </RouterNavLink>
          </ShowWhen>
          <ShowWhen
            is={WidgetStore.users.currentUser?.userRole === UserRole.God}
          >
            <RouterNavLink
              activeClassName="none"
              className="dropdown-item"
              to={`/admin`}
              title="Admin"
            >
              <i className="fas fa-toolbox"></i>
              <span>Admin</span>
            </RouterNavLink>
          </ShowWhen>
          <a
            href="https://soundbite.ai/"
            target="_blank"
            rel="noopener noreferrer"
            className="dropdown-item"
            title="Support"
          >
            <i className="fas fa-life-ring"></i>
            <span>Support</span>
          </a>
          <ShowWhen is={SpaStore.config.featureFlags.referralEnabled}>
            <InviteBtn onSubmitAsync={onInviteAsync}>
              <span className="dropdown-item">
                <i className="fas fa-share-alt"></i>
                <span>Share</span>
              </span>
            </InviteBtn>
          </ShowWhen>
          <div className="dropdown-divider"></div>
          <div onClick={logout} className="dropdown-item" title="Logout">
            <i className="fas fa-sign-out-alt"></i>
            <span>Logout</span>
          </div>
        </div>
      </Loader>
    </li>
  );
});
